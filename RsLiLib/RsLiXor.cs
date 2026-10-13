namespace RsLiLib;

/// <summary>
/// Потоковый XOR-шифр NL-архива: им зашифрована таблица записей (ключ - младшие 16 бит поля +0x14 заголовка) и данные записей с XOR-способами.
/// Алгоритм из <c>rsOpenLib</c> / <c>rsLoad</c> (Ngi32.dll 0x10013760, 0x10013C60): два байта состояния <c>x</c> (младший байт ключа) и <c>y</c> (старший).
/// </summary>
public static class RsLiXor
{
    /// <summary>Применяет шифр (шифрование и расшифровка совпадают).</summary>
    /// <param name="data">Исходные байты.</param>
    /// <param name="key">16-битный ключ.</param>
    public static byte[] Apply(ReadOnlySpan<byte> data, ushort key)
    {
        var result = new byte[data.Length];
        var x = (byte)(key & 0xFF);
        var y = (byte)(key >> 8);

        for (var i = 0; i < data.Length; i++)
        {
            var gamma = (byte)((x << 1) ^ y);
            x = gamma;
            y = (byte)((y >> 1) ^ gamma);
            result[i] = (byte)(data[i] ^ gamma);
        }

        return result;
    }
}
