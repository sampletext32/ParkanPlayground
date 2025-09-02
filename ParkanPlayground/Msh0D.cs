using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

public static class Msh0D
{
    public const int ElementSize = 20;
    
    public static List<Msh0DElement> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "0D 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (0D)");
        }

        var data = new byte[entry.ElementCount * entry.ElementSize];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(ElementSize);

        var elements = elementBytes.Select(x => new Msh0DElement()
        {
            Flags = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0)),
            Magic04 = x.AsSpan(4)[0],
            Magic05 = x.AsSpan(5)[0],
            Magic06 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(6)),
            CountOf06 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(8)),
            IndexInto06 = BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0xA)),
            CountOf03 = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0xE)),
            IndexInto03 = BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x10)),
        }).ToList();

        return elements;
    }

    public class Msh0DElement
    {
        public uint Flags { get; set; }
        
        // Magic04 и Magic06 обрабатываются вместе

        public byte Magic04 { get; set; }
        public byte Magic05 { get; set; }
        public ushort Magic06 { get; set; }
        public ushort CountOf06 { get; set; }
        public int IndexInto06 { get; set; }
        public ushort CountOf03 { get; set; }
        public int IndexInto03 { get; set; }
    }
}