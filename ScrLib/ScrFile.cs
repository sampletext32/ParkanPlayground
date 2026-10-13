namespace ScrLib;

/// <summary>SCR файл: список секций скрипта ИИ.</summary>
/// <param name="Magic">Число встроенных функций, известных скрипту (в <c>ai.dll</c> <c>native_actions_count</c>); в первой части игры всегда 59 (0x3B), в IS2 - 73.</param>
/// <param name="EntryCount">Количество секций.</param>
/// <param name="Entries">Секции SCR.</param>
public record ScrFile(int Magic, int EntryCount, List<ScrEntry> Entries);
