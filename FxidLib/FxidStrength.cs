namespace FxidLib;

/// <summary>
/// Post-processing of the effect strength exactly as <c>FxManager::ComputeEffectStrength</c> does it
/// (flags 0x200 and 0x20), plus the environment gate (flags 0x80 / 0x100).
/// Only the time-based sources are modelled here; other sources need live game state.
/// </summary>
public static class FxidStrength
{
    /// <summary>Life = (now - start) / (end - start). With end == start this is NaN/Infinity, like in the engine.</summary>
    public static float Life(float nowMs, float startMs, float endMs) => (nowMs - startMs) / (endMs - startMs);

    /// <summary>Value for time-based sources (Life_0to1, Life_Fract, Life_1minus); null for sources that need game state.</summary>
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

    /// <summary>True when flags 0x80/0x100 force the strength to 0 for the given environment bit 0.</summary>
    public static bool IsGatedOff(FxidEffectFlags flags, bool environmentBit0) =>
        (flags.HasFlag(FxidEffectFlags.OnlyWhenEnvBit0On) && !environmentBit0) ||
        (flags.HasFlag(FxidEffectFlags.OnlyWhenEnvBit0Off) && environmentBit0);

    /// <summary>Applies MultiplyByLife (0x200) and TriangularShape (0x20), in the engine's order.</summary>
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
