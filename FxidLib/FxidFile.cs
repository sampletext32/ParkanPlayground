using System.Buffers.Binary;
using System.Text;
using Common;

namespace FxidLib;

/// <summary>Thrown when an FXID payload does not conform to the layout verified on the original game files.</summary>
public sealed class FxidFormatException(string message) : Exception(message);

/// <summary>
/// Fixed sizes and resource-reference offsets of FXID components.
/// Sizes are taken from <c>EffectHandle::InitFromFxidData</c> (Effect.dll) and verified on all 476 files of the NRes archive effects.rlb.
/// Resource offsets were verified on the 1567 components of the same files: library name is empty or ends with ".lib", item name is printable;
/// the item name is empty for many type 1 (light, no texture) and type 10 components (empty = the engine picks a random material).
/// </summary>
public static class FxidComponentLayout
{
    /// <summary>Size of the on-disk component block, including the 4-byte TypeAndFlags.</summary>
    public static int GetSize(FxidComponentType type) => type switch
    {
        FxidComponentType.Billboard => 0xE0,
        FxidComponentType.Sound => 0x94,
        FxidComponentType.AnimParticle => 0xC8,
        FxidComponentType.AnimBillboard => 0xCC,
        FxidComponentType.Trail => 0x70,
        FxidComponentType.Point => 0x04,
        FxidComponentType.Plane => 0xD0,
        FxidComponentType.Model => 0xF8,
        FxidComponentType.AnimModel => 0xD0,
        FxidComponentType.Cube => 0xD0,
        _ => throw new FxidFormatException($"Unknown FXID component type {(byte)type}")
    };

    /// <summary>
    /// Offset of the 0x40-byte <see cref="ResourceNamedRef"/> inside the component, or -1 when the type has none.
    /// Note: types 4 and 9 have extra bytes after the reference (4 and 8 respectively).
    /// </summary>
    public static int GetResourceRefOffset(FxidComponentType type) => type switch
    {
        FxidComponentType.Billboard => 0xA0,
        FxidComponentType.Sound => 0x54,
        FxidComponentType.AnimParticle => 0x88,
        FxidComponentType.AnimBillboard => 0x88,
        FxidComponentType.Trail => 0x30,
        FxidComponentType.Point => -1,
        FxidComponentType.Plane => 0x90,
        FxidComponentType.Model => 0xB8,
        FxidComponentType.AnimModel => 0x88,
        FxidComponentType.Cube => 0x90,
        _ => -1
    };

    /// <summary>
    /// Size of the data after the resource reference. Type 4: one float (in the files: 300/500/700, looks like a distance);
    /// type 9: two dwords (in the files: 0/1 values).
    /// </summary>
    public static int GetTrailingSize(FxidComponentType type)
    {
        var refOffset = GetResourceRefOffset(type);
        return refOffset < 0 ? 0 : GetSize(type) - refOffset - ResourceNamedRef.Size;
    }
}

