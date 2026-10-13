namespace ScrLib;

/// <summary>
/// Инструкция секции SCR (<c>ScrInstruction</c>, раньше <c>AIScript</c>, в <c>ai.dll</c>). Исполняется в <c>AIActions::ExecuteInstruction</c> (ai.dll 0x024705b0);
/// секция выполняется заново сверху при каждом вызове до конца, до инструкции «Return» или до перехода.
/// </summary>
/// <param name="NativeActionIndex">Для <see cref="ScrInstructionType.Execute"/>: номер встроенной функции (0..58, <c>action_N</c> в ai.dll), -1 - не встроенная
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
public record ScrInstruction(
    int NativeActionIndex,
    int ResultVarIndex,
    int ValueOrSourceVarIndex,
    ScrInstructionType Type,
    int CompareOperator,
    int ArgumentsCount,
    List<int> Arguments,
    int FmlFunctionIndex);
