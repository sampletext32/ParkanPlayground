using Common;

namespace TexmLib;

public class TexmParser
{
    public static TexmParseResult ReadFromStream(Stream stream, string file)
    {
        if (stream.Length < 32)
        {
            return new TexmParseResult(null, "Файл короче 32 байт - точно не текстура");
        }

        var texmAscii = stream.ReadFixedString(4);
        var width = stream.ReadInt32LittleEndian();
        var height = stream.ReadInt32LittleEndian();
        var mipmapCount = stream.ReadInt32LittleEndian();
        var flags4 = stream.ReadInt32LittleEndian();
        var flags5 = stream.ReadInt32LittleEndian();
        var unk6 = stream.ReadInt32LittleEndian();
        var format = stream.ReadInt32LittleEndian();

        if (texmAscii != "Texm")
        {
            return new TexmParseResult(null, "Файл не начинается с Texm");
        }
        
        var header = new TexmHeader(
            texmAscii,
            width,
            height,
            mipmapCount,
            flags4,
            flags5,
            unk6,
            format
        );

        List<byte[]> mipmapBytesList;
        var isIndexed = false;
        byte[] lookupColors = [];

        if (format == 0)
        {
            // если формат 0, то текстура использует lookup таблицу в первых 1024 байтах (256 разных цветов в формате ARGB 888)

            lookupColors = new byte[1024];
            stream.ReadExactly(lookupColors, 0, lookupColors.Length);

            mipmapBytesList = ReadMipmapsAsIndexes(
                stream,
                mipmapCount,
                width,
                height
            );

            isIndexed = true;
        }
        else
        {
            mipmapBytesList = ReadMipmaps(
                stream,
                format.AsStride(),
                mipmapCount,
                width,
                height
            );
        }

        PageHeader? pages = null;
        if (stream.Position < stream.Length)
        {
            // has PAGE data
            pages = ReadPage(stream);
        }

        var textureFile = new TexmFile(file, header, mipmapBytesList, pages, isIndexed, lookupColors);
        return new TexmParseResult(textureFile);
    }

    private static PageHeader ReadPage(Stream stream)
    {
        var pageHeaderAscii = stream.ReadFixedString(4);
        var pageCount = stream.ReadInt32LittleEndian();

        List<PageItem> pageItems = [];

        for (int i = 0; i < pageCount; i++)
        {
            var x = stream.ReadInt16LittleEndian();
            var pageWidth = stream.ReadInt16LittleEndian();
            var y = stream.ReadInt16LittleEndian();
            var pageHeight = stream.ReadInt16LittleEndian();

            pageItems.Add(new PageItem(x, pageWidth, y, pageHeight));
        }

        var pageHeader = new PageHeader(pageHeaderAscii, pageCount, pageItems);
        return pageHeader;
    }

    private static List<byte[]> ReadMipmaps(Stream stream, int stride, int mipmapCount, int topWidth, int topHeight)
    {
        List<int> mipmapByteLengths = [];

        for (int i = 0; i < mipmapCount; i++)
        {
            var mipWidth = Math.Max(1, topWidth >> i);
            var mipHeight = Math.Max(1, topHeight >> i);

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
            var mipWidth = Math.Max(1, topWidth >> i);
            var mipHeight = Math.Max(1, topHeight >> i);

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
