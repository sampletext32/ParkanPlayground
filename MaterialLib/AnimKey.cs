namespace MaterialLib;

/// <summary>Ключ анимации материала (6 байт в файле).</summary>
/// <param name="StageIndex">Номер стадии материала для этого ключа.</param>
/// <param name="DurationMs">Конец интервала ключа в миллисекундах от начала анимации (накопленное время, а не длительность одного ключа).
/// Длительность всей анимации - значение последнего ключа. В данных типичный шаг 50 мс, у <c>DEFL_G</c> - 5 мс.</param>
/// <param name="InterpolationCurve">Кривая интерполяции. Движок это поле не использует; во всех 1848 ключах равно 0.</param>
public readonly record struct AnimKey(ushort StageIndex, ushort DurationMs, ushort InterpolationCurve);
