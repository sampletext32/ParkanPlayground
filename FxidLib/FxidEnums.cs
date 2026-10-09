namespace FxidLib;

/// <summary>
/// Source of the effect "strength" value. Stored in the FXID header at +0x04 and copied by
/// <c>EffectHandle::InitFromFxidData</c> into <c>EffectMainParams.m_StrengthSource</c>.
/// Names come from the Ghidra enum <c>EEffectStrengthSource</c>; behaviour below is taken from
/// <c>FxManager::ComputeEffectStrength</c> (Effect.dll). The corpus uses only 0,1,2,4,5,14,15,16,17;
/// the remaining values are defined by the engine but never occur in data.
/// Life = (current_time_ms - start_time_ms) / (end_time_ms - start_time_ms), so with Duration = 0
/// the life-based sources divide 0/0; such files presumably rely on non-life sources.
/// Previously mislabeled as "TimeMode".
/// </summary>
public enum EffectStrengthSource : uint
{
    /// <summary>EffectHandle field unk_0x28_TBD (set at runtime).</summary>
    Constant = 0,
    /// <summary>Life.</summary>
    Life_0to1 = 1,
    /// <summary>Fractional part of Life (sawtooth for loops).</summary>
    Life_Fract = 2,
    /// <summary>1 - Life.</summary>
    Life_1minus = 3,
    /// <summary>IAnimation.GetPieceAnimTransitionProgress for the attach piece.</summary>
    AnimParam = 4,
    /// <summary>IControl base property 0x21 (tangential speed) / limit from 0x11; Mag = vector length, X/Y/Z = component.</summary>
    TangSpeedMag = 5,
    TangSpeedX = 6,
    TangSpeedY = 7,
    TangSpeedZ = 8,
    /// <summary>IControl base property 0x24 (normal speed) / limit from 0x11; Mag = vector length, X/Y/Z = component.</summary>
    NormSpeedMag = 9,
    NormSpeedX = 10,
    NormSpeedY = 11,
    NormSpeedZ = 12,
    /// <summary>1 - ILifeSystem.QueryNdpLifeStateRatio.</summary>
    LifeSystemInvX = 13,
    /// <summary>1 - IControl base property 0x31.</summary>
    ControlInvX = 14,
    /// <summary>max(|tangential speed| / limit, |normal speed| / limit). Decompilation is garbled; confirm in assembly.</summary>
    Max_TangOrNorm = 15,
    /// <summary>AnimParam, but never below the previous strength unless the animation reached 0.</summary>
    AnimMinClamp = 16,
    /// <summary>AnimParam, but never above the previous strength unless the animation reached 1.</summary>
    AnimMaxClamp = 17,
}

/// <summary>
/// FXID header flags (+0x10). Evidence: Effect.dll functions FxManager::UpdateEffectHandle,
/// FxManager::ComputeEffectStrength, EffectHandle::EvaluateComponentsAndMergeBounds (ex "UpdateDust"),
/// EffectHandle::RenderEffectComponents, EffectHandle::InitFromFxidData.
/// Corpus usage (476 files): 0x1:52 0x2:106 0x4:108 0x8:51 0x10:130 0x20:56 0x40:7 0x100:110 0x400:113
/// 0x800:148 0x1000:7 0x2000:1 0x8000:5; 0x80 is never used.
/// </summary>
[Flags]
public enum FxidEffectFlags : uint
{
    None = 0,

