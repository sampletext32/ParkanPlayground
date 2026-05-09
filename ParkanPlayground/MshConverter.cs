using System.Text;
using Common;
using NResLib;

namespace ParkanPlayground;

public enum MshType
{
    Unknown,
    Model,      // Has component 06 (indices), 0D (batches), 07
    Landscape   // Has component 0B (per-triangle material), uses 15 directly
}

public class MshConverter
{
    /// <summary>Определяет тип MSH по набору hex-компонентов архива.</summary>
    public static MshType DetectMeshType(NResArchive archive)
    {
        bool hasComponent06 = archive.Files.Any(f => f.FileType == "06 00 00 00");
        bool hasComponent0B = archive.Files.Any(f => f.FileType == "0B 00 00 00");
        bool hasComponent0D = archive.Files.Any(f => f.FileType == "0D 00 00 00");
        
        // Model: Uses indexed triangles via component 06 and batches via 0D
        if (hasComponent06 && hasComponent0D)
            return MshType.Model;
        
        // Landscape: Uses direct triangles in component 15, with material data in 0B
        if (hasComponent0B && !hasComponent06)
            return MshType.Landscape;
        
        return MshType.Unknown;
    }

    /// <summary>Конвертирует .msh в OBJ с автоопределением типа меша.</summary>
    /// <param name="mshPath">Путь к .msh файлу.</param>
    /// <param name="outputPath">Путь к OBJ, по умолчанию рядом с исходным файлом.</param>
    /// <param name="lodLevel">LOD для экспорта.</param>
    /// <param name="group">Группа slot внутри LOD. slotIndex[lod * 5 + group].</param>
    public void Convert(string mshPath, string? outputPath = null, int lodLevel = 0, int group = 0)
    {
        var mshNresResult = NResParser.ReadFile(mshPath);
        if (mshNresResult.Archive is null)
        {
            Console.WriteLine($"ERROR: Failed to read NRes archive: {mshNresResult.Error}");
            return;
        }
        
        var archive = mshNresResult.Archive;
        var meshType = DetectMeshType(archive);
        
        outputPath ??= Path.ChangeExtension(mshPath, ".obj");
        
        Console.WriteLine($"Converting: {Path.GetFileName(mshPath)}");
        Console.WriteLine($"Detected type: {meshType}");
        
        using var fs = new FileStream(mshPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        switch (meshType)
        {
            case MshType.Model:
                ConvertModel(fs, archive, outputPath, lodLevel, group);
                break;
            case MshType.Landscape:
                ConvertLandscape(fs, archive, outputPath, lodLevel, group);
                break;
            default:
                Console.WriteLine("ERROR: Unknown mesh type, cannot convert.");
                break;
        }
    }

    /// <summary>
    /// Конвертирует обычную модель в OBJ.
    /// Путь данных: 0x01 -> 0x02 -> 0x0D -> 0x06 -> 0x03.
    /// </summary>
    private void ConvertModel(FileStream fs, NResArchive archive, string outputPath, int lodLevel, int group)
    {
        var component01 = Msh0x01.ReadComponent(fs, archive);
        var component02 = Msh0x02.ReadComponent(fs, archive);
        var component03 = Msh0x03.ReadComponent(fs, archive);
        var component06 = Msh0x06.ReadComponent(fs, archive);
        var component07 = Msh0x07.ReadComponent(fs, archive);
        var component0D = Msh0x0D.ReadComponent(fs, archive);

        Console.WriteLine($"Vertices: {component03.Count}");
        Console.WriteLine($"Pieces: {component01.Elements.Count}");
        Console.WriteLine($"Submeshes: {component02.Elements.Count}");
        
        using var sw = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        sw.WriteLine($"# Model mesh converted from {Path.GetFileName(outputPath)}");
        sw.WriteLine($"# LOD level: {lodLevel}");

        foreach (var v in component03)
        {
            sw.WriteLine(FormattableString.Invariant($"v {v.X:F6} {v.Y:F6} {v.Z:F6}"));
        }

        int exportedFaces = 0;
        
        for (var pieceIndex = 0; pieceIndex < component01.Elements.Count; pieceIndex++)
        {
            var piece = component01.Elements[pieceIndex];
            
            var submeshIdx = piece.ResolveSlotIndex(lodLevel, group);
            if (submeshIdx == 0xFFFF || submeshIdx >= component02.Elements.Count)
                continue;

            sw.WriteLine($"g piece_{pieceIndex}");
            
            var submesh = component02.Elements[submeshIdx];
            var batchStart = submesh.BatchStart;
            var batchCount = submesh.BatchCount;
            if (batchStart + batchCount > component0D.Count)
            {
                Console.WriteLine($"WARNING: Batch range {batchStart}:{batchCount} out of range for piece {pieceIndex}");
                continue;
            }

            for (var batchIdx = 0; batchIdx < batchCount; batchIdx++)
            {
                var batch = component0D[batchStart + batchIdx];
                var baseVertex = (int)batch.BaseVertex;
                var indexStart = (int)batch.IndexStart;
                var indexCount = batch.IndexCount;
                if (indexStart + indexCount > component06.Count)
                {
                    Console.WriteLine($"WARNING: Index range {indexStart}:{indexCount} out of range for piece {pieceIndex}");
                    continue;
                }

                for (int i = 0; i < indexCount; i += 3)
                {
                    if (i + 2 >= indexCount)
                    {
                        Console.WriteLine($"WARNING: Batch has non-triangle index tail in piece {pieceIndex}");
                        break;
                    }

                    var i1 = baseVertex + component06[indexStart + i];
                    var i2 = baseVertex + component06[indexStart + i + 1];
                    var i3 = baseVertex + component06[indexStart + i + 2];
                    if (i1 >= component03.Count || i2 >= component03.Count || i3 >= component03.Count)
                    {
                        Console.WriteLine($"WARNING: Vertex index out of range in piece {pieceIndex}");
                        continue;
                    }

                    sw.WriteLine($"f {i1 + 1} {i2 + 1} {i3 + 1}");
                    exportedFaces++;
                }
            }
        }
        
        Console.WriteLine($"Exported: {component03.Count} vertices, {exportedFaces} faces");
        Console.WriteLine($"Output: {outputPath}");
    }

    /// <summary>
    /// Конвертирует terrain mesh в OBJ.
    /// Путь данных является terrain-гипотезой проекта: 0x01 -> 0x02 -> 0x15.
    /// </summary>
    private void ConvertLandscape(FileStream fs, NResArchive archive, string outputPath, int lodLevel, int group)
    {
        var component01 = Msh0x01.ReadComponent(fs, archive);
        var component02 = Msh0x02.ReadComponent(fs, archive);
        var component03 = Msh0x03.ReadComponent(fs, archive);
        var component15 = Msh0x15.ReadComponent(fs, archive);

        Console.WriteLine($"Vertices: {component03.Count}");
        Console.WriteLine($"Triangles: {component15.Count}");
        Console.WriteLine($"Tiles: {component01.Elements.Count}");
        Console.WriteLine($"Submeshes: {component02.Elements.Count}");
        
        using var sw = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        sw.WriteLine($"# Landscape mesh converted from {Path.GetFileName(outputPath)}");
        sw.WriteLine($"# LOD level: {lodLevel}");
        sw.WriteLine($"# Tile grid: {(int)Math.Sqrt(component01.Elements.Count)}x{(int)Math.Sqrt(component01.Elements.Count)}");

        foreach (var v in component03)
        {
            sw.WriteLine(FormattableString.Invariant($"v {v.X:F6} {v.Y:F6} {v.Z:F6}"));
        }

        int exportedFaces = 0;
        
        for (var tileIdx = 0; tileIdx < component01.Elements.Count; tileIdx++)
        {
            var tile = component01.Elements[tileIdx];
            
            var submeshIdx = tile.ResolveSlotIndex(lodLevel, group);
            if (submeshIdx == 0xFFFF || submeshIdx >= component02.Elements.Count)
                continue;

            sw.WriteLine($"g tile_{tileIdx}");
            
            var submesh = component02.Elements[submeshIdx];
            
            var triangleStart = submesh.TriStart;
            var triangleCount = submesh.TriCount;

            for (var triOffset = 0; triOffset < triangleCount; triOffset++)
            {
                var triIdx = triangleStart + triOffset;
                if (triIdx >= component15.Count)
                {
                    Console.WriteLine($"WARNING: Triangle index {triIdx} out of range for tile {tileIdx}");
                    continue;
                }

                var tri = component15[triIdx];
                if (tri.Vertex1Index >= component03.Count || tri.Vertex2Index >= component03.Count || tri.Vertex3Index >= component03.Count)
                {
                    Console.WriteLine($"WARNING: Vertex index out of range for tile {tileIdx}");
                    continue;
                }

                sw.WriteLine($"f {tri.Vertex1Index + 1} {tri.Vertex2Index + 1} {tri.Vertex3Index + 1}");
                exportedFaces++;
            }
        }
        
        Console.WriteLine($"Exported: {component03.Count} vertices, {exportedFaces} faces");
        Console.WriteLine($"Output: {outputPath}");
    }

    public record Face(Vector3 P1, Vector3 P2, Vector3 P3);

    public static void ExportCube(string filePath, Vector3[] points)
    {
        if (points.Length != 8)
            throw new ArgumentException("Cube must have exactly 8 points.");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Запись вершин.
            foreach (var p in points)
            {
                writer.WriteLine($"v {p.X} {p.Y} {p.Z}");
            }

            // Запись граней: каждая грань задается четырьмя вершинами, OBJ использует индексацию с 1.
            int[][] faces = new int[][]
            {
                new int[] { 1, 2, 3, 4 }, // низ
                new int[] { 5, 6, 7, 8 }, // верх
                new int[] { 1, 2, 6, 5 }, // перед
                new int[] { 2, 3, 7, 6 }, // право
                new int[] { 3, 4, 8, 7 }, // зад
                new int[] { 4, 1, 5, 8 } // лево
            };

            foreach (var f in faces)
            {
                writer.WriteLine($"f {f[0]} {f[1]} {f[2]} {f[3]}");
            }
        }
    }