/// <summary>One component of an FXID file. <see cref="Raw"/> always holds the exact on-disk bytes (for lossless round-trip).</summary>
public sealed record FxidComponent(
    int Index,
    int FileOffset,
    FxidComponentType Type,
    uint TypeAndFlags,
    byte[] Raw,
    ResourceNamedRef? Resource)
{
    /// <summary>
    /// Bit 8 of <c>TypeAndFlags</c>. Copied by the engine into byte +4 of the runtime effect object
    /// and, together with header flag 0x400, sets <c>EffectHandle.stateFlags |= 0x10</c>. Meaning TBD.
    /// In the files: set in types 3, 4, 7, 8.
    /// </summary>
    public bool TypeFlagBit8_TBD => ((TypeAndFlags >> 8) & 1) != 0;

    /// <summary>Typed view for <see cref="FxidComponentType.AnimParticle"/>; throws for other types.</summary>
    public EffectType3Data AsType3()
    {
        if (Type != FxidComponentType.AnimParticle)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimParticle");
        return EffectType3Data.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.Trail"/> (type 5): a motion streak between the previous and current attach point.</summary>
    public EffectTrailData AsTrail()
    {
        if (Type != FxidComponentType.Trail)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Trail");
        return EffectTrailData.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.Model"/> (type 8): a particle spawner that emits plane particles along the path of the attach point.</summary>
    public EffectSpawnerData AsSpawner()
    {
        if (Type != FxidComponentType.Model)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Model");
        return EffectSpawnerData.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.Sound"/> (type 2).</summary>
    public EffectSoundData AsSound()
    {
        if (Type != FxidComponentType.Sound)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Sound");
        return EffectSoundData.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.Plane"/> (type 7) and <see cref="FxidComponentType.Cube"/> (type 10): particle emitters.</summary>
    public EffectParticleData AsParticle()
    {
        if (Type is not (FxidComponentType.Plane or FxidComponentType.Cube))
            throw new InvalidOperationException($"Component {Index} is {Type}, not Plane/Cube");
        return EffectParticleData.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.Billboard"/> (a dynamic light in the engine); throws for other types.</summary>
    public EffectType1Data AsType1()
    {
        if (Type != FxidComponentType.Billboard)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Billboard");
        return EffectType1Data.Read(Raw);
    }

    /// <summary>Typed view for <see cref="FxidComponentType.AnimBillboard"/>: the type-3 layout plus one trailing float.</summary>
    public EffectType4Data AsType4()
    {
        if (Type != FxidComponentType.AnimBillboard)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimBillboard");
        return new EffectType4Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadSingleLittleEndian(Raw.AsSpan(0xC8)));
    }

    /// <summary>Typed view for <see cref="FxidComponentType.AnimModel"/>: shares SetParams/EvaluateAtTime with type 3, so the first 0xC8 bytes have the type-3 layout.</summary>
    public EffectType9Data AsType9()
    {
        if (Type != FxidComponentType.AnimModel)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimModel");
        return new EffectType9Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xCC)));
    }
}

/// <summary>A whole FXID item (payload of one NRes entry with type "FXID").</summary>
public sealed record FxidFile(EffectHeader Header, IReadOnlyList<FxidComponent> Components)
{
    public const int HeaderSize = 0x3C;

    /// <summary>Parses an FXID payload. Throws <see cref="FxidFormatException"/> if sizes do not add up exactly.</summary>
    public static FxidFile Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize)
            throw new FxidFormatException($"FXID payload is too short: {data.Length} bytes");

        var header = new EffectHeader
        {
            ComponentCount = U32(data, 0x00),
            StrengthSource = (EffectStrengthSource)U32(data, 0x04),
            Duration = F32(data, 0x08),
            StrengthJitter = F32(data, 0x0C),
            Flags = (FxidEffectFlags)U32(data, 0x10),
            SettingsId = U32(data, 0x14),
            RandShiftX = F32(data, 0x18),
            RandShiftY = F32(data, 0x1C),
            RandShiftZ = F32(data, 0x20),
            PivotX = F32(data, 0x24),
            PivotY = F32(data, 0x28),
            PivotZ = F32(data, 0x2C),
            ScaleX = F32(data, 0x30),
            ScaleY = F32(data, 0x34),
            ScaleZ = F32(data, 0x38),
        };

        var components = new List<FxidComponent>((int)Math.Min(header.ComponentCount, 64u));
        var offset = HeaderSize;
        for (var i = 0; i < header.ComponentCount; i++)
        {
            if (offset + 4 > data.Length)
                throw new FxidFormatException($"Component {i}: truncated at offset 0x{offset:X}");

            var typeAndFlags = U32(data, offset);
            var type = (FxidComponentType)(typeAndFlags & 0xFF);
            // The engine does not advance on an unknown type (it would re-read the same block);
            // we treat it as a format error instead.
            var size = FxidComponentLayout.GetSize(type);
            if (offset + size > data.Length)
                throw new FxidFormatException($"Component {i} ({type}): truncated at offset 0x{offset:X}");

            var raw = data.Slice(offset, size).ToArray();
            ResourceNamedRef? resource = null;
            var refOffset = FxidComponentLayout.GetResourceRefOffset(type);
            if (refOffset >= 0)
                resource = ResourceNamedRef.Read(raw.AsSpan(refOffset, ResourceNamedRef.Size));

            components.Add(new FxidComponent(i, offset, type, typeAndFlags, raw, resource));
            offset += size;
        }

        if (offset != data.Length)
            throw new FxidFormatException(
                $"FXID size mismatch: components end at 0x{offset:X}, payload is 0x{data.Length:X} bytes");

        return new FxidFile(header, components);
    }

    private static uint U32(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
    private static float F32(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
}

/// <summary>
/// Typed view of component type 3 (<c>EffectType3FileData</c> in Ghidra, runtime class <c>EffectType3</c>).
/// Field meaning comes from <c>EffectType3::SetParams / EvaluateAtTime / method3</c> (Effect.dll);
/// names with _maybe / _TBD are inferred from usage and not fully confirmed.
/// </summary>
/// <remarks>
/// Replaces the earlier <c>AnimParticleComponentData</c>, whose layout was shifted by 4 bytes
/// from +0x28 on and overlapped the resource reference.
/// </remarks>
public sealed record EffectType3Data(
    uint TypeAndFlags,
    // +0x04: ESpace (0 mesh, 1 local, 2 world); copied to EffectContents.space.
    uint Space,
    // +0x08..0x10: value v = pow(t, PhaseExponent) * (PhaseEnd - max(PhaseStart, 0)) + max(PhaseStart, 0);
    // t = effect strength when PhaseStart < 0 (in the files: -1.0 in 317 of 394), otherwise normalized life.
    // The fractional part of v is passed to the render call as a phase (animation frame?).
    float PhaseStart,
    float PhaseEnd,
    float PhaseExponent,      // in the files: always 1.0
    // +0x14..0x1C: lerp(LifeLerpStart, LifeLerpEnd, pow(life, IntensityExponent)) is passed to the render call (alpha/size?).
    float IntensityStart,
    float IntensityEnd,
    float IntensityExponent,         // in the files: ~always 1.0
    // +0x20..0x24: component is active while ActiveTimeStart <= t <= ActiveTimeEnd; life = (t - start) / (end - start).
    // Units: EvaluateAtTime.param_2[1] (ctx dword1) is the effect strength value 0..1 (see FxidStrength), NOT necessarily time: types 7/10 (verified in 10001240)
    // compare it with their window and normalise (v-lo)/(hi-lo). With life-based StrengthSource it equals normalized life; type 1 uses the same convention (windows like 0.75..1.0 occur).
    float ActiveTimeStart,
    float ActiveTimeEnd,
    // Animated over life: value = Start + (End - Start) * pow(life, Exponent) per axis.
    Vector3 PositionStart,          // +0x28
    Vector3 PositionEnd,            // +0x34
    Vector3 PositionExponent,       // +0x40 (in the files: (1,1,1) almost always)
    Vector3 DirectionStart,         // +0x4C (exponent is not used: linear in life)
    Vector3 DirectionEnd,           // +0x58
    Vector3 ScaleStart,             // +0x64
    Vector3 ScaleEnd,               // +0x70
    Vector3 ScaleExponent,          // +0x7C
    ResourceNamedRef Resource)       // +0x88
{
    public static EffectType3Data Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length < 0xC8)
            throw new FxidFormatException($"Type 3 block must be at least 0xC8 bytes, got 0x{raw.Length:X}");

        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));

        return new EffectType3Data(
            BinaryPrimitives.ReadUInt32LittleEndian(raw),
            BinaryPrimitives.ReadUInt32LittleEndian(raw[4..]),
            F(raw, 0x08), F(raw, 0x0C), F(raw, 0x10),
            F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C),
            F(raw, 0x20), F(raw, 0x24),
            V(raw, 0x28), V(raw, 0x34), V(raw, 0x40),
            V(raw, 0x4C), V(raw, 0x58),
            V(raw, 0x64), V(raw, 0x70), V(raw, 0x7C),
            ResourceNamedRef.Read(raw.Slice(0x88, ResourceNamedRef.Size)));
    }

    /// <summary>True when the phase is driven by the effect strength instead of local life (PhaseStart &lt; 0).</summary>
    public bool PhaseUsesEffectStrength => PhaseStart < 0f;

    /// <summary>Normalized life 0..1 at time t, or null outside the active window (the engine stores -1 there).</summary>
    public float? LifeAt(float t) =>
        t >= ActiveTimeStart && t <= ActiveTimeEnd
            ? (t - ActiveTimeStart) / (ActiveTimeEnd - ActiveTimeStart)
            : null;

    /// <summary>
    /// Bounding radius exactly as <c>EffectType3::SetParams</c> computes it (stored in the 4th dword of the
    /// effect's first 0x10 bytes): max(maxComponent(PositionStart), maxComponent(PositionEnd))
    /// + max(|ScaleStart|, |ScaleEnd|). Note: max of signed components, not of absolute values.
    /// </summary>
    public float BoundRadius()
    {
        static float MaxComp(Vector3 v) => MathF.Max(v.X, MathF.Max(v.Y, v.Z));
        static float Len(Vector3 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return MathF.Max(MaxComp(PositionStart), MaxComp(PositionEnd)) + MathF.Max(Len(ScaleStart), Len(ScaleEnd));
    }
}

