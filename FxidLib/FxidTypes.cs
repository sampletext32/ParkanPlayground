using Common;

#pragma warning disable CS0618 // legacy records reference each other

namespace FxidLib;

/// <summary>
/// Effect-level header at the start of each FXID file (60 bytes total).
/// Parsed from CEffect_InitFromDef: defines component count, global duration/flags,
/// some unknown control fields, and the uniform scale vector applied to the effect.
/// </summary>
public record struct EffectHeader
{
    /// <summary>FXID payload offset 0x00: command count.</summary>
    public uint ComponentCount;
    /// <summary>
    /// FXID payload offset 0x04: source of the effect "strength" value (Ghidra: EEffectStrengthSource,
    /// copied to EffectMainParams.m_StrengthSource). Formerly misnamed "TimeMode".
    /// </summary>
    public EffectStrengthSource StrengthSource;
    /// <summary>
    /// FXID payload offset 0x08: effect duration in seconds. The engine computes
    /// end_time_ms = Duration * 1000 + start_time_ms. 0.0 in 120 corpus files, always with a non-life StrengthSource (0,4,5,14,15,16,17), because life-based sources would divide 0/0.
    /// </summary>
    public float Duration;
    /// <summary>FXID payload offset 0x0C: amplitude of the random strength offset; used only with <see cref="FxidEffectFlags.RandomizeStrength"/> (corpus: 0.1/0.2/0.4).</summary>
    public float StrengthJitter;
    /// <summary>FXID payload offset 0x10: effect behavior flags (see <see cref="FxidEffectFlags"/> for evidence levels).</summary>
    public FxidEffectFlags Flags;
    /// <summary>
    /// FXID payload offset 0x14: quality-settings id (read in Effect.dll by <c>EffectDllSettings::GetDust</c> and <c>NextRandLODValue</c>).
    /// Low byte (<see cref="SettingsCategory"/>, corpus 0..19): index into a table of 20 enable flags; an entry of 0 means the effect is not created at the current detail preset.
    /// The flags are filled by <c>SetDefaultValues(preset)</c>: presets 1 and 2 enable everything, preset 3 (lowest detail) disables indices 0,1,6,7,10,11,12,16.
    /// By the order of the resource strings the indices look like: 0 Dust, 1 Smoke, 2 EngineFire, 3 Explode, 4 Shield, 5 Lights, 6 Dust2, 7 Smoke2, 8 Explode2, 9 Shield2,
    /// 10 Lights2, 11 Smoke3, 12 EngineFire2, 13 Explode3, 14 GunFire1, 15 Lights3, 16 Smoke4, 17 Explode4, 18 GunFire2, 19 Lights4 (names are a hypothesis from the string order).
    /// High byte (<see cref="SettingsLodClass"/>, 0..3): class used by particle emitters (types 7, 8, 10). Each particle gets a random threshold
    /// <c>rand16/65536 * D[class] - H[class]</c> (D = 3,3,3.5,5.5 for preset 1/3 and 1.5,1.5,1.75,2.75 for preset 2; H = 0,0,-2,-3) and is drawn only if the
    /// render quality level (0..3, render context dword 0x18) is not above the threshold: classes 0/1 thin out as the level grows, classes 2/3 keep their particles.
    /// </summary>
    public uint SettingsId;

    /// <summary>Low byte of <see cref="SettingsId"/>: effect category index (0..19).</summary>
    public byte SettingsCategory => (byte)(SettingsId & 0xFF);

    /// <summary>Second byte of <see cref="SettingsId"/>: particle LOD class (0..3).</summary>
    public byte SettingsLodClass => (byte)((SettingsId >> 8) & 0xFF);
    /// <summary>FXID payload offset 0x18: amplitude of the random position offset X (used with <see cref="FxidEffectFlags.RandomOffset"/>).</summary>
    public float RandShiftX;
    /// <summary>FXID payload offset 0x1C: random spatial shift Y.</summary>
    public float RandShiftY;
    /// <summary>FXID payload offset 0x20: random spatial shift Z.</summary>
    public float RandShiftZ;
    /// <summary>FXID payload offset 0x24: local pivot X.</summary>
    public float PivotX;
    /// <summary>FXID payload offset 0x28: local pivot Y.</summary>
    public float PivotY;
    /// <summary>FXID payload offset 0x2C: local pivot Z.</summary>
    public float PivotZ;
    /// <summary>FXID payload offset 0x30: base scale X.</summary>
    public float ScaleX;
    /// <summary>FXID payload offset 0x34: base scale Y.</summary>
    public float ScaleY;
    /// <summary>FXID payload offset 0x38: base scale Z.</summary>
    public float ScaleZ;

}