    /// <summary>VERIFIED: strength override gets a random offset of +-<c>StrengthJitter</c>/2 and is clamped to 0..1.</summary>
    RandomizeStrength = 0x0001,
    /// <summary>VERIFIED: when strength reaches >= 1 the effect handle is deleted.</summary>
    AutoDeleteAtFull = 0x0002,
    /// <summary>VERIFIED: after the first update the effect detaches from its attach piece (ResetTransformToParent).</summary>
    DetachAfterEmit = 0x0004,
    /// <summary>VERIFIED: the world position gets a random offset per axis, amplitude = RandShift (+0x18..0x20).</summary>
    RandomOffset = 0x0008,
    /// <summary>VERIFIED: the effect is enabled regardless of the attach piece flags/state (see <see cref="EnabledInAnyPieceState"/>).</summary>
    AlwaysEnabled = 0x0010,
    /// <summary>VERIFIED (ComputeEffectStrength): triangular shape, v &gt;= 0.5 ? 1 - v : v, then v * 2. Applied after <see cref="MultiplyByLife"/>.</summary>
    TriangularShape = 0x0020,
    /// <summary>VERIFIED (InitFromFxidData): <c>isActive = !(Flags &amp; 0x40)</c>. Must be switched on with ActivateEffect.</summary>
    StartInactive = 0x0040,
    /// <summary>VERIFIED (ComputeEffectStrength): strength is 0 while FxManager.environment_flags bit 0 is CLEAR (bit 0 is set/cleared by two environment toggle messages; meaning TBD). The README had 0x80/0x100 reversed. Unused in the corpus.</summary>
    OnlyWhenEnvBit0On = 0x0080,
    /// <summary>VERIFIED (ComputeEffectStrength): strength is 0 while environment_flags bit 0 is SET.</summary>
    OnlyWhenEnvBit0Off = 0x0100,
    /// <summary>VERIFIED (ComputeEffectStrength): strength *= life progress, before <see cref="TriangularShape"/>.</summary>
    MultiplyByLife = 0x0200,

    /// <summary>
    /// Occlusion test (probable): InitFromFxidData sets stateFlags |= 0x100|0x10. RenderEffectComponents then casts a
    /// periodic ray (FindNearestIntersectedFaceInSystemArealMap) from the pivot; the components are drawn only when the
    /// ray is clear (stateFlags bit 3). Name stays _maybe until the ray endpoints are confirmed.
    /// </summary>
    HideWhenOccluded_maybe = 0x0400,

    /// <summary>
    /// VERIFIED (RenderEffectComponents): when set the effect is drawn only if the render call passes its extra flag
    /// (param_3 != 0); without it the manager sets environment_flags |= 2 ("has regular effects").
    /// What the extra render pass is: TBD.
    /// </summary>
    RenderOnlyInExtraPass_maybe = 0x0800,
    /// <summary>VERIFIED: sets environment_flags |= 0x10, which makes FRAME_UPDATE recompute the attach point and call NotifyAttachPointMoved on the effects.</summary>
    TracksAttachPoint_maybe = 0x1000,
    /// <summary>Only 1 file uses it. Passed (bit 13) as the last argument of the component render call. Meaning TBD.</summary>
    Unknown0x2000_TBD = 0x2000,
    /// <summary>VERIFIED: allows the effect when the attach piece state is non-zero (enabled = pieceFlagsOk &amp;&amp; (state == 0 || this) || AlwaysEnabled).</summary>
    EnabledInAnyPieceState = 0x8000,
}

/// <summary>Component type, low byte of <c>TypeAndFlags</c>. Names follow the existing FxidLib naming.</summary>
public enum FxidComponentType : byte
{
    /// <summary>Runtime class <c>EffectType1</c> (0xF0 bytes): a dynamic light (ILightManager).</summary>
    Billboard = 1,
    /// <summary>Runtime class <c>EffectType2</c> (0xA0 bytes): a 3D sound.</summary>
    Sound = 2,
    /// <summary>Runtime class <c>EffectType3</c> (0xFC bytes) with <c>EffectContents</c>.</summary>
    AnimParticle = 3,
    /// <summary>Runtime class <c>EffectType4</c> (0x104 bytes) with <c>EffectContents</c>.</summary>
    AnimBillboard = 4,
    /// <summary>Runtime class <c>EffectType5</c> (0x54 bytes): a streak drawn between the previous and current attach point.</summary>
    Trail = 5,
    /// <summary>Empty component: 4-byte payload, SetParams/EvaluateAtTime/Render are stubs (EvaluateAtTime returns false). Not present in the corpus.</summary>
    Point = 6,
    /// <summary>Particle emitter, 0xCC-byte particles, non-looping (EffectType7, 0x48 bytes).</summary>
    Plane = 7,
    /// <summary>Particle spawner along the attach point path (EffectType8, 0xAC bytes). The name is legacy.</summary>
    Model = 8,
    AnimModel = 9,
    /// <summary>Particle emitter, 0x1EC-byte particles, looping (EffectType10, 0x48 bytes).</summary>
    Cube = 10,
}