/// <summary>
/// Type 4: the type-3 layout plus a trailing float at +0xC8 (in the files: 150/300/500/700).
/// VERIFIED (SetParamsWithReferenceDistance 10011650 / EffectType4::RenderContents): stores 1 / value; at render time the animation
/// progress becomes clamp(distance_to_camera * (1 / value) * life, 0..1), where distance_to_camera is the length of the reference
/// vector (camera position transformed into the effect's local space). So the value is the distance at which the animation is complete.
/// </summary>
public sealed record EffectType4Data(EffectType3Data Base, float ReferenceDistance);

/// <summary>
/// Type 9: type-3 prefix (same constructor, SetParams and EvaluateAtTime as type 3; only the vtable and Render at 100149e0 differ) plus two dwords.
/// Render sets the render flags of <c>EffectContents</c> (field at +8) and then calls the type-3 content render:
/// <c>EffectContents.render_group</c> is really a render-flags word (types 3/4 use 1): base value by <see cref="DrawMode_maybe"/> (0 -> 3, 1 -> 5, 2 -> 6), the render-context quality level (0..3) goes into the top two bits
/// (level &lt;&lt; 30), and <see cref="Flag_TBD"/> == 0 additionally sets 0x8000000. In the files: (0,0) 129, (1,0) 66, (1,1) 6 (mode 2 never occurs).
/// </summary>
public sealed record EffectType9Data(EffectType3Data Base, uint DrawMode_maybe, uint Flag_TBD);

