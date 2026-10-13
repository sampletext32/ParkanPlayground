using System.Buffers.Binary;
using System.IO.Compression;

namespace RsLiLib;

/// <summary>
/// NL-архив (в коде игры - «RsLi», <c>rsOpenLib</c>/<c>rsLoad</c>/<c>rsFind</c> в Ngi32.dll): плоский список именованных записей с упаковкой по записи.
/// В первой части это <c>gamefont.rlb</c> (шрифты, палитра) и <c>sprites.lib</c> (текстуры). Формат, подтверждённый кодом игры и данными:
/// <code>
/// 0x00  "NL", 0x00, 0x01      сигнатура и версия
/// 0x04  int16                 число записей N
/// 0x0E  uint16                0xABBA - таблица уже отсортирована по имени (иначе игра сортирует при открытии)
/// 0x14  uint32                ключ XOR-шифра таблицы (используются младшие 16 бит)
/// 0x20  N * 32 байта          таблица записей, зашифрована потоковым XOR (см. RsLiXor), поля - в RsLiEntry
/// ...                         данные записей по смещениям из таблицы
/// -6    "AO", int32           необязательный хвост (при открытии с флагом 2): смещение начала архива внутри файла
/// </code>
/// </summary>
public sealed class RsLiArchive
{
    private readonly byte[] _data;

    private RsLiArchive(byte[] data, IReadOnlyList<RsLiEntry> entries, bool isNameSorted, int optionalOffset)
    {
        _data = data;
        Entries = entries;
        IsNameSorted = isNameSorted;
        OptionalOffset = optionalOffset;
    }

    /// <summary>Записи архива в порядке таблицы.</summary>
    public IReadOnlyList<RsLiEntry> Entries { get; }

    /// <summary>В заголовке стоит 0xABBA: таблица считается отсортированной по имени, и <c>SortIndexOrKey</c> каждой записи - индекс отсортированного порядка.</summary>
    public bool IsNameSorted { get; }

    /// <summary>Смещение начала архива в файле (из хвоста «AO»); 0, если хвоста нет или он не запрашивался.</summary>
    public int OptionalOffset { get; }

    /// <summary>Читает архив из файла.</summary>
    /// <param name="path">Путь к файлу.</param>
    /// <param name="hasOptionalTrailer">Искать хвост «AO» (в игре - флаг 2 у <c>rsOpenLib</c>).</param>
    public static RsLiArchive Read(string path, bool hasOptionalTrailer = false)
    {
        return Read(File.ReadAllBytes(path), hasOptionalTrailer);
    }

    /// <summary>Читает архив из байтов.</summary>
    /// <param name="data">Содержимое файла.</param>
    /// <param name="hasOptionalTrailer">Искать хвост «AO».</param>
    /// <exception cref="InvalidDataException">Сигнатура или размеры не соответствуют NL-архиву.</exception>
    public static RsLiArchive Read(byte[] data, bool hasOptionalTrailer = false)
    {
        var optionalOffset = 0;
        if (hasOptionalTrailer)
        {
            if (data.Length < 6 || BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(data.Length - 6)) != 0x4F41)
                throw new InvalidDataException("Нет хвоста «AO» с смещением архива.");

            optionalOffset = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(data.Length - 4));
        }

        var header = data.AsSpan(optionalOffset);
        if (header.Length < 0x20 || header[0] != (byte)'N' || header[1] != (byte)'L' || header[2] != 0 || header[3] != 1)
            throw new InvalidDataException("Сигнатура не NL\\0\\1.");

        var count = BinaryPrimitives.ReadInt16LittleEndian(header[4..]);
        if (count < 0 || header.Length < 0x20 + count * 32)
            throw new InvalidDataException("Таблица записей выходит за пределы файла.");

        var isNameSorted = BinaryPrimitives.ReadUInt16LittleEndian(header[0x0E..]) == 0xABBA;
        var tableKey = (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(header[0x14..]) & 0xFFFF);
        var table = RsLiXor.Apply(header.Slice(0x20, count * 32), tableKey);

        var entries = new List<RsLiEntry>(count);
        for (var i = 0; i < count; i++)
        {
            var record = table.AsSpan(i * 32, 32);
            var nameBytes = record[..12];
            var nameLength = nameBytes.IndexOf((byte)0);
            var name = System.Text.Encoding.Latin1.GetString(nameBytes[..(nameLength < 0 ? 12 : nameLength)]);

            entries.Add(new RsLiEntry(
                Index: i,
                Name: name,
                Unknown0C: BinaryPrimitives.ReadUInt32LittleEndian(record[0x0C..]),
                MethodFlags: BinaryPrimitives.ReadUInt16LittleEndian(record[0x10..]),
                SortIndexOrKey: BinaryPrimitives.ReadUInt16LittleEndian(record[0x12..]),
                UnpackedSize: BinaryPrimitives.ReadInt32LittleEndian(record[0x14..]),
                DataOffset: BinaryPrimitives.ReadInt32LittleEndian(record[0x18..]),
                PackedSize: BinaryPrimitives.ReadInt32LittleEndian(record[0x1C..])));
        }

        return new RsLiArchive(data, entries, isNameSorted, optionalOffset);
    }

    /// <summary>Ищет запись по имени без учёта регистра (<c>rsFind</c> приводит имя к верхнему регистру).</summary>
    /// <param name="name">Имя записи.</param>
    /// <returns>Запись или null.</returns>
    public RsLiEntry? Find(string name)
    {
        return Entries.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Возвращает распакованные данные записи.</summary>
    /// <param name="entry">Запись этого архива.</param>
    /// <exception cref="InvalidDataException">Данные выходят за пределы файла или способ упаковки неизвестен.</exception>
    public byte[] Extract(RsLiEntry entry)
    {
        var start = OptionalOffset + entry.DataOffset;
        if (entry.PackedSize < 0 || start < 0 || start > _data.Length)
            throw new InvalidDataException($"Данные записи «{entry.Name}» выходят за пределы файла.");

        // Игра читает файл через отображение в память и не проверяет хвост: у последней записи sprites.lib размер на байт больше остатка файла.
        var packed = _data.AsSpan(start, Math.Min(entry.PackedSize, _data.Length - start));

        switch (entry.Method)
        {
            case RsLiPackMethod.None:
                return packed.ToArray();

            case RsLiPackMethod.Xor:
                return RsLiXor.Apply(packed, entry.SortIndexOrKey);

            case RsLiPackMethod.Lzss:
                return RsLiLzss.Decode(packed, entry.UnpackedSize);

            case RsLiPackMethod.XorLzss:
                return RsLiLzss.Decode(RsLiXor.Apply(packed, entry.SortIndexOrKey), entry.UnpackedSize);

            case RsLiPackMethod.Lzhuf:
                return RsLiLzhuf.Decode(packed, entry.UnpackedSize);

            case RsLiPackMethod.XorLzhuf:
                return RsLiLzhuf.Decode(RsLiXor.Apply(packed, entry.SortIndexOrKey), entry.UnpackedSize);

            case RsLiPackMethod.Deflate:
            {
                using var input = new MemoryStream(packed.ToArray());
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                var result = new byte[entry.UnpackedSize];
                deflate.ReadExactly(result);
                return result;
            }

            default:
                throw new InvalidDataException($"Неизвестный способ упаковки 0x{entry.MethodFlags:X} у записи «{entry.Name}».");
        }
    }
}
