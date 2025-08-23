using System.Buffers.Binary;
using System.Text;

namespace Common;

public static class Extensions
{
    public static int ReadInt32LittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }

    public static uint ReadUInt32LittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }

    public static float ReadFloatLittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadSingleLittleEndian(buf);
    }

    public static string ReadNullTerminatedString(this FileStream fs)
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

    public static string ReadLengthPrefixedString(this FileStream fs)
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
}