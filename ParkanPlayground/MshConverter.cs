using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

public class MshConverter
{
    public void Convert(string mshPath)
    {
        var mshNresResult = NResParser.ReadFile(mshPath);

        var mshNres = mshNresResult.Archive!;

        var verticesFileEntry = mshNres.Files.FirstOrDefault(x => x.FileType == "03 00 00 00");

        if (verticesFileEntry is null)
        {
            throw new Exception("Archive doesn't contain vertices file (03)");
        }

        if (verticesFileEntry.ElementSize != 12)
        {
            throw new Exception("Vertices file (03) element size is not 12");
        }
        
        using var mshFs = new FileStream(mshPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var vertices = ReadVertices(verticesFileEntry, mshFs);

        var edgesFileEntry = mshNres.Files.FirstOrDefault(x => x.FileType == "06 00 00 00");

        if (edgesFileEntry is null)
        {
            throw new Exception("Archive doesn't contain edges file (06)");
        }
        
        var edgesFile = new byte[edgesFileEntry.ElementCount * edgesFileEntry.ElementSize];
        mshFs.Seek(edgesFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(edgesFile, 0, edgesFile.Length);

        var edges = new List<IndexedEdge>((int)edgesFileEntry.ElementCount / 2);

        for (int i = 0; i < edgesFileEntry.ElementCount / 2; i++)
        {
            var index1 = BinaryPrimitives.ReadUInt16LittleEndian(edgesFile.AsSpan().Slice(i * 2));
            var index2 = BinaryPrimitives.ReadUInt16LittleEndian(edgesFile.AsSpan().Slice(i * 2 + 2));
            edges.Add(new IndexedEdge(index1, index2));
        }

        Export($"{Path.GetFileNameWithoutExtension(mshPath)}.obj", vertices, edges);

    }

    private static List<Vector3> ReadVertices(ListMetadataItem verticesFileEntry, FileStream mshFs)
    {
        var verticesFile = new byte[verticesFileEntry.ElementCount * verticesFileEntry.ElementSize];
        mshFs.Seek(verticesFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(verticesFile, 0, verticesFile.Length);

        var vertices = verticesFile.Chunk(12).Select(x => new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(x.AsSpan(0)),
                BinaryPrimitives.ReadSingleLittleEndian(x.AsSpan(4)),
                BinaryPrimitives.ReadSingleLittleEndian(x.AsSpan(8))
            )
        ).ToList();
        return vertices;
    }

    void Export(string filePath, List<Vector3> vertices, List<IndexedEdge> edges)
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