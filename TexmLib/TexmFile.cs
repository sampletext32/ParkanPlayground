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
/// <param name="FormatOptionFlags">Дополнительные флаги для текстуры</param>
/// <param name="Format">Формат пикселя(4444, 8888, 888)</param>
public record TexmHeader(
    string TexmAscii,
    int Width,
    int Height,
    int MipmapCount,
    int Stride,
    int Magic1,
    int FormatOptionFlags,
    int Format
);

/// <summary>
/// В конце файла есть секция Page, она содержит информацию об атласе самого большого мипмапа
/// </summary>
/// <param name="Page">Заголовок секции</param>
/// <param name="Count">Кол-во элементов</param>
/// <param name="Items">Элементы</param>
public record PageHeader(string Page, int Count, List<PageItem> Items);

/// <summary>Элемент PAGE-секции TEXM.</summary>
/// <param name="X">X-координата в атласе.</param>
/// <param name="Width">Ширина элемента в атласе.</param>
/// <param name="Y">Y-координата в атласе.</param>
/// <param name="Height">Высота элемента в атласе.</param>
public record PageItem(short X, short Width, short Y, short Height);

/// <summary>TEXM файл.</summary>
/// <param name="FileName">Исходное имя файла текстуры TEXM.</param>
/// <param name="Header">Заголовок файла (length = 32).</param>
/// <param name="MipmapBytes">Байты mipmap уровней.</param>
/// <param name="Pages">PAGE-секция атласа, если присутствует.</param>
/// <param name="IsIndexed">Признак indexed texture с lookup таблицей на 1024 байта.</param>
/// <param name="LookupColors">Lookup таблица цветов: 256 цветов * 4 байта.</param>
public record class TexmFile(
    string FileName,
    TexmHeader Header,
    List<byte[]> MipmapBytes,
    PageHeader? Pages,
    bool IsIndexed,
    byte[] LookupColors)
{
    public Task WriteToFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var outputDir = Path.Combine(folder, Path.GetFileName(FileName));
        Directory.CreateDirectory(outputDir);

        if (IsIndexed)
        {
            for (var i = 0; i < Header.MipmapCount; i++)
            {
                var mipWidth = Math.Max(1, Header.Width >> i);
                var mipHeight = Math.Max(1, Header.Height >> i);
                var reinterpretedPixels = ReinterpretIndexedMipmap(MipmapBytes[i], LookupColors);
                
                var image = Image.LoadPixelData<Rgba32>(reinterpretedPixels, mipWidth, mipHeight);
            
                image.SaveAsPng(Path.Combine(outputDir, Path.GetFileName(FileName)) + $"_{mipWidth}x{mipHeight}_indexed.png");
            }

            return Task.CompletedTask;
        }

        for (var i = 0; i < Header.MipmapCount; i++)
        {
            var mipWidth = Math.Max(1, Header.Width >> i);
            var mipHeight = Math.Max(1, Header.Height >> i);

            var reinterpretedPixels = ReinterpretMipmapBytesAsRgba32(
                MipmapBytes[i],
                mipWidth,
                mipHeight,
                Header.Format
            );

            var image = Image.LoadPixelData<Rgba32>(reinterpretedPixels, mipWidth, mipHeight);
            
            image.SaveAsPng(Path.Combine(outputDir, Path.GetFileName(FileName)) + $"_{Header.Format}_{mipWidth}x{mipHeight}.png");
        }

        return Task.CompletedTask;
    }

    public byte[] GetRgba32BytesFromMipmap(int index, out int mipWidth, out int mipHeight)
    {
        var mipmapBytes = MipmapBytes[index];
        
        mipWidth = Math.Max(1, Header.Width >> index);
        mipHeight = Math.Max(1, Header.Height >> index);

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
            556 => ReinterpretAs556(bytes, mipWidth, mipHeight),
            88 => ReinterpretAs88(bytes, mipWidth, mipHeight),
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
            var rawPixel = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(i, 2));

            var r = (byte)(((rawPixel >> 11) & 0b11111) * 255 / 31);
            var g = (byte)(((rawPixel >> 5) & 0b111111) * 255 / 63);
            var b = (byte)((rawPixel & 0b11111) * 255 / 31);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = 0xff;
        }

        return result;
    }

    private byte[] ReinterpretAs556(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 2)
        {
            var rawPixel = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(i, 2));

            var r = (byte)(((rawPixel >> 11) & 0b11111) * 255 / 31);
            var g = (byte)(((rawPixel >> 6) & 0b11111) * 255 / 31);
            var b = (byte)((rawPixel & 0b111111) * 255 / 63);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = 0xff;
        }

        return result;
    }

    private byte[] ReinterpretAs4444(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 2)
        {
            var rawPixel = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(i, 2));

            var a = (byte)(((rawPixel >> 12) & 0b1111) * 17);
            var r = (byte)(((rawPixel >> 8) & 0b1111) * 17);
            var g = (byte)(((rawPixel >> 4) & 0b1111) * 17);
            var b = (byte)((rawPixel & 0b1111) * 17);

            result[i / 2 * 4 + 0] = r;
            result[i / 2 * 4 + 1] = g;
            result[i / 2 * 4 + 2] = b;
            result[i / 2 * 4 + 3] = a;
        }

        return result;
    }

    private byte[] ReinterpretAs88(byte[] bytes, int mipWidth, int mipHeight)
    {
        var span = bytes.AsSpan();

        var result = new byte[bytes.Length * 2];
        for (var i = 0; i < span.Length; i += 2)
        {
            var l = span[i];
            var a = span[i + 1];

            result[i / 2 * 4 + 0] = l;
            result[i / 2 * 4 + 1] = l;
            result[i / 2 * 4 + 2] = l;
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

            result[i + 0] = rawPixel[0];
            result[i + 1] = rawPixel[1];
            result[i + 2] = rawPixel[2];
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
            // swap endianess back
            // (rawPixel[0], rawPixel[1], rawPixel[2], rawPixel[3]) = (rawPixel[3], rawPixel[2], rawPixel[1], rawPixel[0]);

            var r = rawPixel[0];
            var g = rawPixel[1];
            var b = rawPixel[2];
            var a = rawPixel[3];

            result[i + 0] = r;
            result[i + 1] = g;
            result[i + 2] = b;
            result[i + 3] = 255;
            
            // swap endianess back
            // (rawPixel[0], rawPixel[1], rawPixel[2], rawPixel[3]) = (rawPixel[3], rawPixel[2], rawPixel[1], rawPixel[0]);
        }

        return result;
    }
}
