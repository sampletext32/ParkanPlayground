using System.Numerics;
using MshLib;
using NResLib;
using NResUI.Rendering.Viewport.Meshes;
using Silk.NET.OpenGL;

namespace NResUI.Rendering.Viewport.Msh;

public static class MshViewportLoader
{
    private const int DefaultModelState = 0;
    private const int DefaultLod = 0;

    public static MshViewportLoadResult LoadFromFile(GL gl, string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var parseResult = NResParser.ReadFile(fs);
            if (parseResult.Archive == null)
                return MshViewportLoadResult.Failure(parseResult.Error ?? "Failed to parse MSH/NRes file.", path);

            var archive = parseResult.Archive;
            var meshType = MshConverter.DetectMeshType(archive);
            if (meshType != MshType.Model)
            {
                return MshViewportLoadResult.Failure(
                    $"Only normal MSH models are supported for this viewport pass. Detected: {meshType}.",
                    path);
            }

            fs.Seek(0, SeekOrigin.Begin);
            var pieces = LoadModelPieces(gl, fs, archive);
            if (pieces.Count == 0)
                return MshViewportLoadResult.Failure("MSH was parsed, but no renderable geometry pieces were found.", path);

            return MshViewportLoadResult.Success(path, pieces);
        }
        catch (Exception ex)
        {
            return MshViewportLoadResult.Failure(ex.Message, path);
        }
    }

    private static List<ViewportPiece> LoadModelPieces(GL gl, FileStream fs, NResArchive archive)
    {
        var nodes = Msh0x01.ReadComponent(fs, archive);
        var geometry = Msh0x02.ReadComponent(fs, archive);
        var positions = Msh0x03.ReadComponent(fs, archive);
        var indices = Msh0x06.ReadComponent(fs, archive);
        var batches = Msh0x0D.ReadComponent(fs, archive);
        var animationDescriptors = Msh0x08.ReadComponent(fs, archive);
        var restPoses = MshRestPoseBuilder.BuildRestPose(nodes, animationDescriptors);
        var mshToViewportTransform = Matrix4x4.CreateRotationX(-MathF.PI * 0.5f);
        var names = TryReadNames(fs, archive);

        var pieces = new List<ViewportPiece>();

        for (var nodeIndex = 0; nodeIndex < nodes.Nodes.Count; nodeIndex++)
        {
            var node = nodes.Nodes[nodeIndex];
            var restPose = restPoses[nodeIndex];
            var slotIndex = node.ResolveSlotIndex(DefaultModelState, DefaultLod);
            if (slotIndex == ushort.MaxValue)
                continue;

            if (slotIndex >= geometry.Slots.Count)
                continue;

            var slot = geometry.Slots[slotIndex];
            if (!slot.HasBatches)
                continue;

            var meshBuildResult = BuildPieceMesh(gl, nodeIndex, slot, positions, indices, batches);
            if (meshBuildResult == null)
                continue;

            var name = ResolvePieceName(names, nodeIndex);
            pieces.Add(new ViewportPiece(
                id: nodeIndex,
                name: name,
                mesh: meshBuildResult.Mesh,
                localTransform: restPose.MeshSpaceTransform * mshToViewportTransform,
                boundsMin: meshBuildResult.BoundsMin,
                boundsMax: meshBuildResult.BoundsMax,
                debugInfo: new ViewportPieceDebugInfo
                {
                    SourceKind = "MSH 0x01 piece",
                    SourcePieceIndex = nodeIndex,
                    SourceParentIndex = restPose.ParentIndex,
                    GeometrySlotIndex = slotIndex,
                    Msh01Flags = (uint)node.Flags,
                    BatchCount = meshBuildResult.BatchCount,
                    TriangleCount = meshBuildResult.TriangleCount,
                    FallbackKeyframeIndex = restPose.FallbackKeyframeIndex,
                    HasRestPose = restPose.HasFallbackPose
                }));
        }

        return pieces;
    }

    private static PieceMeshBuildResult? BuildPieceMesh(
        GL gl,
        int nodeIndex,
        Msh0x02.GeometrySlot slot,
        IReadOnlyList<Common.Vector3> positions,
        IReadOnlyList<ushort> indices,
        IReadOnlyList<Msh0x0D.Batch> batches)
    {
        var vertices = new List<float>();
        var outIndices = new List<uint>();
        var color = PickDebugColor(nodeIndex);

        var batchCount = 0;
        var triangleCount = 0;

        var boundsMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var boundsMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (var batchIndex = slot.BatchStart0x0D; batchIndex < slot.BatchEndExclusive0x0D; batchIndex++)
        {
            if (batchIndex < 0 || batchIndex >= batches.Count)
                continue;

            var batch = batches[batchIndex];
            if (batch.IndexStart0x06 + batch.IndexCount0x06 > indices.Count)
                continue;

            batchCount++;

            for (var i = 0; i + 2 < batch.IndexCount0x06; i += 3)
            {
                var indexBase = (int)batch.IndexStart0x06 + i;
                var vertexIndex0 = checked((int)batch.BaseVertex0x03 + indices[indexBase + 0]);
                var vertexIndex1 = checked((int)batch.BaseVertex0x03 + indices[indexBase + 1]);
                var vertexIndex2 = checked((int)batch.BaseVertex0x03 + indices[indexBase + 2]);

                if (!IsValidTriangle(positions.Count, vertexIndex0, vertexIndex1, vertexIndex2))
                    continue;

                var p0 = ToNumericsVector3(positions[vertexIndex0]);
                var p1 = ToNumericsVector3(positions[vertexIndex1]);
                var p2 = ToNumericsVector3(positions[vertexIndex2]);

                var normal = Vector3.Cross(p1 - p0, p2 - p0);
                if (normal.LengthSquared() < 1e-8f)
                    normal = Vector3.UnitY;
                else
                    normal = Vector3.Normalize(normal);

                AddTriangleVertex(p0, color, normal, vertices, outIndices, ref boundsMin, ref boundsMax);
                AddTriangleVertex(p1, color, normal, vertices, outIndices, ref boundsMin, ref boundsMax);
                AddTriangleVertex(p2, color, normal, vertices, outIndices, ref boundsMin, ref boundsMax);
                triangleCount++;
            }
        }

        if (triangleCount == 0)
            return null;

        var mesh = PrimitiveMeshes.CreateColoredIndexedMesh(gl, vertices, outIndices, PrimitiveType.Triangles);
        return new PieceMeshBuildResult(mesh, boundsMin, boundsMax, batchCount, triangleCount);
    }
    
    private static Vector3 ToNumericsVector3(Common.Vector3 position)
    {
        return new Vector3(position.X, position.Y, position.Z);
    }

    private static void AddTriangleVertex(
        Vector3 position,
        Vector3 color,
        Vector3 normal,
        List<float> vertices,
        List<uint> indices,
        ref Vector3 boundsMin,
        ref Vector3 boundsMax
    )
    {
        var vertexIndex = (uint)(vertices.Count / 6);

        vertices.Add(position.X);
        vertices.Add(position.Y);
        vertices.Add(position.Z);
        vertices.Add(color.X);
        vertices.Add(color.Y);
        vertices.Add(color.Z);
        vertices.Add(normal.X);
        vertices.Add(normal.Y);
        vertices.Add(normal.Z);

        indices.Add(vertexIndex);

        var p = new Vector3(position.X, position.Y, position.Z);
        boundsMin = Vector3.Min(boundsMin, p);
        boundsMax = Vector3.Max(boundsMax, p);
    }

    private static bool IsValidTriangle(int vertexCount, int vertexIndex0, int vertexIndex1, int vertexIndex2)
    {
        return IsValidVertexIndex(vertexCount, vertexIndex0) &&
               IsValidVertexIndex(vertexCount, vertexIndex1) &&
               IsValidVertexIndex(vertexCount, vertexIndex2);
    }

    private static bool IsValidVertexIndex(int vertexCount, int vertexIndex)
    {
        return vertexIndex >= 0 && vertexIndex < vertexCount;
    }

    private static List<string> TryReadNames(FileStream fs, NResArchive archive)
    {
        try
        {
            return Msh0x0A.ReadComponent(fs, archive);
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string ResolvePieceName(IReadOnlyList<string> names, int nodeIndex)
    {
        if (nodeIndex >= 0 && nodeIndex < names.Count && !string.IsNullOrWhiteSpace(names[nodeIndex]))
            return $"{nodeIndex}: {names[nodeIndex]}";

        return $"{nodeIndex}: piece_{nodeIndex:D3}";
    }

    private static Vector3 PickDebugColor(int index)
    {
        ReadOnlySpan<Vector3> palette =
        [
            new Vector3(0.90f, 0.36f, 0.30f),
            new Vector3(0.30f, 0.68f, 0.95f),
            new Vector3(0.42f, 0.82f, 0.42f),
            new Vector3(0.95f, 0.78f, 0.32f),
            new Vector3(0.78f, 0.48f, 0.95f),
            new Vector3(0.36f, 0.86f, 0.78f),
            new Vector3(0.95f, 0.55f, 0.78f),
            new Vector3(0.72f, 0.72f, 0.72f),
        ];

        return palette[index % palette.Length];
    }

    private sealed record PieceMeshBuildResult(
        GpuMesh Mesh,
        Vector3 BoundsMin,
        Vector3 BoundsMax,
        int BatchCount,
        int TriangleCount);
}
