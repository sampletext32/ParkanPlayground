using System.Buffers.Binary;
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
        
        ReadComponent01(mshFs, mshNres);

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

    private static List<string> TryRead0AComponent(FileStream mshFs, NResArchive archive)
    {
        var aFileEntry = archive.Files.FirstOrDefault(x => x.FileType == "0A 00 00 00");

        if (aFileEntry is null)
        {
            return [];
        }
        var data = new byte[aFileEntry.FileLength];
        mshFs.Seek(aFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        int pos = 0;
        var strings = new List<string>();
        while (pos < data.Length)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(pos));
            if (len == 0)
            {
                pos += 4; // empty entry, no string attached
                strings.Add(""); // add empty string
            }
            else
            {
                // len is not 0, we need to read it
                var strBytes = data.AsSpan(pos + 4, len);
                var str = Encoding.UTF8.GetString(strBytes);
                strings.Add(str);
                pos += len + 4 + 1; // skip length prefix and string itself, +1, because it's null-terminated
            }
        }
        if (strings.Count != aFileEntry.ElementCount)
        {
            throw new Exception("String count mismatch in 0A component");
        }

        return strings;
    }

    private static void ReadComponent01(FileStream mshFs, NResArchive archive)
    {
        var headerFileEntry = archive.Files.FirstOrDefault(x => x.FileType == "01 00 00 00");

        if (headerFileEntry is null)
        {
            throw new Exception("Archive doesn't contain header file (01)");
        }
        var headerData = new byte[headerFileEntry.ElementCount * headerFileEntry.ElementSize];
        mshFs.Seek(headerFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(headerData, 0, headerData.Length);
        
        var descriptions = TryRead0AComponent(mshFs, archive);
        
        var chunks = headerData.Chunk(headerFileEntry.ElementSize).ToList();

        var converted = chunks.Select(x => new
        {
            Byte00 = x[0],
            Byte01 = x[1],
            Bytes0204 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(2)),
        }).ToList();
        
        _ = 5;
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