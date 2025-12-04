using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

public static class Msh02
{
    public static Msh02Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var fileEntry = archive.Files.FirstOrDefault(x => x.FileType == "02 00 00 00");

        if (fileEntry is null)
        {
            throw new Exception("Archive doesn't contain 02 component");
        }

        var data = new byte[fileEntry.FileLength];
        mshFs.Seek(fileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var header = data.AsSpan(0, 0x8c); // 140 bytes header

        var center = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x60)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x64)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x68))
        );
        var centerW = BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x6c));

        var bb = new BoundingBox();
        bb.BottomFrontLeft = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(4)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(8))
        );
        bb.BottomFrontRight = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(12)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(16)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(20))
        );
        bb.BottomBackRight = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(24)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(28)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(32))
        );
        bb.BottomBackLeft = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(36)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(40)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(44))
        );
        bb.TopFrontLeft = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(48)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(52)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(56))
        );
        bb.TopFrontRight = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(60)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(64)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(68))
        );
        bb.TopBackRight = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(72)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(76)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(80))
        );
        bb.TopBackLeft = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(84)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(88)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(92))
        );

        var bottom = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(112)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(116)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(120))
        );

        var top = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(124)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(128)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(132))
        );

        var xyRadius = BinaryPrimitives.ReadSingleLittleEndian(header.Slice(136));


        List<Msh02Element> elements = new List<Msh02Element>();
        var skippedHeader = data.AsSpan(0x8c); // skip header
        for (var i = 0; i < fileEntry.ElementCount; i++)
        {
            var element = new Msh02Element();
            element.StartIndexIn07 =
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 0));
            element.CountIn07 =
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 2));
            element.StartOffsetIn0d =
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 4));
            element.ByteLengthIn0D =
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 6));
            element.LocalMinimum = new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 8)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 12)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 16))
            );
            element.LocalMaximum = new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 20)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 24)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 28))
            );
            element.Center = new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 32)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 36)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 40))
            );
            element.Vector4 = new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 44)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 48)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 52))
            );
            element.Vector5 = new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 56)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 60)),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(fileEntry.ElementSize * i + 64))
            );
            elements.Add(element);

            _ = 5;
        }

        return new Msh02Component()
        {
            Header = new Msh02Header()
            {
                BoundingBox = bb,
                Center = center,
                CenterW = centerW,
                Bottom = bottom,
                Top = top,
                XYRadius = xyRadius
            },
            Elements = elements
        };
    }

    public class Msh02Component
    {
        public Msh02Header Header { get; set; }
        public List<Msh02Element> Elements { get; set; }
    }

    /// <summary>
    /// 140 байт в начале файла
    /// </summary>
    public class Msh02Header
    {
        public BoundingBox BoundingBox { get; set; }
        public Vector3 Center { get; set; }
        public float CenterW { get; set; }
        public Vector3 Bottom { get; set; }
        public Vector3 Top { get; set; }
        public float XYRadius { get; set; }
    }

    public class Msh02Element
    {
        public ushort StartIndexIn07 { get; set; }
        public ushort CountIn07 { get; set; }
        public ushort StartOffsetIn0d { get; set; }
        public ushort ByteLengthIn0D { get; set; }
        public Vector3 LocalMinimum { get; set; }
        public Vector3 LocalMaximum { get; set; }
        public Vector3 Center { get; set; }
        public Vector3 Vector4 { get; set; }
        public Vector3 Vector5 { get; set; }
    }

    /// <summary>
    /// 96 bytes - bounding box (8 points each 3 float = 96 bytes)
    /// 0x60 bytes or 0x18 by 4 bytes
    /// </summary>
    public class BoundingBox
    {
        public Vector3 BottomFrontLeft { get; set; }
        public Vector3 BottomFrontRight { get; set; }
        public Vector3 BottomBackRight { get; set; }
        public Vector3 BottomBackLeft { get; set; }
        public Vector3 TopBackRight { get; set; }
        public Vector3 TopFrontRight { get; set; }
        public Vector3 TopBackLeft { get; set; }
        public Vector3 TopFrontLeft { get; set; }
    }
}