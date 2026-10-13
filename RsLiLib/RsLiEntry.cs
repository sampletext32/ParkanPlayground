namespace RsLiLib;

/// <summary>
/// Запись NL-архива (32 байта в зашифрованной таблице, <c>NLDecryptedHeader</c> в Ghidra).
/// </summary>
/// <param name="Index">Номер записи в таблице.</param>
/// <param name="Name">[+0x00] Имя файла, до 12 символов в верхнем регистре (при загрузке игра дописывает нули до 16 байт).</param>
/// <param name="Unknown0C">[+0x0C] Не читается игрой (при загрузке имя обрезается до 12 байт, это поле обнуляется); в данных выглядит как случайное число.</param>
/// <param name="MethodFlags">[+0x10] Флаги упаковки; способ - <see cref="Method"/>.</param>
/// <param name="SortIndexOrKey">[+0x12] 16 бит: при флаге «таблица отсортирована» (0xABBA в заголовке) - индекс записи, стоящей на этой позиции в порядке
/// сортировки по имени (по нему <c>rsFind</c> ищет бинарным поиском); для XOR-способов <c>rsLoad</c> читает из него же ключ шифра данных.</param>
/// <param name="UnpackedSize">[+0x14] Размер распакованных данных.</param>
/// <param name="DataOffset">[+0x18] Смещение упакованных данных от начала файла (плюс <see cref="RsLiArchive.OptionalOffset"/>).</param>
/// <param name="PackedSize">[+0x1C] Размер упакованных данных в файле.</param>
public sealed record RsLiEntry(
    int Index,
    string Name,
    uint Unknown0C,
    ushort MethodFlags,
    ushort SortIndexOrKey,
    int UnpackedSize,
    int DataOffset,
    int PackedSize)
{
    /// <summary>Способ упаковки (биты 0x1E0 поля <see cref="MethodFlags"/>).</summary>
    public RsLiPackMethod Method => (RsLiPackMethod)(MethodFlags & 0x1E0);
}
