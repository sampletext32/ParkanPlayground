namespace ScrLib;

/// <summary>
/// Тип инструкции SCR (<c>ScrInstruction.type</c>, enum <c>ScrInstructionType</c> в ai.dll). Условия образуют стек: каждое Compare увеличивает глубину,
/// результат на глубине i = (сравнение) И (результат на глубине i-1); EndIf уменьшает глубину. Инструкции Execute,
/// GotoInstruction, GotoEntry и Return выполняются, только если глубина 0 или результат верхнего условия истинен.
/// Проверено по данным (75 .scr): у Compare всегда 2 аргумента и операция 0..5, число Compare (680) близко к числу EndIf (682),
/// номера переходов лежат в пределах секции/файла.
/// </summary>
public enum ScrInstructionType
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
