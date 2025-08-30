using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

public class Msh03
{
    public static List<Vector3> ReadComponent(FileStream mshFs, NResArchive mshNres)
    {
        var verticesFileEntry = mshNres.Files.FirstOrDefault(x => x.FileType == "03 00 00 00");

        if (verticesFileEntry is null)
        {
            throw new Exception("Archive doesn't contain vertices file (03)");
        }

        if (verticesFileEntry.ElementSize != 12)
        {
            throw new Exception("Vertices file (03) element size is not 12");
        }

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
}