/// <summary>
/// Type 1 (named Billboard in FxidLib). EffectType1::SetParams works with ILightManager, so the engine treats it as a
/// dynamic light with animated position, direction, colour and range. Layout from SetParams and the files (e.g. aim_light_L:
/// direction (0,-1,0), colour (1,0.1,0.1) -> (2.5,0.1,0.1), range 3 -> 12). Bytes after +0x78 up to the resource at +0xA0 are not decoded yet.
/// </summary>
public sealed record EffectType1Data(
    uint TypeAndFlags,
    uint LightType_maybe,       // +0x04: 1,2,5,6,7 in the files (3 and 4 are handled by the engine); mapped to ILightManager type + flags
    float ActiveTimeStart,      // +0x08: window on the effect strength value (ctx dword1); = normalized life for life-based sources
    float ActiveTimeEnd,        // +0x0C
    Vector3 PositionStart,      // +0x10
    Vector3 PositionEnd,        // +0x1C
    Vector3 DirectionStart_maybe, // +0x28
    Vector3 DirectionEnd_maybe,   // +0x34
    float[] ColorStart_maybe,   // +0x40: 4 floats, RGB + 4th (0 in the sample files); values above 1 occur
    float[] ColorEnd_maybe,     // +0x50
    float[] ColorJitter_TBD,    // +0x60: random amplitude per channel
    float RangeStart_maybe,     // +0x70
    float RangeEnd_maybe,       // +0x74
    ResourceNamedRef Resource)   // +0xA0
{
    public static EffectType1Data Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xE0)
            throw new FxidFormatException($"Type 1 block must be 0xE0 bytes, got 0x{raw.Length:X}");

        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        static float[] Q(ReadOnlySpan<byte> d, int o) => [F(d, o), F(d, o + 4), F(d, o + 8), F(d, o + 12)];

        return new EffectType1Data(
            BinaryPrimitives.ReadUInt32LittleEndian(raw),
            BinaryPrimitives.ReadUInt32LittleEndian(raw[4..]),
            F(raw, 0x08), F(raw, 0x0C),
            V(raw, 0x10), V(raw, 0x1C), V(raw, 0x28), V(raw, 0x34),
            Q(raw, 0x40), Q(raw, 0x50), Q(raw, 0x60),
            F(raw, 0x70), F(raw, 0x74),
            ResourceNamedRef.Read(raw.Slice(0xA0, ResourceNamedRef.Size)));
    }
}


