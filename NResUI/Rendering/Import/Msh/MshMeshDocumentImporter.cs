using System.Numerics;
using MshLib;
using NResLib;
using NResUI.Rendering.Inspection;
using NResUI.Rendering.Materials;
using NResUI.Rendering.Viewport.Msh;

namespace NResUI.Rendering.Import.Msh;

public static class MshMeshDocumentImporter
{
    private const int DefaultModelState = 0;
    private const int DefaultLod = 0;

    /// <summary>
    /// Читает MSH/NRes с диска и строит CPU-документ для инспектора.
    /// OpenGL здесь намеренно не используется: этот слой должен быть проверяемым без viewport.
    /// </summary>
    public static MeshDocumentLoadResult LoadFromFile(string path)
    {
        var warnings = new List<string>();

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var parseResult = NResParser.ReadFile(fs);
            if (parseResult.Archive == null)
                return MeshDocumentLoadResult.Failure(parseResult.Error ?? "Failed to parse MSH/NRes file.", path);

            var archive = parseResult.Archive;
            var meshType = MshConverter.DetectMeshType(archive);
            if (meshType != MshType.Model)
            {
                return MeshDocumentLoadResult.Failure(
                    $"Only normal MSH models are supported by the mesh inspector. Detected: {meshType}.",
                    path);
            }

            fs.Seek(0, SeekOrigin.Begin);
            var document = LoadModelDocument(fs, archive, path, warnings);
            return MeshDocumentLoadResult.Success(document);
        }
        catch (Exception ex)
        {
            return MeshDocumentLoadResult.Failure(ex.Message, path);
        }
    }

    private static MeshDocument LoadModelDocument(
        FileStream fs,
        NResArchive archive,
        string path,
        List<string> warnings)
    {
        var nodes = Msh0x01.ReadComponent(fs, archive);
        var geometry = Msh0x02.ReadComponent(fs, archive);
        var positions = Msh0x03.ReadComponent(fs, archive);
        var normals = TryReadNormals(fs, archive, warnings);
        var uvs = TryReadUvs(fs, archive, warnings);
        var indices = Msh0x06.ReadComponent(fs, archive);
        var batches = Msh0x0D.ReadComponent(fs, archive);
        var animationDescriptors = Msh0x08.ReadComponent(fs, archive);
        var restPoses = MshRestPoseBuilder.BuildRestPose(nodes, animationDescriptors);
        var names = TryReadNames(fs, archive, warnings);

        var weaMatch = WeaResourceResolver.TryLoadMatchingWeaForMsh(path, warnings);
        if (weaMatch != null)
            warnings.Add($"Loaded WEA '{Path.GetFileName(weaMatch.Path)}' ({weaMatch.Reason})");

        var materialNamesById = weaMatch?.File.Materials.ToDictionary(x => x.Id, x => x.Name)
            ?? new Dictionary<int, string>();

        var pieces = new List<MeshPieceInfo>(nodes.Nodes.Count);
        for (var nodeIndex = 0; nodeIndex < nodes.Nodes.Count; nodeIndex++)
        {
            var node = nodes.Nodes[nodeIndex];
            var restPose = restPoses[nodeIndex];
            var activeSlotIndex = node.ResolveSlotIndex(DefaultModelState, DefaultLod);
            var boundsMin = Vector3.Zero;
            var boundsMax = Vector3.Zero;

            if (activeSlotIndex != ushort.MaxValue && activeSlotIndex < geometry.Slots.Count)
            {
                var slot = geometry.Slots[activeSlotIndex];
                boundsMin = ToNumericsVector3(slot.LocalMinimum);
                boundsMax = ToNumericsVector3(slot.LocalMaximum);
            }

            pieces.Add(new MeshPieceInfo
            {
                Id = nodeIndex,
                Name = ResolvePieceName(names, nodeIndex),
                ParentId = restPose.ParentIndex,
                Flags = node.Flags,
                GeometrySlotsByStateAndLod = node.Msh02SlotIndicesByStateAndLOD.ToArray(),
                Batches = BuildPieceBatchInfo(node, geometry, batches, indices.Count, materialNamesById, warnings),
                LocalTransform = restPose.LocalTransform,
                MeshSpaceTransform = restPose.MeshSpaceTransform,
                BoundsMin = boundsMin,
                BoundsMax = boundsMax,
                FallbackKeyframeIndex = restPose.FallbackKeyframeIndex,
                HasRestPose = restPose.HasFallbackPose
            });
        }

        return new MeshDocument
        {
            SourcePath = path,
            MeshType = MshType.Model,
            Pieces = pieces,
            Materials = materialNamesById
                .Select(x => new MeshMaterialInfo { Id = x.Key, Name = x.Value })
                .OrderBy(x => x.Id)
                .ToList(),
            Warnings = warnings,
            MshModelGeometry = new MshModelGeometry
            {
                Nodes = nodes,
                GeometrySlots = geometry,
                Positions = positions,
                Normals = normals,
                Uvs = uvs,
                Indices = indices,
                Batches = batches
            }
        };
    }

    /// <summary>
    /// Собирает batch metadata для всех state/LOD слотов piece.
    /// Дубликаты batch убираются, потому что разные состояния могут ссылаться на один и тот же geometry slot.
    /// </summary>
    private static IReadOnlyList<MeshBatchInfo> BuildPieceBatchInfo(
        Msh0x01.Node node,
        Msh0x02.Msh0x02Component geometry,
        IReadOnlyList<Msh0x0D.Batch> batches,
        int indexCount,
        IReadOnlyDictionary<int, string> materialNamesById,
        ICollection<string> documentWarnings)
    {
        var result = new List<MeshBatchInfo>();
        var seenBatchIndices = new HashSet<int>();

        foreach (var slotIndex in node.Msh02SlotIndicesByStateAndLOD.Distinct())
        {
            if (slotIndex == ushort.MaxValue)
                continue;

            if (slotIndex >= geometry.Slots.Count)
            {
                documentWarnings.Add($"Geometry slot {slotIndex} is out of range.");
                continue;
            }

            var slot = geometry.Slots[slotIndex];
            for (var batchIndex = slot.BatchStart0x0D; batchIndex < slot.BatchEndExclusive0x0D; batchIndex++)
            {
                if (batchIndex < 0 || batchIndex >= batches.Count || !seenBatchIndices.Add(batchIndex))
                    continue;

                var batch = batches[batchIndex];
                var warnings = new List<string>();
                if (batch.IndexStart0x06 + batch.IndexCount0x06 > indexCount)
                    warnings.Add("Index range exceeds MSH 0x06 index buffer.");

                var materialName = materialNamesById.GetValueOrDefault(batch.MaterialIndexLo);

                if (materialName == "B_PLT_06")
                {
                    _ = 5;
                }
                
                result.Add(new MeshBatchInfo
                {
                    BatchIndex = batchIndex,
                    MaterialId = batch.MaterialIndexLo,
                    MaterialName = materialName,
                    Flags = batch.Flags,
                    IndexStart = batch.IndexStart0x06,
                    IndexCount = batch.IndexCount0x06,
                    BaseVertex = batch.BaseVertex0x03,
                    VertexCount = batch.VertexCount0x03,
                    TriangleCount = batch.IndexCount0x06 / 3,
                    Warnings = warnings
                });
            }
        }

        return result;
    }

    private static List<string> TryReadNames(FileStream fs, NResArchive archive, ICollection<string> warnings)
    {
        try
        {
            return Msh0x0A.ReadComponent(fs, archive);
        }
        catch (Exception ex)
        {
            warnings.Add($"MSH 0x0A names are unavailable: {ex.Message}");
            return [];
        }
    }

    private static List<Msh04Normal> TryReadNormals(FileStream fs, NResArchive archive, ICollection<string> warnings)
    {
        try
        {
            return Msh0x04.ReadComponent(fs, archive);
        }
        catch (Exception ex)
        {
            warnings.Add($"MSH 0x04 normals are unavailable: {ex.Message}");
            return [];
        }
    }

    private static List<Msh05Uv> TryReadUvs(FileStream fs, NResArchive archive, ICollection<string> warnings)
    {
        try
        {
            return Msh0x05.ReadComponent(fs, archive);
        }
        catch (Exception ex)
        {
            warnings.Add($"MSH 0x05 UVs are unavailable: {ex.Message}");
            return [];
        }
    }

    private static string ResolvePieceName(IReadOnlyList<string> names, int nodeIndex)
    {
        if (nodeIndex >= 0 && nodeIndex < names.Count && !string.IsNullOrWhiteSpace(names[nodeIndex]))
            return $"{nodeIndex}: {names[nodeIndex]}";

        return $"{nodeIndex}: piece_{nodeIndex:D3}";
    }

    private static Vector3 ToNumericsVector3(Common.Vector3 position)
    {
        return new Vector3(position.X, position.Y, position.Z);
    }
}

public sealed class MeshDocumentLoadResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? SourcePath { get; }
    public MeshDocument? Document { get; }

    private MeshDocumentLoadResult(bool isSuccess, string? error, string? sourcePath, MeshDocument? document)
    {
        IsSuccess = isSuccess;
        Error = error;
        SourcePath = sourcePath;
        Document = document;
    }

    public static MeshDocumentLoadResult Success(MeshDocument document)
    {
        return new MeshDocumentLoadResult(true, null, document.SourcePath, document);
    }

    public static MeshDocumentLoadResult Failure(string error, string? sourcePath = null)
    {
        return new MeshDocumentLoadResult(false, error, sourcePath, null);
    }
}
