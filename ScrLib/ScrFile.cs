namespace ScrLib;

/// <summary>SCR файл.</summary>
/// <param name="Magic">Число известных игре скриптов; обычно 59 (0x3B).</param>
/// <param name="EntryCount">Количество ScrEntry.</param>
/// <param name="Entries">Записи SCR.</param>
public record ScrFile(int Magic, int EntryCount, List<ScrEntry> Entries);

/// <summary>Запись SCR.</summary>
/// <param name="Title">Название записи.</param>
/// <param name="Index">Индекс записи.</param>
/// <param name="InnerCount">Количество вложенных записей.</param>
/// <param name="Inners">Вложенные записи.</param>
public record ScrEntry(string Title, int Index, int InnerCount, List<ScrEntryInner> Inners);

/// <summary>Вложенная запись SCR.</summary>
/// <param name="ScriptIndex">Номер скрипта в игре из таблицы известных скриптов (обычно 0x3B записей).</param>
/// <param name="UnkInner2">Неизвестное поле. Для SetVarsetValue это индекс в Varset.</param>
/// <param name="UnkInner3">Неизвестное поле. Для SetVarsetValue это устанавливаемое значение.</param>
/// <param name="Type">Тип вложенной записи.</param>
/// <param name="UnkInner5">Неизвестное поле.</param>
/// <param name="ArgumentsCount">Количество аргументов.</param>
/// <param name="Arguments">Аргументы вложенной записи.</param>
/// <param name="UnkInner7">Неизвестное поле.</param>
public record ScrEntryInner(
    int ScriptIndex,
    int UnkInner2,
    int UnkInner3,
    ScrEntryInnerType Type,
    int UnkInner5,
    int ArgumentsCount,
    List<int> Arguments,
    int UnkInner7);

public enum ScrEntryInnerType
{
    Unspecified = -1,
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
    _4 = 4,
    CheckInternalState = 5,
    /// <summary>
    /// В случае 6, игра берёт UnkInner2 (индекс в Varset) и устанавливает ему значение UnkInner3
    /// </summary>
    SetVarsetValue = 6,
}
