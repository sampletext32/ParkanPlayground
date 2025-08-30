using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using Common;
using NResLib;

namespace ParkanPlayground;

public class MshConverter
{
    public void Convert(string mshPath)
    {
        var mshNresResult = NResParser.ReadFile(mshPath);
        var mshNres = mshNresResult.Archive!;

        using var mshFs = new FileStream(mshPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var component01 = Msh01.ReadComponent(mshFs, mshNres);
        var component02 = Msh02.ReadComponent(mshFs, mshNres);
        var component0A = Msh0A.ReadComponent(mshFs, mshNres);
        var component07 = Msh07.ReadComponent(mshFs, mshNres);
        var component0D = Msh0D.ReadComponent(mshFs, mshNres);

        // Triangle Vertex Indices
        var component06 = Msh06.ReadComponent(mshFs, mshNres);

        // vertices
        var component03 = Msh03.ReadComponent(mshFs, mshNres);

        _ = 5;

        // --- Write OBJ ---
        using var sw = new StreamWriter("test.obj", false, new UTF8Encoding(false));

        foreach (var v in component03)
            sw.WriteLine($"v {v.X:F8} {v.Y:F8} {v.Z:F8}");

        var vertices = new List<Vector3>();
        var faces = new List<(int, int, int)>(); // store indices into vertices list

        // 01 - это части меша (Piece)
        for (var pieceIndex = 0; pieceIndex < component01.Elements.Count; pieceIndex++)
        {
            var piece01 = component01.Elements[pieceIndex];
            // var state = (piece.State00 == 0xffff) ? 0 : piece.State00;

            for (var lodIndex = 0; lodIndex < piece01.Lod.Length; lodIndex++)
            {
                var lod = piece01.Lod[lodIndex];
                if (lod == 0xffff)
                {
                    Console.WriteLine($"Piece {pieceIndex} has lod -1 at {lodIndex}. Skipping");
                    continue;
                }

                sw.WriteLine($"o piece_{pieceIndex}_lod_{lodIndex}");
                // 02 - Submesh
                var part02 = component02.Elements[lod];

                int indexInto07 = part02.StartIndexIn07;

                var element0Dstart = part02.StartOffsetIn0d;
                var element0Dcount = part02.ByteLengthIn0D;

                // Console.WriteLine($"Started piece {pieceIndex}. LOD={lod}. 0D start={element0Dstart}, count={element0Dcount}");

                for (var comp0Dindex = 0; comp0Dindex < element0Dcount; comp0Dindex++)
                {
                    var element0D = component0D[element0Dstart + comp0Dindex];

                    var indexInto03 = element0D.IndexInto03;
                    var indexInto06 = element0D.IndexInto06; // indices

                    uint maxIndex = element0D.CountOf03;
                    uint indicesCount = element0D.CountOf06;


                    // Convert IndexInto06 to ushort array index (3 ushorts per triangle)
                    // Console.WriteLine($"Processing 0D element[{element0Dstart + comp0Dindex}]. IndexInto03={indexInto03}, IndexInto06={indexInto06}. Number of triangles={indicesCount}");

                    if (indicesCount != 0)
                    {
                        // sw.WriteLine($"o piece_{pieceIndex}_of_mesh_{comp0Dindex}");

                        for (int ind = 0; ind < indicesCount; ind += 3)
                        {
                            // Each triangle uses 3 consecutive ushorts in component06

                            // sw.WriteLine($"o piece_{pieceIndex}_of_mesh_{comp0Dindex}_tri_{ind}");

                            var comp07 = component07[indexInto07];

                            var i1 = indexInto03 + component06[indexInto06];
                            var i2 = indexInto03 + component06[indexInto06 + 1];
                            var i3 = indexInto03 + component06[indexInto06 + 2];

                            var v1 = component03[i1];
                            var v2 = component03[i2];
                            var v3 = component03[i3];

                            sw.WriteLine($"f {i1 + 1} {i2 + 1} {i3 + 1}");

                            // push vertices to global list
                            vertices.Add(v1);
                            vertices.Add(v2);
                            vertices.Add(v3);

                            int baseIndex = vertices.Count;
                            // record face (OBJ is 1-based indexing!)
                            faces.Add((baseIndex - 2, baseIndex - 1, baseIndex));

                            indexInto07++;
                            indexInto06 += 3; // step by 3 since each triangle uses 3 ushorts
                        }

                        _ = 5;
                    }
                }
            }
        }
    }

    public void Convert2(string mshPath)
    {
        var mshNresResult = NResParser.ReadFile(mshPath);
        var mshNres = mshNresResult.Archive!;

        using var mshFs = new FileStream(mshPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var component01 = Msh01.ReadComponent(mshFs, mshNres);
        var component02 = Msh02.ReadComponent(mshFs, mshNres);
        var component0A = Msh0A.ReadComponent(mshFs, mshNres);
        var component07 = Msh07.ReadComponent(mshFs, mshNres);
        var component0D = Msh0D.ReadComponent(mshFs, mshNres);

        // Triangle Vertex Indices
        var component06 = Msh06.ReadComponent(mshFs, mshNres);

        // vertices
        var component03 = Msh03.ReadComponent(mshFs, mshNres);

        var csv06 = new StreamWriter("06.csv", false, Encoding.UTF8);

        for (var i = 0; i < component06.Count; i += 3)
        {
            // csv06.WriteLine($"{component06[i]}");
            csv06.WriteLine($"{component06[i]}, {component06[i + 1]}, {component06[i + 2]}");
        }

        csv06.Dispose();

        var csv03 = new StreamWriter("03.obj", false, Encoding.UTF8);

        csv03.WriteLine();
        for (var i = 7525; i < component03.Count; i++)
        {
            // csv06.WriteLine($"{component06[i]}");
            // csv03.WriteAsync($"o {i - 7525}");
            csv03.WriteLine(
                $"v {component03[i].X.ToString("F2", CultureInfo.InvariantCulture)} {component03[i].Y.ToString("F2", CultureInfo.InvariantCulture)} {component03[i].Z.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        for (var i = 10485; i < component06.Count; i += 3)
        {
            csv03.WriteLine($"f {component06[i] + 1} {component06[i + 1] + 1} {component06[i + 2] + 1}");
        }

        csv03.Dispose();

        // --- Write OBJ ---
        using var sw = new StreamWriter("test.obj", false, Encoding.UTF8);

        for (var index = 0; index < component03.Count; index++)
        {
            var v = component03[index];
            sw.WriteLine($"v {v.X:F8} {v.Y:F8} {v.Z:F8}");
        }

        for (var i = 0; i < 1; i++)
        {
            sw.WriteLine($"o elem_0d_{i}");
            var element0D = component0D[i];

            Console.WriteLine($"Processing element 0D [{i}]:");

            var elementsOf06 = component06
                .Skip(element0D.IndexInto06)
                .Take(element0D.CountOf06)
                .ToList();

            Console.WriteLine($"\tCount of 06: {elementsOf06.Count}, starting from {element0D.IndexInto06}");
            var indexInto03 = element0D.IndexInto03;

            Console.WriteLine($"\tIndexInto03: {indexInto03}");

            if (elementsOf06.Count < 3)
            {
                throw new Exception("Less than 3 points");
            }
        }
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