/// <summary>
/// Shared on-disk definition layout for billboard-style components (type 1).
/// Used by CBillboardComponent_Initialize/Update/Render to drive size/color/alpha
/// curves and sample scattering within a 3D extent volume.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct BillboardComponentData
{
    public uint TypeAndFlags;       // type (low byte) and flags as seen in CEffect_InitFromDef
    public float Unknown04;         // mode / flag-like float, semantics not fully clear
    public float ScalarAMin;        // base scalar A (e.g. base radius)
    public float ScalarAMax;        // max scalar A
    public float ScalarAExp;        // exponent applied to scalar A curve
    public float ActiveTimeStart;   // activation window start (seconds)
    public float ActiveTimeEnd;     // activation window end (seconds)
    public float SampleSpreadParam; // extra param used when scattering samples
    public uint Unknown20;          // used as integer param in billboard code, exact meaning unknown
    public uint PrimarySampleCount; // number of samples along primary axis
    public uint SecondarySampleCount; // number of samples along secondary axis
    public Vector3 ExtentVec0;      // base extent/origin vector
    public Vector3 ExtentVec1;      // extent center / offset
    public Vector3 ExtentVec2;      // extent size used for random boxing
    public Vector3 ExponentTriplet0;// exponent triplet for size/color curve
    public Vector3 RadiusTriplet0;  // radius curve key 0
    public Vector3 RadiusTriplet1;  // radius curve key 1
    public Vector3 RadiusTriplet2;  // radius curve key 2
    public Vector3 ExponentTriplet1;// second exponent triplet (e.g. alpha curve)
    public float NoiseAmplitude;    // per-sample noise amplitude
    public byte[] Reserved;         // 0x50-byte tail, currently not touched by billboard code
}

/// <summary>
/// 3D sound component definition (type 2).
/// Used by CSoundComponent_Initialize/Update to drive positional audio, playback
/// window, and scalar ranges (e.g. volume / pitch), plus a 0x40-byte sound name tail.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct SoundComponentData
{
    public uint TypeAndFlags;           // component type and flags
    public uint PlayMode;               // playback mode (looping, one-shot, etc.)
    public float StartTime;             // playback window start (seconds)
    public float EndTime;               // playback window end (seconds)
    public Vector3 Pos0;                // base 3D position or path start
    public Vector3 Pos1;                // secondary position / path end
    public Vector3 Offset0;             // random offset range 0
    public Vector3 Offset1;             // random offset range 1
    public float Scalar0Min;            // scalar range 0 min (e.g. volume)
    public float Scalar0Max;            // scalar range 0 max
    public float Scalar1Min;            // scalar range 1 min (e.g. pitch)
    public float Scalar1Max;            // scalar range 1 max
    public uint SoundFlags;             // misc sound control flags
    public byte[] SoundNameAndReserved; // 0x40-byte tail; sound name plus padding/unused
}

