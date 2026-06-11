using System.Buffers.Binary;
using NResLib;

namespace MshLib;

public static class Msh0x13
{
    public static Msh0x13Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "13 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain animation map component (0x13)");
        }

        if (entry.ElementSize < 2)
        {
            throw new Exception("Animation map component (0x13) element size is too small");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementCount = checked((int)entry.ElementCount);
        var entries = new List<MshAnimationMapEntry>(elementCount);
        var span = data.AsSpan();
        for (var i = 0; i < elementCount; i++)
        {
            var elementOffset = i * entry.ElementSize;
            entries.Add(new MshAnimationMapEntry(
                BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(elementOffset, 2))));
        }

        return new Msh0x13Component(entry.ElementSize, entries)
        {
            MaxAnimationTime = entry.Magic1
        };
    }
}

public readonly record struct MshAnimationMapEntry(
    ushort KeyframeIndex
);

public sealed record Msh0x13Component(
    int ElementSize,
    List<MshAnimationMapEntry> Entries
)
{
    public int MaxAnimationTime { get; init; }
}
