using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x04: упакованные нормали вершин. clamp(component / 127.0, -1..1).
/// </summary>
public static class Msh0x04
{
    public static List<Msh04Normal> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "04 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (04)");
        }

        if (entry.ElementSize != 4)
        {
            throw new Exception("Packed normals file (04) element size is not 4");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Packed normals component (0x04) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elements = new List<Msh04Normal>(entry.FileLength / entry.ElementSize);
        for (var i = 0; i < entry.FileLength / entry.ElementSize; i++)
        {
            var offset = i * 4;
            elements.Add(new Msh04Normal(
                unchecked((sbyte)data[offset + 0]),
                unchecked((sbyte)data[offset + 1]),
                unchecked((sbyte)data[offset + 2]),
                unchecked((sbyte)data[offset + 3])));
        }

        return elements;
    }
}

/// <summary>Упакованная нормаль: четыре int8-компоненты (length = 4).</summary>
/// <param name="X">[0x00..0x01] X-компонента.</param>
/// <param name="Y">[0x01..0x02] Y-компонента.</param>
/// <param name="Z">[0x02..0x03] Z-компонента.</param>
/// <param name="W">[0x03..0x04] W-компонента.</param>
public readonly record struct Msh04Normal(sbyte X, sbyte Y, sbyte Z, sbyte W)
;
