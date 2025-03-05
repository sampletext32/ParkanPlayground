namespace ScrLib;

public class ScrFile
{
    /// <summary>
    /// тут всегда число 59 (0x3b) - это число известных игре скриптов
    /// </summary>
    public int Magic { get; set; }

    public int EntryCount { get; set; }

    public List<ScrEntry> Entries { get; set; }
}

public class ScrEntry
{
    public string Title { get; set; }

    public int Index { get; set; }

    public int InnerCount { get; set; }

    public List<ScrEntryInner> Inners { get; set; }
}

public class ScrEntryInner
{
    /// <summary>
    /// Номер скрипта в игре (это тех, которых 0x3b)
    /// </summary>
    public int ScriptIndex { get; set; }

    public int UnkInner2 { get; set; }
    public int UnkInner3 { get; set; }

    public ScrEntryInnerType Type { get; set; }

    public int UnkInner5 { get; set; }

    public int ArgumentsCount { get; set; }

    public List<int> Arguments { get; set; }

    public int UnkInner7 { get; set; }
}

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