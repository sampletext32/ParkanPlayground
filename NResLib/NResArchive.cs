namespace NResLib;

/// <summary>
/// Архив NRes (файл NRes)
/// </summary>
public record NResArchive(NResArchiveHeader Header, List<ListMetadataItem> Files);

/// <summary>
/// Заголовок файла
/// </summary>
/// <param name="NRes">[0..4] ASCII NRes.</param>
/// <param name="Version">[4..8] Версия кодировщика, обычно 0x100.</param>
/// <param name="FileCount">[8..12] Количество записей каталога.</param>
/// <param name="TotalFileLengthBytes">[12..16] Длина всего архива.</param>
public record NResArchiveHeader(string NRes, int Version, int FileCount, int TotalFileLengthBytes);

/// <summary>
/// 64-байтная запись каталога NRes.
///
/// Важный нюанс RE-имен:
/// - ElementCount/Magic1/ElementSize оставлены как удобные имена для исследования, но на уровне
///   общего NRes-контейнера это attr1/attr2/attr3. Их смысл зависит от TypeId ресурса.
/// - FileName на диске занимает 36 байт: [20..56]. Старый разбор читал только [20..40].
/// - Старые Magic3..Magic6 оказались не отдельными полями формата, а байтами хвоста name_raw.
/// - SortIndex на диске это перестановка для бинарного поиска по имени, а не порядковый
///   индекс записи каталога. Порядковый индекс хранится отдельно в DirectoryIndex.
/// </summary>
/// <param name="TypeId">[0x00..0x04] Resource type id.</param>
/// <param name="FileType">[0x00..0x04] Resource type id как ASCII или hex bytes для RE.</param>
/// <param name="ElementCount">[0x04..0x08] ElementCount. смысл зависит от TypeId.</param>
/// <param name="Magic1">[0x08..0x0C] смысл зависит от TypeId.</param>
/// <param name="FileLength">[0x0C..0x10] Длина payload в байтах.</param>
/// <param name="ElementSize">[0x10..0x14] ElementSize. смысл зависит от TypeId.</param>
/// <param name="FileName">[0x14..0x38] C-string из 36-байтного name_raw.</param>
/// <param name="OffsetInFile">[0x38..0x3C] Смещение payload от начала архива.</param>
/// <param name="SortIndex">[0x3C..0x40] Индекс перестановки для бинарного поиска по имени.</param>
/// <param name="DirectoryIndex">Индекс записи в порядке каталога; не хранится отдельным полем NRes.</param>
public sealed record class ListMetadataItem(
    uint TypeId,
    string FileType,
    uint ElementCount,
    int Magic1,
    int FileLength,
    int ElementSize,
    string FileName,
    int OffsetInFile,
    int SortIndex,
    int DirectoryIndex);
