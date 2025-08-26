using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

public static class Msh01
{
    public static Msh01Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var headerFileEntry = archive.Files.FirstOrDefault(x => x.FileType == "01 00 00 00");

        if (headerFileEntry is null)
        {
            throw new Exception("Archive doesn't contain header file (01)");
        }

        var data = new byte[headerFileEntry.ElementCount * headerFileEntry.ElementSize];
        mshFs.Seek(headerFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var dataSpan = data.AsSpan();

        var elements = new List<SubMesh>((int)headerFileEntry.ElementCount);
        for (var i = 0; i < headerFileEntry.ElementCount; i++)
        {
            var element = new SubMesh()
            {
                Type1 = dataSpan[i * headerFileEntry.ElementSize + 0],
                Type2 = dataSpan[i * headerFileEntry.ElementSize + 1],
                ParentIndex =
                    BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 2)),
                OffsetIntoFile13 =
                    BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 4)),
                IndexInFile08 =
                    BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 6)),
                State00 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 8)),
                State01 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 10)),
                State10 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 12)),
                State11 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 14)),
                State20 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 16)),
                State21 = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 18)),
                S20 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 20)),
                S22 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 22)),
                S24 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 24)),
                S26 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 26)),
                S28 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 28)),
                S30 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 30)),
                S32 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 32)),
                S34 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 34)),
                S36 = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 36))
            };
            elements.Add(element);
        }

        return new Msh01Component()
        {
            Elements = elements
        };
    }


    public class Msh01Component
    {
        public List<SubMesh> Elements { get; set; }
    }

    public class SubMesh
    {
        public byte Type1 { get; set; }
        public byte Type2 { get; set; }
        public short ParentIndex { get; set; }
        public short OffsetIntoFile13 { get; set; }
        public short IndexInFile08 { get; set; }
        public ushort State00 { get; set; }
        public ushort State01 { get; set; }
        public ushort State10 { get; set; }
        public ushort State11 { get; set; }
        public ushort State20 { get; set; }
        public ushort State21 { get; set; }
        public short S20 { get; set; }
        public short S22 { get; set; }
        public short S24 { get; set; }
        public short S26 { get; set; }
        public short S28 { get; set; }
        public short S30 { get; set; }
        public short S32 { get; set; }
        public short S34 { get; set; }
        public short S36 { get; set; }
    }
}