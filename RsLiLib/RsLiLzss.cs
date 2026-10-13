namespace RsLiLib;

/// <summary>
/// LZSS в варианте игры (<c>FUN_100156BE</c>, Ngi32.dll 0x100156BE, ветка без флага 0x80): кольцевой буфер 4096 байт, до начала заполнен пробелами,
/// позиция записи начинается с 0xFEE; байт флагов читается младшим битом вперёд (1 - литерал, 0 - ссылка из двух байт: 12 бит позиции в буфере и
/// 4 бита длины - 3).
/// </summary>
public static class RsLiLzss
{
    private const int WindowSize = 4096;
    private const int MaxMatch = 18;

    /// <summary>Распаковывает данные.</summary>
    /// <param name="source">Упакованные байты.</param>
    /// <param name="unpackedSize">Ожидаемый размер результата.</param>
    public static byte[] Decode(ReadOnlySpan<byte> source, int unpackedSize)
    {
        var window = new byte[WindowSize];
        Array.Fill(window, (byte)' ', 0, WindowSize - MaxMatch);

        var writePos = WindowSize - MaxMatch;
        var output = new byte[unpackedSize];
        var outputLength = 0;
        var inputPos = 0;
        var flags = 0;

        while (outputLength < unpackedSize)
        {
            flags >>= 1;
            if ((flags & 0x100) == 0)
            {
                if (inputPos >= source.Length)
                    break;

                flags = source[inputPos++] | 0xFF00;
            }

            if ((flags & 1) != 0)
            {
                if (inputPos >= source.Length)
                    break;

                var literal = source[inputPos++];
                output[outputLength++] = literal;
                window[writePos] = literal;
                writePos = (writePos + 1) & (WindowSize - 1);
                continue;
            }

            if (inputPos + 1 >= source.Length)
                break;

            var low = source[inputPos++];
            var high = source[inputPos++];
            var position = low | ((high & 0xF0) << 4);
            var length = (high & 0x0F) + 3;

            for (var i = 0; i < length && outputLength < unpackedSize; i++)
            {
                var value = window[(position + i) & (WindowSize - 1)];
                output[outputLength++] = value;
                window[writePos] = value;
                writePos = (writePos + 1) & (WindowSize - 1);
            }
        }

        return output;
    }
}
