namespace ControlLib;

/// <summary>
/// Файл повреждений NDP (тип NRes <c>NDPR</c>): вклад каждой детали в жизнь и подвижность объекта и ресурс взрыва детали.
/// Игра загружает его как четвёртую ссылку записи агента; у присоединённых объектов запись 0 пропускается.
/// </summary>
/// <param name="SourceName">Имя исходного файла или <c>null</c>, если читалось из потока.</param>
/// <param name="Count">Число записей. В данных: 417 файлов, 1145 записей, от 1 до 34 в файле.</param>
/// <param name="Records">Записи по деталям в порядке следования.</param>
public sealed record NdpFile(
    string? SourceName,
    uint Count,
    IReadOnlyList<NdpRecordRaw> Records);
