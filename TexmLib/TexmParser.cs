using System.Buffers.Binary;
using System.Text;

namespace TexmLib;

public class TexmParser
{
    public static TexmParseResult ReadFromStream(Stream stream, string file)
    {
        if (stream.Length < 32)
        {
            return new TexmParseResult(null, "Файл короче 32 байт - точно не текстура");
        }

        Span<byte> headerBytes = stackalloc byte[32];
        stream.ReadExactly(headerBytes);

        var texmHeader = headerBytes[0..4];

        var widthBytes = headerBytes[4..8];
        var heightBytes = headerBytes[8..12];
        var mipmapCountBytes = headerBytes[12..16];
        var strideBytes = headerBytes[16..20];
        var magic1Bytes = headerBytes[20..24];
        var magic2Bytes = headerBytes[24..28];
        var formatBytes = headerBytes[28..32];

        var texmAscii = Encoding.ASCII.GetString(texmHeader).Trim('\0');
        var width = BinaryPrimitives.ReadInt32LittleEndian(widthBytes);
        var height = BinaryPrimitives.ReadInt32LittleEndian(heightBytes);
        var mipmapCount = BinaryPrimitives.ReadInt32LittleEndian(mipmapCountBytes);
        var stride = BinaryPrimitives.ReadInt32LittleEndian(strideBytes);
        var magic1 = BinaryPrimitives.ReadInt32LittleEndian(magic1Bytes);
        var magic2 = BinaryPrimitives.ReadInt32LittleEndian(magic2Bytes);
        var format = BinaryPrimitives.ReadInt32LittleEndian(formatBytes);

        if (texmAscii != "Texm")
        {
            return new TexmParseResult(null, "Файл не начинается с Texm");
        }
        
        var textureFile = new TexmFile()
        {
            FileName = file
        };

        var header = new TexmHeader(
            texmAscii,
            width,
            height,
            mipmapCount,
            stride,
            magic1,
            magic2,
            format
        );

        textureFile.Header = header;

        if (format == 0)
        {
            // если формат 0, то текстура использует lookup таблицу в первых 1024 байтах (256 разных цветов в формате ARGB 888)

            var lookupColors = new byte[1024];
            stream.ReadExactly(lookupColors, 0, lookupColors.Length);

            textureFile.LookupColors = lookupColors;

            var mipmapBytesList = ReadMipmapsAsIndexes(
                stream,
                mipmapCount,
                width,
                height
            );

            textureFile.MipmapBytes = mipmapBytesList;
            textureFile.IsIndexed = true;
        }
        else
        {
            var mipmapBytesList = ReadMipmaps(
                stream,
                format.AsStride(),
                mipmapCount,
                width,
                height
            );

            textureFile.MipmapBytes = mipmapBytesList;
        }

        if (stream.Position < stream.Length)
        {
            // has PAGE data
            var pageHeader = ReadPage(stream);

            textureFile.Pages = pageHeader;
        }

        return new TexmParseResult(textureFile);
    }

    private static PageHeader ReadPage(Stream stream)
    {
        Span<byte> pageBytes = stackalloc byte[4];

        stream.ReadExactly(pageBytes);

        var pageHeaderAscii = Encoding.ASCII.GetString(pageBytes);

        stream.ReadExactly(pageBytes);

        var pageCount = BinaryPrimitives.ReadInt32LittleEndian(pageBytes);

        List<PageItem> pageItems = [];

        Span<byte> itemBytes = stackalloc byte[2];
        for (int i = 0; i < pageCount; i++)
        {
            stream.ReadExactly(itemBytes);
            var x = BinaryPrimitives.ReadInt16LittleEndian(itemBytes);
            stream.ReadExactly(itemBytes);
            var pageWidth = BinaryPrimitives.ReadInt16LittleEndian(itemBytes);
            stream.ReadExactly(itemBytes);
            var y = BinaryPrimitives.ReadInt16LittleEndian(itemBytes);
            stream.ReadExactly(itemBytes);
            var pageHeight = BinaryPrimitives.ReadInt16LittleEndian(itemBytes);

            pageItems.Add(
                new PageItem(
                    x,
                    pageWidth,
                    y,
                    pageHeight
                )
            );
        }

        var pageHeader = new PageHeader(pageHeaderAscii, pageCount, pageItems);
        return pageHeader;
    }

    private static List<byte[]> ReadMipmaps(Stream stream, int stride, int mipmapCount, int topWidth, int topHeight)
    {
        if (stride == 0)
        {
            stride = 16;
        }

        List<int> mipmapByteLengths = [];

        for (int i = 0; i < mipmapCount; i++)
        {
            var mipWidth = topWidth / (int) Math.Pow(2, i);
            var mipHeight = topHeight / (int) Math.Pow(2, i);

            var imageByteLength = mipWidth * mipHeight * (stride / 8);
            mipmapByteLengths.Add(imageByteLength);
        }

        List<byte[]> mipmapBytesList = [];

        foreach (var mipmapByteLength in mipmapByteLengths)
        {
            var mipmapBuffer = new byte[mipmapByteLength];

            stream.ReadExactly(mipmapBuffer, 0, mipmapByteLength);

            mipmapBytesList.Add(mipmapBuffer);
        }

        return mipmapBytesList;
    }

    private static List<byte[]> ReadMipmapsAsIndexes(Stream stream, int mipmapCount, int topWidth, int topHeight)
    {
        List<int> mipmapByteLengths = [];

        for (int i = 0; i < mipmapCount; i++)
        {
            var mipWidth = topWidth / (int) Math.Pow(2, i);
            var mipHeight = topHeight / (int) Math.Pow(2, i);

            var imageByteLength = mipWidth * mipHeight;
            mipmapByteLengths.Add(imageByteLength);
        }

        List<byte[]> mipmapBytesList = [];

        foreach (var mipmapByteLength in mipmapByteLengths)
        {
            var mipmapBuffer = new byte[mipmapByteLength];

            stream.ReadExactly(mipmapBuffer, 0, mipmapByteLength);

            mipmapBytesList.Add(mipmapBuffer);
        }

        return mipmapBytesList;
    }
}