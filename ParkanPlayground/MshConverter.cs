using System.Text;
using Common;
using NResLib;

namespace ParkanPlayground;

public enum MshType
{
    Unknown,
    /// <summary>
    /// Для обычной модели минимальный геометрический путь сейчас выглядит так:
    /// 0x01 node
    ///   -> 0x02 geometry slot
    ///     -> 0x0D batch
    ///       -> 0x06 indices
    ///         -> 0x03 positions
    /// </summary>
    Model,
    Landscape
}

public sealed class MshConverter
{
    public void Convert(string mshPath, string? outputPath = null, int lod = 0, int group = 0)
    {
        var result = NResParser.ReadFile(mshPath);
        if (result.Archive is null)
        {
            Console.WriteLine($"ERROR: Failed to read NRes archive: {result.Error}");
            return;
        }

        var archive = result.Archive;
        var meshType = DetectMeshType(archive);

        outputPath ??= Path.ChangeExtension(mshPath, ".obj");

        Console.WriteLine($"Converting: {Path.GetFileName(mshPath)}");
        Console.WriteLine($"Detected type: {meshType}");
        Console.WriteLine($"LOD: {lod}, group: {group}");

        using var fs = new FileStream(mshPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        switch (meshType)
        {
            case MshType.Model:
                ConvertModel(fs, archive, outputPath, lod, group);
                break;

            case MshType.Landscape:
                ConvertLandscape(fs, archive, outputPath, lod, group);
                break;

            default:
                Console.WriteLine("ERROR: Unknown or unsupported MSH type.");
                break;
        }
    }

    public static MshType DetectMeshType(NResArchive archive)
    {
        var has03 = HasComponent(archive, "03");
        var has06 = HasComponent(archive, "06");
        var has0D = HasComponent(archive, "0D");
        var has15 = HasComponent(archive, "15");

        if (has03 && has06 && has0D)
        {
            return MshType.Model;
        }

        if (has03 && has15 && !has06)
        {
            return MshType.Landscape;
        }

        return MshType.Unknown;
    }

    private static bool HasComponent(NResArchive archive, string hexType)
    {
        return archive.Files.Any(x => x.FileType.Equals($"{hexType} 00 00 00", StringComparison.OrdinalIgnoreCase));
    }

    private static void ConvertModel(
        FileStream fs,
        NResArchive archive,
        string outputPath,
        int lod,
        int group)
    {
        var nodes = Msh0x01.ReadComponent(fs, archive);
        var geometry = Msh0x02.ReadComponent(fs, archive);
        var vertices = Msh0x03.ReadComponent(fs, archive);
        var indices = Msh0x06.ReadComponent(fs, archive);
        var batches = Msh0x0D.ReadComponent(fs, archive);

        using var writer = CreateObjWriter(outputPath);

        writer.WriteLine($"# MSH model converted from {Path.GetFileName(outputPath)}");
        writer.WriteLine($"# LOD: {lod}, group: {group}");
        writer.WriteLine($"# Nodes: {nodes.Nodes.Count}");
        writer.WriteLine($"# Geometry slots: {geometry.Slots.Count}");
        writer.WriteLine($"# Batches: {batches.Count}");
        writer.WriteLine($"# Vertices: {vertices.Count}");
        writer.WriteLine();

        WriteVertices(writer, vertices);

        var exportedFaces = 0;
        var skippedSlots = 0;
        var skippedBatches = 0;
        var skippedFaces = 0;

        for (var pieceIndex = 0; pieceIndex < nodes.Nodes.Count; pieceIndex++)
        {
            var node = nodes.Nodes[pieceIndex];
            var slotIndex = node.ResolveSlotIndex(lod, group);

            if (slotIndex == ushort.MaxValue)
            {
                skippedSlots++;
                continue;
            }

            if (slotIndex >= geometry.Slots.Count)
            {
                Warn($"Piece {pieceIndex}: geometry slot {slotIndex} out of range");
                skippedSlots++;
                continue;
            }

            var slot = geometry.Slots[slotIndex];

            if (!slot.HasBatches)
            {
                continue;
            }

            if (slot.BatchEndExclusive0x0D > batches.Count)
            {
                Warn($"Piece {pieceIndex}: batch range {slot.BatchStart0x0D}:{slot.BatchCount0x0D} out of range");
                skippedBatches++;
                continue;
            }

            writer.WriteLine();
            writer.WriteLine($"g piece_{pieceIndex}_slot_{slotIndex}");

            for (var batchIndex = slot.BatchStart0x0D; batchIndex < slot.BatchEndExclusive0x0D; batchIndex++)
            {
                var batch = batches[batchIndex];

                if (batch.IndexStart + batch.IndexCount > indices.Count)
                {
                    Warn($"Piece {pieceIndex}, batch {batchIndex}: index range {batch.IndexStart}:{batch.IndexCount} out of range");
                    skippedBatches++;
                    continue;
                }

                writer.WriteLine($"# batch {batchIndex}, material {batch.MaterialIndex}, flags {batch.Flags}");

                for (var i = 0; i + 2 < batch.IndexCount; i += 3)
                {
                    var indexBase = (int)batch.IndexStart + i;

                    var v1 = checked((int)batch.BaseVertex + indices[indexBase + 0]);
                    var v2 = checked((int)batch.BaseVertex + indices[indexBase + 1]);
                    var v3 = checked((int)batch.BaseVertex + indices[indexBase + 2]);

                    if (!IsValidTriangle(vertices.Count, v1, v2, v3))
                    {
                        skippedFaces++;
                        continue;
                    }

                    WriteFace(writer, v1, v2, v3);
                    exportedFaces++;
                }

                if (batch.IndexCount % 3 != 0)
                {
                    Warn($"Piece {pieceIndex}, batch {batchIndex}: index count {batch.IndexCount} is not divisible by 3");
                }
            }
        }

        Console.WriteLine($"Exported: {vertices.Count} vertices, {exportedFaces} faces");
        Console.WriteLine($"Skipped slots: {skippedSlots}, skipped batches: {skippedBatches}, skipped faces: {skippedFaces}");
        Console.WriteLine($"Output: {outputPath}");
    }

    private static void ConvertLandscape(
        FileStream fs,
        NResArchive archive,
        string outputPath,
        int lod,
        int group)
    {
        var nodes = Msh0x01.ReadComponent(fs, archive);
        var geometry = Msh0x02.ReadComponent(fs, archive);
        var vertices = Msh0x03.ReadComponent(fs, archive);
        var triangles = Msh0x15.ReadComponent(fs, archive);

        using var writer = CreateObjWriter(outputPath);

        writer.WriteLine($"# MSH landscape converted from {Path.GetFileName(outputPath)}");
        writer.WriteLine($"# LOD: {lod}, group: {group}");
        writer.WriteLine($"# Nodes/tiles: {nodes.Nodes.Count}");
        writer.WriteLine($"# Geometry slots: {geometry.Slots.Count}");
        writer.WriteLine($"# Triangles 0x15: {triangles.Count}");
        writer.WriteLine($"# Vertices: {vertices.Count}");
        writer.WriteLine();

        WriteVertices(writer, vertices);

        var exportedFaces = 0;
        var skippedSlots = 0;
        var skippedFaces = 0;

        for (var tileIndex = 0; tileIndex < nodes.Nodes.Count; tileIndex++)
        {
            var node = nodes.Nodes[tileIndex];
            var slotIndex = node.ResolveSlotIndex(lod, group);

            if (slotIndex == ushort.MaxValue)
            {
                skippedSlots++;
                continue;
            }

            if (slotIndex >= geometry.Slots.Count)
            {
                Warn($"Tile {tileIndex}: geometry slot {slotIndex} out of range");
                skippedSlots++;
                continue;
            }

            var slot = geometry.Slots[slotIndex];

            if (!slot.HasTriangles)
            {
                continue;
            }

            writer.WriteLine();
            writer.WriteLine($"g tile_{tileIndex}_slot_{slotIndex}");

            for (var triIndex = slot.TriStart0x07; triIndex < slot.TriEndExclusive0x07; triIndex++)
            {
                if (triIndex >= triangles.Count)
                {
                    Warn($"Tile {tileIndex}: triangle {triIndex} out of range");
                    skippedFaces++;
                    continue;
                }

                var tri = triangles[triIndex];

                var v1 = tri.Vertex1Index;
                var v2 = tri.Vertex2Index;
                var v3 = tri.Vertex3Index;

                if (!IsValidTriangle(vertices.Count, v1, v2, v3))
                {
                    skippedFaces++;
                    continue;
                }

                WriteFace(writer, v1, v2, v3);
                exportedFaces++;
            }
        }

        Console.WriteLine($"Exported: {vertices.Count} vertices, {exportedFaces} faces");
        Console.WriteLine($"Skipped slots: {skippedSlots}, skipped faces: {skippedFaces}");
        Console.WriteLine($"Output: {outputPath}");
    }

    private static StreamWriter CreateObjWriter(string outputPath)
    {
        return new StreamWriter(outputPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void WriteVertices(StreamWriter writer, IReadOnlyList<Vector3> vertices)
    {
        foreach (var vertex in vertices)
        {
            writer.WriteLine(FormattableString.Invariant($"v {vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}"));
        }
    }

    private static void WriteFace(StreamWriter writer, int v1, int v2, int v3)
    {
        writer.WriteLine($"f {v1 + 1} {v2 + 1} {v3 + 1}");
    }

    private static bool IsValidTriangle(int vertexCount, int v1, int v2, int v3)
    {
        return IsValidVertexIndex(vertexCount, v1)
            && IsValidVertexIndex(vertexCount, v2)
            && IsValidVertexIndex(vertexCount, v3);
    }

    private static bool IsValidVertexIndex(int vertexCount, int index)
    {
        return index >= 0 && index < vertexCount;
    }

    private static void Warn(string message)
    {
        Console.WriteLine($"WARNING: {message}");
    }
}