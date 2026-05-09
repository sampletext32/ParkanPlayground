using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x06: индексный буфер
/// </summary>
public static class Msh0x06
{
    public static List<ushort> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "06 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (06)");
        }

        if (entry.ElementSize != 2)
        {
            throw new Exception("Index buffer component (0x06) element size is not 2");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Index buffer component (0x06) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elements = new List<ushort>(entry.FileLength / entry.ElementSize);
        for (var i = 0; i < entry.FileLength / entry.ElementSize; i++)
        {
            elements.Add(
                BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(i * 2))
            );
        }

        return elements;
    }
}
