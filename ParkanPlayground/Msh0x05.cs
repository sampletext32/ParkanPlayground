using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x05: упакованные UV0. component / 1024.0.
/// </summary>
public static class Msh0x05
{
    public static List<Msh05Uv> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "05 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (05)");
        }

        if (entry.ElementSize != 4)
        {
            throw new Exception("Packed UV file (05) element size is not 4");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Packed UV component (0x05) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elements = new List<Msh05Uv>(entry.FileLength / entry.ElementSize);
        for (var i = 0; i < entry.FileLength / entry.ElementSize; i++)
        {
            var span = data.AsSpan(i * 4, 4);
            elements.Add(new Msh05Uv(
                BinaryPrimitives.ReadInt16LittleEndian(span[0..2]),
                BinaryPrimitives.ReadInt16LittleEndian(span[2..4])));
        }

        return elements;
    }
}

/// <summary>Упакованные UV0: две int16-компоненты (length = 4).</summary>
/// <param name="U">[0x00..0x02] U-компонента, uv = U / 1024.0.</param>
/// <param name="V">[0x02..0x04] V-компонента, uv = V / 1024.0.</param>
public readonly record struct Msh05Uv(short U, short V)
;
