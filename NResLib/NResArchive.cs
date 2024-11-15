namespace NResLib;

/// <summary>
/// Архив NRes (файл NRes)
/// </summary>
public record NResArchive(NResArchiveHeader Header, List<ListMetadataItem> Files);

/// <summary>
/// Заголовок файла
/// </summary>
/// <param name="NRes">[0..4] ASCII NRes</param>
/// <param name="Version">[4..8] Версия кодировщика (должно быть всегда 0x100)</param>
/// <param name="FileCount">[8..12] Количество файлов </param>
/// <param name="TotalFileLengthBytes">[12..16] Длина всего архива</param>
public record NResArchiveHeader(string NRes, int Version, int FileCount, int TotalFileLengthBytes);

/// <summary>
/// В конце файла есть список метаданных,
/// каждый элемент это 64 байта,
/// найти начало можно как (Header.TotalFileLengthBytes - Header.FileCount * 64)
/// </summary>
/// <param name="FileType">[0..8] ASCII описание типа файла, например TEXM или MAT0</param>
/// <param name="Magic1">[8..12] Неизвестное число</param>
/// <param name="FileLength">[12..16] Длина файла в байтах</param>
/// <param name="Magic2">[16..20] Неизвестное число</param>
/// <param name="FileName">[20..40] ASCII имя файла</param>
/// <param name="Magic3">[40..44] Неизвестное число</param>
/// <param name="Magic4">[44..48] Неизвестное число</param>
/// <param name="Magic5">[48..52] Неизвестное число</param>
/// <param name="Magic6">[52..56] Неизвестное число</param>
/// <param name="OffsetInFile">[56..60] Смещение подфайла от начала NRes (именно самого NRes) в байтах</param>
/// <param name="Index">[60..64] Индекс в файле (от 0, не больше чем кол-во файлов)</param>
public record ListMetadataItem(
    string FileType,
    int Magic1,
    int FileLength,
    int Magic2,
    string FileName,
    int Magic3,
    int Magic4,
    int Magic5,
    int Magic6,
    int OffsetInFile,
    int Index
);