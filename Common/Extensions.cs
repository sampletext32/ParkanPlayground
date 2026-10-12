using System.Buffers.Binary;
using System.Text;

namespace Common;

public static class Extensions
{
    public static int ReadInt32LittleEndian(this Stream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }

    public static uint ReadUInt32LittleEndian(this Stream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }

    public static ushort ReadUInt16LittleEndian(this Stream fs)
    {
        Span<byte> buf = stackalloc byte[2];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadUInt16LittleEndian(buf);
    }

    public static short ReadInt16LittleEndian(this Stream fs)
    {
        Span<byte> buf = stackalloc byte[2];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadInt16LittleEndian(buf);
    }

    public static float ReadFloatLittleEndian(this Stream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadSingleLittleEndian(buf);
    }

    public static string ReadNullTerminatedString(this Stream fs)
    {
        var sb = new StringBuilder();

        while (true)
        {
            var b = fs.ReadByte();
            if (b == 0)
            {
                break;
            }

            sb.Append((char)b);
        }

        return sb.ToString();
    }

    public static string ReadNullTerminated1251String(this Stream fs)
    {
        var sb = new StringBuilder();

        while (true)
        {
            var b = (byte)fs.ReadByte();
            if (b == 0)
            {
                break;
            }
            
            sb.Append(Encoding.GetEncoding("windows-1251").GetString([b]));
        }

        return sb.ToString();
    }

    public static string ReadLengthPrefixedString(this Stream fs)
    {
        var len = fs.ReadInt32LittleEndian();

        if (len == 0)
        {
            return "";
        }

        var buffer = new byte[len];

        fs.ReadExactly(buffer, 0, len);

        return Encoding.ASCII.GetString(buffer, 0, len);
    }

    /// <summary>Читает ровно <paramref name="count"/> байт или бросает исключение, если поток короче.</summary>
    /// <param name="fs">Поток.</param>
    /// <param name="count">Число байт.</param>
    public static byte[] ReadBytesExact(this Stream fs, int count)
    {
        var bytes = new byte[count];
        fs.ReadExactly(bytes, 0, count);

        return bytes;
    }

    /// <summary>Строка из буфера фиксированного размера: ASCII до первого нуля (остаток буфера игнорируется).</summary>
    /// <param name="bytes">Содержимое буфера.</param>
    public static string ReadFixedString(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        if (end < 0)
        {
            end = bytes.Length;
        }

        return Encoding.ASCII.GetString(bytes[..end]);
    }

    /// <summary>Читает буфер <paramref name="byteCount"/> байт и возвращает ASCII-строку до первого нуля.</summary>
    /// <param name="fs">Поток.</param>
    /// <param name="byteCount">Размер буфера в байтах.</param>
    public static string ReadFixedString(this Stream fs, int byteCount) =>
        ReadFixedString(fs.ReadBytesExact(byteCount));

    /// <summary>Читает три float подряд (12 байт).</summary>
    /// <param name="fs">Поток.</param>
    public static Vector3 ReadVector3LittleEndian(this Stream fs) =>
        new(fs.ReadFloatLittleEndian(), fs.ReadFloatLittleEndian(), fs.ReadFloatLittleEndian());

    /// <summary>Читает массив int32 фиксированной длины.</summary>
    /// <param name="fs">Поток.</param>
    /// <param name="count">Число элементов.</param>
    public static int[] ReadInt32ArrayLittleEndian(this Stream fs, int count)
    {
        var values = new int[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = fs.ReadInt32LittleEndian();
        }

        return values;
    }

    /// <summary>Читает ссылку на элемент NRes (64 байта).</summary>
    /// <param name="fs">Поток.</param>
    public static ResourceNamedRef ReadResourceNamedRef(this Stream fs) =>
        ResourceNamedRef.Read(fs.ReadBytesExact(ResourceNamedRef.Size));
}
