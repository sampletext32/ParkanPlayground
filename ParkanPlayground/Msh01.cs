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
                    BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 6))
            };
            
            element.Lod[0] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 8));
            element.Lod[1] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 10));
            element.Lod[2] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 12));
            element.Lod[3] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 14));
            element.Lod[4] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 16));
            element.Lod[5] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 18));
            element.Lod[6] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 20));
            element.Lod[7] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 22));
            element.Lod[8] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 24));
            element.Lod[9] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 26));
            element.Lod[10] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 28));
            element.Lod[11] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 30));
            element.Lod[12] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 32));
            element.Lod[13] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 34));
            element.Lod[14] = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(i * headerFileEntry.ElementSize + 36));
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
        public ushort[] Lod { get; set; } = new ushort[15];
    }
}