namespace FxidLib;

/// <summary>
/// Компонент типа 9 (<see cref="FxidComponentType.AnimModel"/>): префикс типа 3 (общие конструктор, <c>SetParams</c> и <c>EvaluateAtTime</c>;
/// различаются только vtable и <c>Render</c> по адресу 100149e0) плюс два dword.
/// <c>Render</c> задаёт флаги отрисовки <c>EffectContents</c> (поле +8; <c>EffectContents.render_group</c> на деле - слово флагов, у типов 3 и 4 оно равно 1)
/// и вызывает отрисовку содержимого типа 3: базовое значение по <see cref="DrawMode_maybe"/> (0 → 3, 1 → 5, 2 → 6), уровень качества контекста
/// (0..3) идёт в два старших бита (<c>level &lt;&lt; 30</c>), а <see cref="Flag_TBD"/> == 0 дополнительно ставит 0x8000000.
/// </summary>
/// <param name="Base">Поля, общие с типом 3 (первые 0xC8 байт; у типа 9 <c>Space</c> всегда 2).</param>
/// <param name="DrawMode_maybe">+0xC8: режим отрисовки (0, 1; значение 2 движок знает, но в данных не встречается).</param>
/// <param name="Flag_TBD">+0xCC: флаг, при 0 добавляющий бит 0x8000000; смысл не выяснен.
/// В данных пары (режим, флаг): (0, 0) - 129 компонентов, (1, 0) - 66, (1, 1) - 6.</param>
public sealed record EffectType9Data(EffectType3Data Base, uint DrawMode_maybe, uint Flag_TBD);