/// <summary>
/// Animated particle component definition (type 3).
/// Prefix layout matches BillboardComponentData and is used to allocate a grid of
/// particle objects; the 0x38-byte tail is passed into CFxManager_LoadTexture.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct AnimParticleComponentData
{
    public uint TypeAndFlags;       // type (low byte) and flags as seen in CEffect_InitFromDef
    public float Unknown04;         // mode / flag-like float, semantics not fully clear
    public float ScalarAMin;        // base scalar A (e.g. base radius)
    public float ScalarAMax;        // max scalar A
    public float ScalarAExp;        // exponent applied to scalar A curve
    public float ActiveTimeStart;   // activation window start (seconds)
    public float ActiveTimeEnd;     // activation window end (seconds)
    public float SampleSpreadParam; // extra param used when scattering particles
    public uint Unknown20;          // used as integer param in anim particle code, exact meaning unknown
    public uint PrimarySampleCount; // number of particles along primary axis
    public uint SecondarySampleCount; // number of particles along secondary axis
    public Vector3 ExtentVec0;      // base extent/origin vector
    public Vector3 ExtentVec1;      // extent center / offset
    public Vector3 ExtentVec2;      // extent size used for random boxing
    public Vector3 ExponentTriplet0;// exponent triplet for size/color curve
    public Vector3 RadiusTriplet0;  // radius curve key 0
    public Vector3 RadiusTriplet1;  // radius curve key 1
    public Vector3 RadiusTriplet2;  // radius curve key 2
    public Vector3 ExponentTriplet1;// second exponent triplet (e.g. alpha curve)
    public float NoiseAmplitude;    // per-particle noise amplitude
    public byte[] Reserved;         // 0x38-byte tail; forwarded to CFxManager_LoadTexture unchanged
}

/// <summary>
/// Animated billboard component definition (type 4).
/// Shares the same prefix layout as BillboardComponentData, including extents and
/// radius/exponent triplets, but uses a 0x3C-byte tail passed to CFxManager_LoadTexture.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct AnimBillboardComponentData
{
    public uint TypeAndFlags;       // type (low byte) and flags as seen in CEffect_InitFromDef
    public float Unknown04;         // mode / flag-like float, semantics not fully clear
    public float ScalarAMin;        // base scalar A (e.g. base radius)
    public float ScalarAMax;        // max scalar A
    public float ScalarAExp;        // exponent applied to scalar A curve
    public float ActiveTimeStart;   // activation window start (seconds)
    public float ActiveTimeEnd;     // activation window end (seconds)
    public float SampleSpreadParam; // extra param used when scattering animated billboards
    public uint Unknown20;          // used as integer param in anim billboard code, exact meaning unknown
    public uint PrimarySampleCount; // number of samples along primary axis
    public uint SecondarySampleCount; // number of samples along secondary axis
    public Vector3 ExtentVec0;      // base extent/origin vector
    public Vector3 ExtentVec1;      // extent center / offset
    public Vector3 ExtentVec2;      // extent size used for random boxing
    public Vector3 ExponentTriplet0;// exponent triplet for size/color curve
    public Vector3 RadiusTriplet0;  // radius curve key 0
    public Vector3 RadiusTriplet1;  // radius curve key 1
    public Vector3 RadiusTriplet2;  // radius curve key 2
    public Vector3 ExponentTriplet1;// second exponent triplet (e.g. alpha curve)
    public float NoiseAmplitude;    // per-sample noise amplitude
    public byte[] Reserved;         // 0x3C-byte tail; forwarded to CFxManager_LoadTexture unchanged
}

/// <summary>
/// Compact definition for trail / ribbon components (type 5).
/// CTrailComponent_Initialize interprets this as segment count, width/alpha/UV
/// ranges, timing, and a shared texture name at +0x30.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct TrailComponentData
{
    public uint TypeAndFlags;             // component type and flags
    public byte[] Unknown04To10;          // 0x10 bytes at +4..+0x13, used only indirectly; types unknown
    public uint SegmentCount;             // number of trail segments (particles)
    public float Param0;                  // first width/alpha/UV control value (start)
    public float Param1;                  // second width/alpha/UV control value (end)
    public uint Unknown20;                // extra integer parameter, purpose unknown
    public uint Unknown24;                // extra integer parameter, purpose unknown
    public float ActiveTimeStart;         // trail activation start time (>= 0)
    public float ActiveTimeEnd;           // trail activation end time
    public byte[] TextureNameAndReserved; // 0x40-byte tail containing texture name and padding/flags
}

/// <summary>
/// Simple point component definition (type 6).
/// Definition block is just the 4-byte typeAndFlags header; no extra data on disk.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct PointComponentData
{
    public uint TypeAndFlags; // component type and flags; definition block has no payload
}

