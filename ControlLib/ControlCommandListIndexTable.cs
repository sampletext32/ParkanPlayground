namespace ControlLib;

/// <summary>
/// Таблица хуков CTL: 21 слот, в каждом индекс списка команд из <see cref="CtlFile.CommandLists"/> или −1 (списка нет).
/// Слот определяет момент, когда игра выполняет список.
/// </summary>
/// <remarks>
/// Слоты и число файлов (из 449) с непустым слотом: 0 <c>on_load</c> (при инициализации/<c>GMSG_INIT</c>) - 200; 2, 3, 4 - состояния снаряда BULL - по 53;
/// 6 / 7 <c>on_enter_critical_decay</c> / <c>on_exit_critical_decay</c> - по 25; 8 момент ближайшего прохода цели у самонаведения (предмет 0x16) - 1;
/// 10..20 юнит оказался на поверхности класса 0..10 (<c>BTLU_frame_update</c>, хук <c>10 + класс</c>) - по 9; слоты 1, 5, 9 нигде не вызываются и пусты во всех файлах.
/// </remarks>
/// <param name="Offset">Смещение таблицы от начала файла (для отладки).</param>
/// <param name="Indices">21 индекс списка команд (или −1).</param>
public sealed record ControlCommandListIndexTable(long Offset, IReadOnlyList<int> Indices)
{
    /// <summary>Слот 0: список при загрузке объекта.</summary>
    public int OnLoad => Indices.Count > 0 ? Indices[0] : -1;

    /// <summary>Слот 6: список при входе в критическое разрушение.</summary>
    public int OnEnterCriticalDecay => Indices.Count > 6 ? Indices[6] : -1;

    /// <summary>Слот 7: список при выходе из критического разрушения.</summary>
    public int OnExitCriticalDecay => Indices.Count > 7 ? Indices[7] : -1;
}
