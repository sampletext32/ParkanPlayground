namespace ScrLib;

/// <summary>Секция SCR (<c>ScrEntry</c> в <c>ai.dll</c>): именованный список инструкций - обработчик события или проблемы (<c>Init</c>, <c>Problems0</c>, <c>PBM_*_Start</c>...).</summary>
/// <param name="Title">Название секции; по нему игра находит секцию (<c>AIActions::FindEntryIndex</c>).</param>
/// <param name="Index">Индекс секции, записанный в файле.</param>
/// <param name="InstructionCount">Количество инструкций.</param>
/// <param name="Instructions">Инструкции секции в порядке выполнения.</param>
public record ScrEntry(string Title, int Index, int InstructionCount, List<ScrInstruction> Instructions);