/// <summary>
/// Plane component definition (type 7).
/// Shares the same 0xC8-byte prefix layout as AnimParticleComponentData (type 3),
/// followed by two dwords of plane-specific data.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct PlaneComponentData
{
    public AnimParticleComponentData Base; // shared 0xC8-byte prefix: time window, sample counts, extents, curves
    public uint ExtraPlaneParam0;          // plane-specific parameter, semantics not yet reversed
    public uint ExtraPlaneParam1;          // plane-specific parameter, semantics not yet reversed
}

/// <summary>
/// Static model component definition (type 8).
/// Layout fully matches the IDA typedef used by CModelComponent_Initialize:
/// time window, instance count, spatial extents/axes, radius triplets, and a
/// 0x40-byte texture name tail.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct ModelComponentData
{
    public uint TypeAndFlags;          // component type and flags
    public byte[] Unk04;               // 0x14-byte blob at +0x04..+0x17, purpose unclear
    public float ActiveTimeStart;      // activation window start (seconds), +0x18
    public float ActiveTimeEnd;        // activation window end (seconds), +0x1C
    public uint Unknown20;             // extra flags/int parameter at +0x20
    public uint InstanceCount;         // number of model instances to spawn at +0x24
    public Vector3 BasePos;            // base position of the emitter / origin, +0x28
    public Vector3 OffsetPos;          // positional offset applied per-instance, +0x34
    public Vector3 ScatterExtent;      // extent volume used for random scattering, +0x40
    public Vector3 Axis0;              // local axis 0 (orientation / shape), +0x4C
    public Vector3 Axis1;              // local axis 1 (orientation / shape), +0x58
    public Vector3 Axis2;              // local axis 2 (orientation / shape), +0x64
    public byte[] Reserved70;          // 0x18 bytes at +0x70..+0x87, not directly used
    public Vector3 RadiusTriplet0;     // radius / extent triplet 0 at +0x88
    public Vector3 RadiusTriplet1;     // radius / extent triplet 1 at +0x94
    public byte[] ReservedA0;          // 0x18 bytes at +0xA0..+0xB7, not directly used
    public byte[] TextureNameAndFlags; // 0x40-byte tail at +0xB8: texture name + padding/flags
}

/// <summary>
/// Animated model component definition (type 9).
/// Layout derived from CAnimModelComponent_Initialize: time params, direction vectors,
/// radius triplets, extent vectors, and a 0x48-byte texture name tail.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct AnimModelComponentData
{
    public uint TypeAndFlags;          // component type and flags
    public float AnimSpeed;            // animation speed multiplier at +0x04
    public float MinTime;              // activation window start (clamped >= 0) at +0x08
    public float MaxTime;              // activation window end at +0x0C
    public float Exponent;             // exponent for time interpolation at +0x10
    public byte[] Reserved14;          // 0x14 bytes at +0x14..+0x27, padding
    public Vector3 DirVec0;            // normalized direction vector 0 at +0x28
    public byte[] Reserved34;          // 0x0C bytes at +0x34..+0x3F, padding
    public Vector3 RadiusTriplet0;     // radius triplet 0 at +0x40
    public Vector3 DirVec1;            // normalized direction vector 1 at +0x4C
    public Vector3 RadiusTriplet1;     // radius triplet 1 at +0x58
    public Vector3 ExtentVec0;         // extent vector 0 at +0x64
    public Vector3 ExtentVec1;         // extent vector 1 at +0x70
    public byte[] Reserved7C;          // 0x0C bytes at +0x7C..+0x87, padding
    public byte[] TextureNameAndFlags; // 0x48-byte tail at +0x88: texture/model name + padding
}

/// <summary>
/// Cube component definition (type 10).
/// Shares the same 0xCC-byte prefix layout as AnimBillboardComponentData (type 4),
/// followed by one dword of cube-specific data.
/// </summary>
[Obsolete("Legacy layout: prefix is shifted and the tail overlaps the resource reference (verified against the corpus). Use FxidFile.Read / FxidComponent.Raw / FxidComponentLayout; typed views: EffectType3Data.")]
public record struct CubeComponentData
{
    public AnimBillboardComponentData Base; // shared 0xCC-byte prefix: billboard-style time window, extents, curves
    public uint ExtraCubeParam0;            // cube-specific parameter, semantics not yet reversed
}