/// <summary>
/// Types 7 (Plane, particle object 0xCC bytes, non-looping) and 10 (Cube, particle object 0x1EC bytes, looping) are particle emitters.
/// They share EvaluateAtTime (10001240), Render (10001310) and the init helper 10001010; SetParams differs (100017e0 / 10001b70).
/// Runtime objects are identical (0x48 bytes, EffectBase + file ptr + arrays); the types differ only in the particle element class returned by
/// vtable slot 7 (<c>GetParticle(index)</c>): type 7 = 0xCC-byte particle, type 10 = 0x1EC-byte particle, plus SetParams (type 7 sets the object flag
/// 'not looping' = 1 and reads SizeEnd; type 10 sets it to 0, ignores SizeEnd, and falls back to a random material if the resource fails to load).
/// Particle count = Groups * PerGroup. Verified by reading the code; field meanings marked _maybe/_TBD are not confirmed in-game.
/// </summary>
public sealed record EffectParticleData(
    uint TypeAndFlags,
    uint Mode_maybe,                // +0x04: VERIFIED. Type 7: 0/2 -> particle render value 0, 1 -> 3. Type 10: 0x10/0x20 pick the polyhedron shape table (see README, FUN_1000caf0)
    float ValueLerpStart_maybe,     // +0x08: lerp start used by Render (t^ValueExponent); in the files 1.0 -> 0.0..0.5, looks like an opacity fade
    float ValueLerpEnd_maybe,       // +0x0C
    float ValueExponent_maybe,      // +0x10
    float ActiveWindowStart,        // +0x14: window on ctx dword1 (strength); EvaluateAtTime stores (v-lo)/(hi-lo) in object+0x1C, -1 outside
    float ActiveWindowEnd,          // +0x18
    float ParticleLifeFraction,     // +0x1C: lifetime of one particle in normalized units; group start offset i = (1-this)*i/Groups
    float RepeatRate,               // +0x20: <=1 divides time, >1 multiplies and takes the fractional part (periodic respawn)
    uint Groups,                    // +0x24
    uint PerGroup,                  // +0x28
    Vector3 TrajectoryStart,        // +0x2C: particle path start (local space)
    Vector3 TrajectoryEnd,          // +0x38: path end; random box added
    Vector3 SpawnBox,               // +0x44: random spawn jitter box (centered)
    Vector3 ColorExponent_maybe,    // +0x50: per-channel pow exponents applied to t in Render
    Vector3 SizeStart,              // +0x5C
    Vector3 SizeEnd,                // +0x68: used by type 7 only
    Vector3 SizeJitterBox,          // +0x74
    Vector3 SizeExponent_maybe,     // +0x80
    float GravityFactor,      // +0x8C: VERIFIED. gravity = this * IWorld::QueryGravity() * 0.5 (InitCommon); drop = gravity * t^2 on the 3rd axis (0/1/2 for type 7, 1/1.5/3 for type 10)
    ResourceNamedRef Resource)       // +0x90: material (falls back to a random material if it fails to load)
{
    public static EffectParticleData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xD0)
            throw new FxidFormatException($"Particle block must be 0xD0 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectParticleData(
            U(raw, 0), U(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20),
            U(raw, 0x24), U(raw, 0x28),
            V(raw, 0x2C), V(raw, 0x38), V(raw, 0x44), V(raw, 0x50), V(raw, 0x5C), V(raw, 0x68), V(raw, 0x74), V(raw, 0x80),
            F(raw, 0x8C),
            ResourceNamedRef.Read(raw.Slice(0x90, ResourceNamedRef.Size)));
    }
}


