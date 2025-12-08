using Common;

namespace ParkanPlayground.Effects;

/// <summary>
/// Effect-level header at the start of each FXID file (60 bytes total).
/// Parsed from CEffect_InitFromDef: defines component count, global duration/flags,
/// some unknown control fields, and the uniform scale vector applied to the effect.
/// </summary>
public struct EffectHeader
{
    public uint ComponentCount;
    public uint Unknown1;
    public float Duration;
    public float Unknown2;
    public uint Flags;
    public uint Unknown3;
    public byte[] Reserved; // 24 bytes
    public float ScaleX;
    public float ScaleY;
    public float ScaleZ;
}

/// <summary>
/// Shared on-disk definition layout for billboard-style components (type 1).
/// Used by CBillboardComponent_Initialize/Update/Render to drive size/color/alpha
/// curves and sample scattering within a 3D extent volume.
/// </summary>
public struct BillboardComponentData
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
public struct SoundComponentData
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
public struct AnimParticleComponentData
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
public struct AnimBillboardComponentData
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
public struct TrailComponentData
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
public struct PointComponentData
{
    public uint TypeAndFlags; // component type and flags; definition block has no payload
}

/// <summary>
/// Plane component definition (type 7).
/// Shares the same 0xC8-byte prefix layout as AnimParticleComponentData (type 3),
/// followed by two dwords of plane-specific data.
/// </summary>
public struct PlaneComponentData
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
public struct ModelComponentData
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
public struct AnimModelComponentData
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
public struct CubeComponentData
{
    public AnimBillboardComponentData Base; // shared 0xCC-byte prefix: billboard-style time window, extents, curves
    public uint ExtraCubeParam0;            // cube-specific parameter, semantics not yet reversed
}
