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
    /// <summary>
    /// Detects mesh type based on which components are present in the archive.
    /// </summary>
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

    /// <summary>
    /// Converts a .msh file to OBJ format, auto-detecting mesh type.
    /// </summary>
    /// <param name="mshPath">Path to the .msh file</param>
    /// <param name="outputPath">Output OBJ path (optional, defaults to input name + .obj)</param>
    /// <param name="lodLevel">LOD level to export (0 = highest detail)</param>
    public void Convert(string mshPath, string? outputPath = null, int lodLevel = 0)
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
                ConvertModel(fs, archive, outputPath, lodLevel);
                break;
            case MshType.Landscape:
                ConvertLandscape(fs, archive, outputPath, lodLevel);
                break;
            default:
                Console.WriteLine("ERROR: Unknown mesh type, cannot convert.");
                break;
        }
    }

    /// <summary>
    /// Converts a model mesh (robots, buildings, etc.) to OBJ.
    /// Uses indexed triangles: 01 → 02 → 0D → 06 → 03
    /// </summary>
    private void ConvertModel(FileStream fs, NResArchive archive, string outputPath, int lodLevel)
    {
        var component01 = Msh01.ReadComponent(fs, archive);
        var component02 = Msh02.ReadComponent(fs, archive);
        var component03 = Msh03.ReadComponent(fs, archive);
        var component06 = Msh06.ReadComponent(fs, archive);
        var component07 = Msh07.ReadComponent(fs, archive);
        var component0D = Msh0D.ReadComponent(fs, archive);

        Console.WriteLine($"Vertices: {component03.Count}");
        Console.WriteLine($"Pieces: {component01.Elements.Count}");
        Console.WriteLine($"Submeshes: {component02.Elements.Count}");
        
        using var sw = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        sw.WriteLine($"# Model mesh converted from {Path.GetFileName(outputPath)}");
        sw.WriteLine($"# LOD level: {lodLevel}");

        // Write all vertices
        foreach (var v in component03)
            sw.WriteLine($"v {v.X:F6} {v.Y:F6} {v.Z:F6}");

        int exportedFaces = 0;
        
        for (var pieceIndex = 0; pieceIndex < component01.Elements.Count; pieceIndex++)
        {
            var piece = component01.Elements[pieceIndex];
            
            // Get submesh index for requested LOD
            if (lodLevel >= piece.Lod.Length)
                continue;
                
            var submeshIdx = piece.Lod[lodLevel];
            if (submeshIdx == 0xFFFF || submeshIdx >= component02.Elements.Count)
                continue;

            sw.WriteLine($"g piece_{pieceIndex}");
            
            var submesh = component02.Elements[submeshIdx];
            var batchStart = submesh.StartOffsetIn0d;
            var batchCount = submesh.ByteLengthIn0D;

            for (var batchIdx = 0; batchIdx < batchCount; batchIdx++)
            {
                var batch = component0D[batchStart + batchIdx];
                var baseVertex = batch.IndexInto03;
                var indexStart = batch.IndexInto06;
                var indexCount = batch.CountOf06;

                for (int i = 0; i < indexCount; i += 3)
                {
                    var i1 = baseVertex + component06[indexStart + i];
                    var i2 = baseVertex + component06[indexStart + i + 1];
                    var i3 = baseVertex + component06[indexStart + i + 2];

                    sw.WriteLine($"f {i1 + 1} {i2 + 1} {i3 + 1}");
                    exportedFaces++;
                }
            }
        }
        
        Console.WriteLine($"Exported: {component03.Count} vertices, {exportedFaces} faces");
        Console.WriteLine($"Output: {outputPath}");
    }

    /// <summary>
    /// Converts a landscape mesh (terrain) to OBJ.
    /// Uses direct triangles: 01 → 02 → 15 (via StartIndexIn07/CountIn07)
    /// </summary>
    private void ConvertLandscape(FileStream fs, NResArchive archive, string outputPath, int lodLevel)
    {
        var component01 = Msh01.ReadComponent(fs, archive);
        var component02 = Msh02.ReadComponent(fs, archive);
        var component03 = Msh03.ReadComponent(fs, archive);
        var component15 = Msh15.ReadComponent(fs, archive);

        Console.WriteLine($"Vertices: {component03.Count}");
        Console.WriteLine($"Triangles: {component15.Count}");
        Console.WriteLine($"Tiles: {component01.Elements.Count}");
        Console.WriteLine($"Submeshes: {component02.Elements.Count}");
        
        using var sw = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        sw.WriteLine($"# Landscape mesh converted from {Path.GetFileName(outputPath)}");
        sw.WriteLine($"# LOD level: {lodLevel}");
        sw.WriteLine($"# Tile grid: {(int)Math.Sqrt(component01.Elements.Count)}x{(int)Math.Sqrt(component01.Elements.Count)}");

        // Write all vertices
        foreach (var v in component03)
            sw.WriteLine($"v {v.X:F6} {v.Y:F6} {v.Z:F6}");

        int exportedFaces = 0;
        
        for (var tileIdx = 0; tileIdx < component01.Elements.Count; tileIdx++)
        {
            var tile = component01.Elements[tileIdx];
            
            // Get submesh index for requested LOD
            if (lodLevel >= tile.Lod.Length)
                continue;
                
            var submeshIdx = tile.Lod[lodLevel];
            if (submeshIdx == 0xFFFF || submeshIdx >= component02.Elements.Count)
                continue;

            sw.WriteLine($"g tile_{tileIdx}");
            
            var submesh = component02.Elements[submeshIdx];
            
            // For landscape, StartIndexIn07 = triangle start index, CountIn07 = triangle count
            var triangleStart = submesh.StartIndexIn07;
            var triangleCount = submesh.CountIn07;

            for (var triOffset = 0; triOffset < triangleCount; triOffset++)
            {
                var triIdx = triangleStart + triOffset;
                if (triIdx >= component15.Count)
                {
                    Console.WriteLine($"WARNING: Triangle index {triIdx} out of range for tile {tileIdx}");
                    continue;
                }

                var tri = component15[triIdx];
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
            // Write vertices
            foreach (var p in points)
            {
                writer.WriteLine($"v {p.X} {p.Y} {p.Z}");
            }

            // Write faces (each face defined by 4 vertices, using 1-based indices)
            int[][] faces = new int[][]
            {
                new int[] { 1, 2, 3, 4 }, // bottom
                new int[] { 5, 6, 7, 8 }, // top
                new int[] { 1, 2, 6, 5 }, // front
                new int[] { 2, 3, 7, 6 }, // right
                new int[] { 3, 4, 8, 7 }, // back
                new int[] { 4, 1, 5, 8 } // left
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
                // Generate 8 vertices for this cube
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

                // Write vertices
                foreach (var v in vertices)
                {
                    writer.WriteLine($"v {v.X} {v.Y} {v.Z}");
                }

                // Define faces (1-based indices, counter-clockwise)
                int[][] faces = new int[][]
                {
                    new int[] { 1, 2, 3, 4 }, // bottom
                    new int[] { 5, 6, 7, 8 }, // top
                    new int[] { 1, 2, 6, 5 }, // front
                    new int[] { 2, 3, 7, 6 }, // right
                    new int[] { 3, 4, 8, 7 }, // back
                    new int[] { 4, 1, 5, 8 } // left
                };

                // Write faces with offset
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

            // Write vertices
            foreach (var v in vertices)
            {
                writer.WriteLine($"v {v.X:F2} {v.Y:F2} {v.Z:F2}");
            }

            // Write edges as lines ("l" elements in .obj format)
            foreach (var e in edges)
            {
                // OBJ uses 1-based indexing
                writer.WriteLine($"l {e.Index1 + 1} {e.Index2 + 1}");
            }
        }
    }
}