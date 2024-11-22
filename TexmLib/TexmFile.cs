using System.Buffers.Binary;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TexmLib;

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

public class TexmFile
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

    public byte[] GetRgba32BytesFromMipmap(int index, out int mipWidth, out int mipHeight)
    {
        var mipmapBytes = MipmapBytes[index];
        
        mipWidth = Header.Width / (int) Math.Pow(2, index);
        mipHeight = Header.Height / (int) Math.Pow(2, index);

        if (IsIndexed)
        {
            return ReinterpretIndexedMipmap(mipmapBytes, LookupColors);
        }

        return ReinterpretMipmapBytesAsRgba32(
            mipmapBytes,
            mipWidth,
            mipHeight,
            Header.Format
        );
    }

    private byte[] ReinterpretIndexedMipmap(byte[] bytes, byte[] lookupColors)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 4];
        for (var i = 0; i < span.Length; i++)
        {
            var index = span[i];

            var r = lookupColors[index * 4 + 0];
            var g = lookupColors[index * 4 + 1];
            var b = lookupColors[index * 4 + 2];
            var a = lookupColors[index * 4 + 3];

            result[i * 4 + 0] = r;
            result[i * 4 + 1] = g;
            result[i * 4 + 2] = b;
            result[i * 4 + 3] = 255;
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

            var g = (byte)(((rawPixel[0] >> 3) & 0b11111) * 255 / 31);
            var b = (byte)((((rawPixel[0] & 0b111) << 3) | ((rawPixel[1] >> 5) & 0b111)) * 255 / 63);
            var r = (byte)((rawPixel[1] & 0b11111) * 255 / 31);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = r;
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

            var a = (byte)(((rawPixel[0] >> 4) & 0b1111) * 17);
            var b = (byte)(((rawPixel[0] >> 0) & 0b1111) * 17);
            var g = (byte)(((rawPixel[1] >> 4) & 0b1111) * 17);
            var r = (byte)(((rawPixel[1] >> 0) & 0b1111) * 17);

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

            var r = rawPixel[0];
            var g = rawPixel[1];
            var b = rawPixel[2];
            var w = rawPixel[3];

            result[i + 0] = r;
            result[i + 1] = g;
            result[i + 2] = b;
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

            var b = rawPixel[0];
            var g = rawPixel[1];
            var r = rawPixel[2];
            var a = rawPixel[3];

            result[i + 0] = r;
            result[i + 1] = g;
            result[i + 2] = b;
            result[i + 3] = a;
        }

        return result;
    }
}