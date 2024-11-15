using System.Buffers.Binary;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TextureDecoder;

/// <summary>
/// Заголовок TEXM файла (может быть .0 файл)
/// </summary>
/// <param name="TexmAscii">Строка TEXM</param>
/// <param name="Width">Ширина или высота (не имеет значения, т.к. текстуры всегда квадратные)</param>
/// <param name="Height">Ширина или высота (не имеет значения, т.к. текстуры всегда квадратные)</param>
/// <param name="MipmapCount">Кол-во мипмапов (уменьшенные копии текстуры)</param>
/// <param name="Stride">Сколько БИТ занимает 1 пиксель</param>
/// <param name="Magic1">Неизвестно</param>
/// <param name="Magic2">Неизвестно</param>
/// <param name="Format">Формат пикселя(4444, 8888, 888)</param>
public record TexmHeader(
    string TexmAscii,
    int Width,
    int Height,
    int MipmapCount,
    int Stride,
    int Magic1,
    int Magic2,
    int Format
);

/// <summary>
/// В конце файла есть секция Page, она содержит информацию об атласе самого большого мипмапа
/// </summary>
/// <param name="Page">Заголовок секции</param>
/// <param name="Count">Кол-во элементов</param>
/// <param name="Items">Элементы</param>
public record PageHeader(string Page, int Count, List<PageItem> Items);

public record PageItem(short X, short Width, short Y, short Height);

public class TextureFile
{
    /// <summary>
    /// Исходное имя файла текстуры TEXM
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Заголовок файла, всегда 32 байта
    /// </summary>
    public TexmHeader Header { get; set; }

    /// <summary>
    /// Если в одной текстуре есть несколько MipMap уровней, тут будет несколько отдельных текстур
    /// </summary>
    public List<byte[]> MipmapBytes { get; set; }

    /// <summary>
    /// Если текстура - это атлас, то здесь будет информация о координатах в атласе
    /// </summary>
    public PageHeader? Pages { get; set; }

    /// <summary>
    /// В некоторых случаях, текстура может быть закодирована как lookup таблица на 1024 байта (256 цветов),
    /// тогда сначала идёт 1024 байта lookup таблицы, а далее сами мипмапы, по 1 байту (каждый байт - индекс в lookup таблице)
    /// </summary>
    public bool IsIndexed { get; set; }

    /// <summary>
    /// Lookup таблица цветов (каждый цвет закодирован как 4 байта (ARGB))
    /// </summary>
    public byte[] LookupColors { get; set; }


    private TextureFile()
    {
    }

    public static TextureFile ReadFromStream(Stream stream, string file)
    {
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

        var texmAscii = Encoding.ASCII.GetString(texmHeader);
        var width = BinaryPrimitives.ReadInt32LittleEndian(widthBytes);
        var height = BinaryPrimitives.ReadInt32LittleEndian(heightBytes);
        var mipmapCount = BinaryPrimitives.ReadInt32LittleEndian(mipmapCountBytes);
        var stride = BinaryPrimitives.ReadInt32LittleEndian(strideBytes);
        var magic1 = BinaryPrimitives.ReadInt32LittleEndian(magic1Bytes);
        var magic2 = BinaryPrimitives.ReadInt32LittleEndian(magic2Bytes);
        var format = BinaryPrimitives.ReadInt32LittleEndian(formatBytes);

        var textureFile = new TextureFile()
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

        return textureFile;
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

    public async Task WriteToFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var outputDir = Path.Combine(folder, Path.GetFileName(FileName));
        Directory.CreateDirectory(outputDir);

        if (IsIndexed)
        {
            for (var i = 0; i < Header.MipmapCount; i++)
            {
                var mipWidth = Header.Width / (int) Math.Pow(2, i);
                var mipHeight = Header.Height / (int) Math.Pow(2, i);
                var reinterpretedPixels = ReinterpretIndexedMipmap(MipmapBytes[i], LookupColors);
                
                var image = Image.LoadPixelData<Rgba32>(reinterpretedPixels, mipWidth, mipHeight);
            
                image.SaveAsPng(Path.Combine(outputDir, Path.GetFileName(FileName)) + $"_{mipWidth}x{mipHeight}_indexed.png");
            }

            return;
        }

        for (var i = 0; i < Header.MipmapCount; i++)
        {
            var mipWidth = Header.Width / (int) Math.Pow(2, i);
            var mipHeight = Header.Height / (int) Math.Pow(2, i);

            var reinterpretedPixels = ReinterpretMipmapBytesAsRgba32(
                MipmapBytes[i],
                mipWidth,
                mipHeight,
                Header.Format
            );

            var image = Image.LoadPixelData<Rgba32>(reinterpretedPixels, mipWidth, mipHeight);
            
            image.SaveAsPng(Path.Combine(outputDir, Path.GetFileName(FileName)) + $"_{Header.Format}_{mipWidth}x{mipHeight}.png");
        }
    }

