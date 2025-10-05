using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

public static class Msh15
{
    public static List<Msh15Element> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "15 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (15)");
        }

        var data = new byte[entry.ElementCount * entry.ElementSize];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(28);

        var elements = elementBytes.Select(x => new Msh15Element()
        {
            Flags = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0)),
            Magic04 = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(4)),
            Vertex1Index = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(8)),
            Vertex2Index = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(10)),
            Vertex3Index = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(12)),
            Magic0E = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(14)),
            Magic12 = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(18)),
            Magic16 = BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(22)),
            Magic1A = BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(26)),
            
        }).ToList();

        return elements;
    }

    public class Msh15Element
    {
        public uint Flags { get; set; }

        public uint Magic04 { get; set; }
        public ushort Vertex1Index { get; set; }
        public ushort Vertex2Index { get; set; }
        public ushort Vertex3Index { get; set; }
        
        public uint Magic0E { get; set; }
        public uint Magic12 { get; set; }
        public uint Magic16 { get; set; }
        public ushort Magic1A { get; set; }
    }
}