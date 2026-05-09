using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x03: позиции вершин
/// </summary>
public class Msh0x03
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

        if (verticesFileEntry.FileLength % verticesFileEntry.ElementSize != 0)
        {
            throw new Exception("Positions component (0x03) payload size is not divisible by element size");
        }

        var verticesFile = new byte[verticesFileEntry.FileLength];
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