    private byte[] ReinterpretIndexedMipmap(byte[] bytes, byte[] lookupColors)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 4];
        for (var i = 0; i < span.Length; i++)
        {
            var index = span[i];

            var a = lookupColors[index * 4 + 0];
            var r = lookupColors[index * 4 + 1];
            var g = lookupColors[index * 4 + 2];
            var b = lookupColors[index * 4 + 3];

            result[i * 4 + 0] = r;
            result[i * 4 + 1] = g;
            result[i * 4 + 2] = b;
            result[i * 4 + 3] = a;
        }

        return result;
    }

    private byte[] ReinterpretMipmapBytesAsRgba32(byte[] bytes, int mipWidth, int mipHeight, int format)
    {
        var result = format switch
        {
            8888 => ReinterpretAs8888(bytes, mipWidth, mipHeight),
            888 => ReinterpretAs888(bytes, mipWidth, mipHeight),
            4444 => ReinterpretAs4444(bytes, mipWidth, mipHeight),
            565 => ReinterpretAs565(bytes, mipWidth, mipHeight),
            _ => throw new InvalidOperationException($"Invalid format {format}")
        };

        return result;
    }

    private byte[] ReinterpretAs565(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 2)
        {
            var rawPixel = span.Slice(i, 2);

            var r = (byte)(((rawPixel[0] >> 3) & 0b11111) / 32 * 255);
            var g = (byte)(((rawPixel[0] & 0b111) << 3) | ((rawPixel[1] >> 5) & 0b111) / 64 * 255);
            var b = (byte)((rawPixel[1] & 0b11111) / 32 * 255);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = 255;
        }

        return result;
    }

    private byte[] ReinterpretAs4444(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 2)
        {
            var rawPixel = span.Slice(i, 2);

            var a = (byte)((float)((rawPixel[0] >> 4) & 0b1111) / 15 * 255);
            var r = (byte)((float)((rawPixel[0] >> 0) & 0b1111) / 15 * 255);
            var g = (byte)((float)((rawPixel[1] >> 4) & 0b1111) / 15 * 255);
            var b = (byte)((float)((rawPixel[1] >> 0) & 0b1111) / 15 * 255);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = a;
        }

        return result;
    }

    private byte[] ReinterpretAs888(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 4)
        {
            var rawPixel = span.Slice(i, 4);

            var x = rawPixel[0];
            var y = rawPixel[1];
            var z = rawPixel[2];
            var w = rawPixel[3];

            result[i + 0] = y;
            result[i + 1] = z;
            result[i + 2] = w;
            result[i + 3] = 255;
        }

        return result;
    }

    private byte[] ReinterpretAs8888(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length];
        for (var i = 0; i < span.Length; i += 4)
        {
            var rawPixel = span.Slice(i, 4);

            var a = rawPixel[0];
            var r = rawPixel[1];
            var g = rawPixel[2];
            var b = rawPixel[3];

            result[i + 0] = r;
            result[i + 1] = g;
            result[i + 2] = b;
            result[i + 3] = a;
        }

        return result;
    }
}