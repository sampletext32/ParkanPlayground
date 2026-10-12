namespace ControlLib;

/// <summary>
/// Список команд CTL: счётчик и подряд идущие записи команд по 0x64 байта. Выполняется <c>CControlBase::ExecuteControlCommandList</c> (Control.dll 0x100028A0).
/// В данных 491 список на 449 файлов, 1271 команда.
/// </summary>
/// <param name="Offset">Смещение списка от начала файла (для отладки).</param>
/// <param name="Count">Число команд в списке.</param>
/// <param name="Records">Команды в порядке выполнения.</param>
public sealed record ControlCommandListRaw(
    long Offset,
    uint Count,
    IReadOnlyList<ControlCommandRecordRaw> Records);
