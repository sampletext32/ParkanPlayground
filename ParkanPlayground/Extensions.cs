using System.Buffers.Binary;
using System.Text;

namespace ParkanPlayground;

public static class Extensions
{
    public static int ReadInt32LittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }

    public static float ReadFloatLittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadSingleLittleEndian(buf);
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