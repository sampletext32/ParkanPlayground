using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

public static class Msh07
{
    public static List<Msh07Element> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "07 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (07)");
        }

        var data = new byte[entry.ElementCount * entry.ElementSize];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(16);

        var elements = elementBytes.Select(x => new Msh07Element()
        {
            Flags = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0)),
            Magic02 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(2)),
            Magic04 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(4)),
            Magic06 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(6)),
            OffsetX = BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(8)),
            OffsetY = BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(10)),
            OffsetZ = BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(12)),
            Magic14 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(14)),
        }).ToList();

        return elements;
    }

    public class Msh07Element
    {
        public ushort Flags { get; set; }
        public ushort Magic02 { get; set; }
        public ushort Magic04 { get; set; }
        public ushort Magic06 { get; set; }
        // normalized vector X, need to divide by 32767 to get float in range -1..1
        public short OffsetX { get; set; }
        // normalized vector Y, need to divide by 32767 to get float in range -1..1
        public short OffsetY { get; set; }
        // normalized vector Z, need to divide by 32767 to get float in range -1..1
        public short OffsetZ { get; set; }
        public ushort Magic14 { get; set; }
    }
}