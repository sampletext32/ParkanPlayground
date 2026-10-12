namespace FxidLib;

/// <summary>
/// Постобработка силы эффекта в точности как в <c>FxManager::ComputeEffectStrength</c> (флаги 0x200 и 0x20)
/// плюс «ворота» окружения (флаги 0x80 и 0x100). Здесь смоделированы только источники по времени;
/// остальным источникам нужно живое состояние игры.
/// </summary>
public static class FxidStrength
{
    /// <summary>Жизнь = (сейчас − начало) / (конец − начало). При конец == начало даёт NaN/бесконечность, как в движке.</summary>
    /// <param name="nowMs">Текущее время, мс.</param>
    /// <param name="startMs">Время старта эффекта, мс.</param>
    /// <param name="endMs">Время окончания эффекта, мс (<c>Duration * 1000 + start</c>).</param>
    public static float Life(float nowMs, float startMs, float endMs) => (nowMs - startMs) / (endMs - startMs);

    /// <summary>Значение для источников по времени (<c>Life_0to1</c>, <c>Life_Fract</c>, <c>Life_1minus</c>); <c>null</c> для источников, зависящих от состояния игры.</summary>
    /// <param name="source">Источник силы.</param>
    /// <param name="nowMs">Текущее время, мс.</param>
    /// <param name="startMs">Время старта эффекта, мс.</param>
    /// <param name="endMs">Время окончания эффекта, мс.</param>
    public static float? FromTimeSource(EffectStrengthSource source, float nowMs, float startMs, float endMs)
    {
        var life = Life(nowMs, startMs, endMs);
        return source switch
        {
            EffectStrengthSource.Life_0to1 => life,
            EffectStrengthSource.Life_Fract => life - MathF.Floor(life),
            EffectStrengthSource.Life_1minus => 1f - life,
            _ => null,
        };
    }

    /// <summary>Истина, если флаги 0x80/0x100 обнуляют силу при данном бите 0 окружения.</summary>
    /// <param name="flags">Флаги заголовка.</param>
    /// <param name="environmentBit0">Бит 0 <c>FxManager.environment_flags</c>.</param>
    public static bool IsGatedOff(FxidEffectFlags flags, bool environmentBit0) =>
        (flags.HasFlag(FxidEffectFlags.OnlyWhenEnvBit0On) && !environmentBit0) ||
        (flags.HasFlag(FxidEffectFlags.OnlyWhenEnvBit0Off) && environmentBit0);

    /// <summary>Применяет <c>MultiplyByLife</c> (0x200) и <c>TriangularShape</c> (0x20) в порядке движка.</summary>
    /// <param name="flags">Флаги заголовка.</param>
    /// <param name="strength">Сила до постобработки.</param>
    /// <param name="life">Прогресс жизни 0..1.</param>
    public static float ApplyFlags(FxidEffectFlags flags, float strength, float life)
    {
        if (flags.HasFlag(FxidEffectFlags.MultiplyByLife)) strength *= life;
        if (flags.HasFlag(FxidEffectFlags.TriangularShape))
        {
            if (strength >= 0.5f) strength = 1f - strength;
            strength += strength;
        }
        return strength;
    }
}
