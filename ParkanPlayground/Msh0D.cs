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
            Magic04 = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(4)),
            number_of_triangles = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(8)),
            IndexInto06 = BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(10)),
            Magic0C = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(14)),
            IndexInto03 = BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(16)),
        }).ToList();

        return elements;
    }

    public class Msh0DElement
    {
        public uint Flags { get; set; }
        public uint Magic04 { get; set; }
        public ushort number_of_triangles { get; set; }
        public int IndexInto06 { get; set; }
        public ushort Magic0C { get; set; }
        public int IndexInto03 { get; set; }
    }
}