/// <summary>
/// Type 5 (Trail), runtime object 0x54 bytes, file block 0x70. Code: SetParams 10002a50, EvaluateAtTime 10002bd0, Render 10002da0,
/// OnAttachPointMoved 10003290 (stores the new point as the trail start).
/// The component remembers the attach point of the previous update and draws a strip from it to the current point, split into
/// <c>ceil(length / MaxPieceLength)</c> pieces (at most <see cref="MaxPieces"/>), each piece being a 0x14C-byte quad object.
/// Updates only run while the strength value (ctx dword1) is inside [start, end); outside it the piece count is reset to 0 (SetParams starts with 1).
/// </summary>
public sealed record EffectTrailData(
    uint TypeAndFlags,
    float ValueStart,        // +0x04: VERIFIED (Render): value = ValueStart + window * (ValueEnd - ValueStart), passed to the draw call; always 1.0 in the files (looks like opacity)
    float ValueEnd,        // +0x08: always 0.0 in the files
    float ActiveWindowStart,    // +0x0C: window on ctx dword1 (strength), half-open [start, end): at value == end the trail disappears
    float ActiveWindowEnd,      // +0x10
    uint MaxPieces,             // +0x14: ring size, also the cap for the piece count
    float WidthStart,           // +0x18: piece scale (y,z) at the start; object scale = (1, w, w)
    float WidthEnd,             // +0x1C: piece scale at the end (lerped across the strip)
    float TextureTileLength,    // +0x20: world length covered by one texture repeat (u scale = pieceLength / this)
    float MaxPieceLength,       // +0x24: target length of one piece
    float TextureScrollStart,   // +0x28: u offset at window value 0; if negative the scroll is driven by elapsed seconds instead (and start is clamped to 0)
    float TextureScrollEnd,     // +0x2C: u offset at window value 1; if negative each piece gets a random u offset
    ResourceNamedRef Resource)   // +0x30: material
{
    public static EffectTrailData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0x70)
            throw new FxidFormatException($"Type 5 block must be 0x70 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        return new EffectTrailData(
            U(raw, 0), F(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), U(raw, 0x14),
            F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20), F(raw, 0x24), F(raw, 0x28), F(raw, 0x2C),
            ResourceNamedRef.Read(raw.Slice(0x30, ResourceNamedRef.Size)));
    }
}

/// <summary>
/// Type 8 (named Model in FxidLib; the name is legacy), runtime object 0xAC bytes, file block 0xF8. Code: SetParams 10012000,
/// EvaluateAtTime 100123b0, Render 10012f90. SetParams copies the header's SettingsId (FxidHeader +0x14) into the object (+0x58; types 7/10 do the same at +0x34), probably for quality-dependent decisions. A spawner: while the attach point moves, it emits particles (0xCC-byte plane
/// particles, the same class type 7 uses) into a ring buffer of <see cref="MaxParticles"/> slots. The particle block at +0x58..+0xB7 has
/// exactly the layout of the type 7/10 block at +0x2C..+0x8B (shifted by 0x2C).
/// </summary>
public sealed record EffectSpawnerData(
    uint TypeAndFlags,
    float ValueLerpStart,        // +0x04: VERIFIED (Render): per-particle value = start + pow(exponent, life) * (end - start), the 4th argument of the particle draw call; 0.8..1.0 (looks like opacity)
    float ValueLerpEnd,        // +0x08: 0 / 0.1 / 0.2
    float ValueLerpExponent,        // +0x0C: always 1.0
    float ActiveWindowStart,    // +0x10: window on ctx dword1 (strength)
    float ActiveWindowEnd,      // +0x14
    float SpawnIntervalStart,   // +0x18: seconds between spawns at window value 0 (see object+0x40/+0x44/+0x48)
    float SpawnIntervalEnd,     // +0x1C: at window value 1
    float LifeExponent_unused,        // +0x20: Render computes pow(this, life) but discards the result (dead code)
    uint MaxParticles,          // +0x24: ring buffer size
    Vector3 SpawnOffsetStart,   // +0x28: offset of the spawn point from the attach point at window value 0 (in the files: always 0,0,0; end/jitter 0, exponent 1,1,1)
    Vector3 SpawnOffsetEnd,     // +0x34: at window value 1
    Vector3 SpawnOffsetJitter,  // +0x40: random box added to SpawnOffsetEnd per particle
    Vector3 SpawnOffsetExponent_maybe, // +0x4C: per-axis pow exponent of the offset lerp
    Vector3 TrajectoryStart,    // +0x58: particle block (see EffectParticleData.TrajectoryStart)
    Vector3 TrajectoryEnd,      // +0x64
    Vector3 SpawnBox,           // +0x70
    Vector3 ColorExponent_maybe,// +0x7C
    Vector3 SizeStart,          // +0x88
    Vector3 SizeEnd,            // +0x94
    Vector3 SizeJitterBox,      // +0xA0
    Vector3 SizeExponent_maybe, // +0xAC
    ResourceNamedRef Resource)   // +0xB8: material
{
    public static EffectSpawnerData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xF8)
            throw new FxidFormatException($"Type 8 block must be 0xF8 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectSpawnerData(
            U(raw, 0), F(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20), U(raw, 0x24),
            V(raw, 0x28), V(raw, 0x34), V(raw, 0x40), V(raw, 0x4C),
            V(raw, 0x58), V(raw, 0x64), V(raw, 0x70), V(raw, 0x7C), V(raw, 0x88), V(raw, 0x94), V(raw, 0xA0), V(raw, 0xAC),
            ResourceNamedRef.Read(raw.Slice(0xB8, ResourceNamedRef.Size)));
    }
}

