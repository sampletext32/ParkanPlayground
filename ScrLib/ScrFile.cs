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

/// <summary>
/// Вложенная запись SCR: одна инструкция секции. Исполняется в <c>AIActions::execute_scr_ai_script</c> (ai.dll 0x024705b0);
/// секция выполняется заново сверху при каждом вызове до конца, до инструкции «Return» или до перехода.
/// </summary>
/// <param name="NativeActionIndex">Для <see cref="ScrEntryInnerType.Execute"/>: номер встроенной функции (0..58, <c>action_N</c> в ai.dll), -1 - не встроенная
/// (копирование переменной или функция из .fml). В остальных типах не используется.</param>
/// <param name="ResultVarIndex">Индекс переменной Varset: для Execute и SetVarsetValue - в неё пишется результат / значение. Для Compare не используется (-1).</param>
/// <param name="ValueOrSourceVarIndex">SetVarsetValue: записываемое значение (для переменной типа DWORD - число, иначе как float). Execute при NativeActionIndex = -1:
/// индекс переменной-источника копирования, -1 - вызвать функцию .fml.</param>
/// <param name="Type">Тип инструкции.</param>
/// <param name="CompareOperator">Compare: операция сравнения первой переменной аргументов со второй: 0 <c>&lt;</c>, 1 <c>==</c>, 2 <c>&gt;</c>, 3 <c>&lt;=</c>, 4 <c>&gt;=</c>, 5 <c>!=</c>.
/// Остальные типы: не используется.</param>
/// <param name="ArgumentsCount">Количество аргументов.</param>
/// <param name="Arguments">Аргументы: Compare - индексы двух переменных Varset; GotoInstruction - номер инструкции (с 1); GotoEntry - номер секции.</param>
/// <param name="FmlFunctionIndex">Execute при NativeActionIndex = -1 и ValueOrSourceVarIndex = -1: номер функции в .fml (<c>FunctionEntry::execute_fml_function</c>).</param>
public record ScrEntryInner(
    int NativeActionIndex,
    int ResultVarIndex,
    int ValueOrSourceVarIndex,
    ScrEntryInnerType Type,
    int CompareOperator,
    int ArgumentsCount,
    List<int> Arguments,
    int FmlFunctionIndex);

/// <summary>
/// Тип инструкции SCR (<c>AIScript.type</c> в ai.dll). Условия образуют стек: каждое Compare увеличивает глубину,
/// результат на глубине i = (сравнение) И (результат на глубине i-1); EndIf уменьшает глубину. Инструкции Execute,
/// GotoInstruction, GotoEntry и Return выполняются, только если глубина 0 или результат верхнего условия истинен.
/// Проверено по данным (75 .scr): у Compare всегда 2 аргумента и операция 0..5, число Compare (680) близко к числу EndIf (682),
/// номера переходов лежат в пределах секции/файла.
/// </summary>
public enum ScrEntryInnerType
{
    /// <summary>Выполнить действие: встроенная функция <c>action_{NativeActionIndex}</c>, копирование переменной или функция .fml; результат - в переменную ResultVarIndex.</summary>
    Execute = -1,

    /// <summary>Сравнить две переменные Varset (аргументы), операция - CompareOperator; DWORD сравнивается как целое, остальное как float.</summary>
    Compare = 0,

    /// <summary>Закрыть условие (уменьшить глубину стека условий).</summary>
    EndIf = 1,

    /// <summary>Ничего не делает (в данных 9 раз).</summary>
    Nop = 2,

    /// <summary>Перейти на инструкцию с номером аргумента 0 (от 1) в этой секции.</summary>
    GotoInstruction = 3,

    /// <summary>Перейти к началу секции с номером аргумента 0.</summary>
    GotoEntry = 4,

    /// <summary>Завершить выполнение секции на этом тике (возвращает false). Выполнение при следующем вызове снова начинается с первой инструкции.</summary>
    Return = 5,

    /// <summary>Записать в переменную Varset ResultVarIndex значение ValueOrSourceVarIndex.</summary>
    SetVarsetValue = 6,
}