    public static void ExportCubesAtPositions(string filePath, List<Vector3> centers, float size = 2f)
    {
        float half = size / 2f;
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int vertexOffset = 0;

            foreach (var c in centers)
            {
                // Генерация восьми вершин куба.
                Vector3[] vertices = new Vector3[]
                {
                    new Vector3(c.X - half, c.Y - half, c.Z - half),
                    new Vector3(c.X + half, c.Y - half, c.Z - half),
                    new Vector3(c.X + half, c.Y - half, c.Z + half),
                    new Vector3(c.X - half, c.Y - half, c.Z + half),

                    new Vector3(c.X - half, c.Y + half, c.Z - half),
                    new Vector3(c.X + half, c.Y + half, c.Z - half),
                    new Vector3(c.X + half, c.Y + half, c.Z + half),
                    new Vector3(c.X - half, c.Y + half, c.Z + half)
                };

                // Запись вершин.
                foreach (var v in vertices)
                {
                    writer.WriteLine($"v {v.X} {v.Y} {v.Z}");
                }

                // Описание граней: индексация с 1, порядок против часовой стрелки.
                int[][] faces = new int[][]
                {
                    new int[] { 1, 2, 3, 4 }, // низ
                    new int[] { 5, 6, 7, 8 }, // верх
                    new int[] { 1, 2, 6, 5 }, // перед
                    new int[] { 2, 3, 7, 6 }, // право
                    new int[] { 3, 4, 8, 7 }, // зад
                    new int[] { 4, 1, 5, 8 } // лево
                };

                // Запись граней со смещением индексов.
                foreach (var f in faces)
                {
                    writer.WriteLine(
                        $"f {f[0] + vertexOffset} {f[1] + vertexOffset} {f[2] + vertexOffset} {f[3] + vertexOffset}");
                }

                vertexOffset += 8;
            }
        }
    }

    void Export(string filePath, IEnumerable<Vector3> vertices, List<IndexedEdge> edges)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("# Exported OBJ file");

            // Запись вершин.
            foreach (var v in vertices)
            {
                writer.WriteLine($"v {v.X:F2} {v.Y:F2} {v.Z:F2}");
            }

            // Запись ребер как line-элементов OBJ.
            foreach (var e in edges)
            {
                // OBJ использует индексацию с 1.
                writer.WriteLine($"l {e.Index1 + 1} {e.Index2 + 1}");
            }
        }
    }
}