/// <summary>
/// Type 2 (Sound), runtime object 0xA0 bytes (file pointer at +0x24, unlike the other types), file block 0x94.
/// Code: ctor 10013d70, SetParams 10013df0, EvaluateAtTime 10013f90, OnEnabledChanged 10014250 (stops the sound when disabled).
/// <see cref="Mode"/> 2 and 3 are looped sounds that play while the strength value (ctx dword1) is inside
/// [<see cref="WindowStart"/>, <see cref="WindowEnd"/>]; other values are one-shot sounds that start when the strength crosses
/// <see cref="WindowStart"/> upward. The wave is loaded through the sound manager (<c>I3DSound::LoadWaveSoundBuffer</c>) from <see cref="Resource"/>.
/// Position offset and volume are lerped by the strength value; the position is transformed by the parent matrix; with min/max distance and Unknown50
/// they are sent to the sound system as a 3D parameter block (command 0x38E). The sound's bounding sphere is read back from the same call.
/// </summary>
public sealed record EffectSoundData(
    uint TypeAndFlags,
    uint Mode,                  // +0x04: 2,3 = looped; also passed to the wave loader
    float WindowStart,          // +0x08
    float WindowEnd,            // +0x0C
    Vector3 PositionOffsetStart, // +0x10: VERIFIED: lerp by strength, then transformed by the parent matrix to a world position; in the files (0,0,0), (0,-3,0), (0,0,-1); start == end in all files
    Vector3 PositionOffsetEnd,   // +0x1C
    Vector3 DirectionStart_unused,      // +0x28: copied to runtime in SetParams but NOT read by EvaluateAtTime; in the files always (1,0,0)
    Vector3 DirectionEnd_unused,        // +0x34
    float MinDistance,    // +0x40: VERIFIED constant (SetParams makes no lerp), 1..150 in the files; goes to the sound parameters
    float MaxDistance,    // +0x44: VERIFIED constant, 15..1500, always > MinDistance
    float VolumeStart,    // +0x48: VERIFIED: volume = start + strength * (end - start); 1.0 in 198 of 199 files
    float VolumeEnd,      // +0x4C: always 1.0
    float Unknown50_TBD,        // +0x50: passed as the last field of the sound parameter block (command 0x38E); always 0 in the files
    ResourceNamedRef Resource)   // +0x54: wave file
{
    public bool IsLooped => Mode is 2 or 3;

    public static EffectSoundData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0x94)
            throw new FxidFormatException($"Type 2 block must be 0x94 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectSoundData(
            U(raw, 0), U(raw, 4), F(raw, 8), F(raw, 0xC),
            V(raw, 0x10), V(raw, 0x1C), V(raw, 0x28), V(raw, 0x34),
            F(raw, 0x40), F(raw, 0x44), F(raw, 0x48), F(raw, 0x4C), F(raw, 0x50),
            ResourceNamedRef.Read(raw.Slice(0x54, ResourceNamedRef.Size)));
    }
}
