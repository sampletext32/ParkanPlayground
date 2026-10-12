namespace FxidLib;

/// <summary>
/// Компонент типа 4 (<see cref="FxidComponentType.AnimBillboard"/>): раскладка типа 3 плюс float по смещению +0xC8.
/// Проверено (<c>SetParamsWithReferenceDistance</c> 10011650, <c>EffectType4::RenderContents</c>): движок хранит <c>1 / value</c>, при отрисовке прогресс
/// анимации равен <c>clamp(distance_to_camera * (1 / value) * life, 0..1)</c>, где <c>distance_to_camera</c> - длина опорного вектора
/// (позиция камеры в локальной системе эффекта). Значение - расстояние, на котором анимация завершена.
/// </summary>
/// <param name="Base">Поля, общие с типом 3 (первые 0xC8 байт; у типа 4 <c>Space</c> всегда 0, <c>PhaseStart</c> всегда −1, <c>PhaseExponent</c> всегда 1).</param>
/// <param name="ReferenceDistance">+0xC8: опорное расстояние до камеры. В данных: 700 - 75 компонентов, 500 - 68, 300 - 30, 150 - 4 (всего 177).</param>
public sealed record EffectType4Data(EffectType3Data Base, float ReferenceDistance);
