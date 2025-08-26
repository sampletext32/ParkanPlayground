using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

public static class Msh06
{
    public static List<ushort> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "06 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (06)");
        }

        var data = new byte[entry.ElementCount * entry.ElementSize];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elements = new List<ushort>((int)entry.ElementCount);
        for (var i = 0; i < entry.ElementCount; i++)
        {
            elements.Add(
                BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(i * 2))
            );
        }

        return elements;
    }
}