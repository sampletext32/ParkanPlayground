namespace MaterialLib;

public class MaterialFile
{
    // === Metadata (not from file content) ===
    public string FileName { get; set; }
    public int Version { get; set; }  // From NRes ElementCount
    public int Magic1 { get; set; }   // From NRes Magic1
    
    // === Derived from Version/ElementCount ===
    public int MaterialRenderingType { get; set; } // (Version >> 2) & 0xF - 0=Standard, 1=Special, 2=Particle
    public bool SupportsBumpMapping { get; set; } // (Version & 2) != 0
    public int IsParticleEffect { get; set; } // Version & 40 - 0=Normal, 8=Particle/Effect
    
    // === File Content (in read order) ===
    // Read order: StageCount (ushort), AnimCount (ushort), then conditionally blend modes and params
    
    // Global Blend Modes (read if Magic1 >= 2)
    public BlendMode SourceBlendMode { get; set; } // Default: Unknown (0xFF)
    public BlendMode DestBlendMode { get; set; }   // Default: Unknown (0xFF)
    
    // Global Parameters (read if Magic1 > 2 and > 3 respectively)
    public float GlobalAlphaMultiplier { get; set; } // Default: 1.0 (always 1.0 in all 628 materials)
    public float GlobalEmissiveIntensity { get; set; } // Default: 0.0 (0=no glow, rare values: 1000, 10000)

    public List<MaterialStage> Stages { get; set; } = new();
    public List<MaterialAnimation> Animations { get; set; } = new();
}

/// <summary>
/// Blend modes for material rendering. These control how source and destination colors are combined.
/// Formula: FinalColor = (SourceColor * SourceBlend) [operation] (DestColor * DestBlend)
/// Maps to Direct3D D3DBLEND values.
/// </summary>
public enum BlendMode : byte
{
    /// <summary>Blend factor is (0, 0, 0, 0) - results in black/transparent</summary>
    Zero = 1,
    
    /// <summary>Blend factor is (1, 1, 1, 1) - uses full color value</summary>
    One = 2,
    
    /// <summary>Blend factor is (Rs, Gs, Bs, As) - uses source color</summary>
    SrcColor = 3,
    
    /// <summary>Blend factor is (1-Rs, 1-Gs, 1-Bs, 1-As) - uses inverted source color</summary>
    InvSrcColor = 4,
    
    /// <summary>Blend factor is (As, As, As, As) - uses source alpha for all channels (standard transparency)</summary>
    SrcAlpha = 5,
    
    /// <summary>Blend factor is (1-As, 1-As, 1-As, 1-As) - uses inverted source alpha</summary>
    InvSrcAlpha = 6,
    
    /// <summary>Blend factor is (Ad, Ad, Ad, Ad) - uses destination alpha</summary>
    DestAlpha = 7,
    
    /// <summary>Blend factor is (1-Ad, 1-Ad, 1-Ad, 1-Ad) - uses inverted destination alpha</summary>
    InvDestAlpha = 8,
    
    /// <summary>Blend factor is (Rd, Gd, Bd, Ad) - uses destination color</summary>
    DestColor = 9,
    
    /// <summary>Blend factor is (1-Rd, 1-Gd, 1-Bd, 1-Ad) - uses inverted destination color</summary>
    InvDestColor = 10,
    
    /// <summary>Blend factor is (f, f, f, 1) where f = min(As, 1-Ad) - saturates source alpha</summary>
    SrcAlphaSat = 11,
    
    /// <summary>Blend factor is (As, As, As, As) for both source and dest - obsolete in D3D9+</summary>
    BothSrcAlpha = 12,
    
    /// <summary>Blend factor is (1-As, 1-As, 1-As, 1-As) for both source and dest - obsolete in D3D9+</summary>
    BothInvSrcAlpha = 13,
    
    /// <summary>Unknown or uninitialized blend mode (0xFF default value)</summary>
    Unknown = 0xFF
}

public class MaterialStage
{
    // === FILE READ ORDER (34 bytes per stage) ===
    // This matches the order bytes are read from the file (decompiled.c lines 159-217)
    // NOT the C struct memory layout (which is Diffuse, Ambient, Specular, Emissive)
    
    // 1. Ambient Color (4 bytes, read first from file)
    public float AmbientR;
    public float AmbientG;
    public float AmbientB;
    public float AmbientA; // Scaled by 0.01 when read from file
    
    // 2. Diffuse Color (4 bytes, read second from file)
    public float DiffuseR;
    public float DiffuseG;
    public float DiffuseB;
    public float DiffuseA;

    // 3. Specular Color (4 bytes, read third from file)
    public float SpecularR;
    public float SpecularG;
    public float SpecularB;
    public float SpecularA;

    // 4. Emissive Color (4 bytes, read fourth from file)
    public float EmissiveR;
    public float EmissiveG;
    public float EmissiveB;
    public float EmissiveA;

    // 5. Power (1 byte → float, read fifth from file)
    public float Power;
    
    // 6. Texture Stage Index (1 byte, read sixth from file)
    // 255 = not set/default, 0-47 = reference to specific texture stage
    public int TextureStageIndex;
    
    // 7. Texture Name (16 bytes, read seventh from file)
    public string TextureName { get; set; }
}

public class MaterialAnimation
{
    // === File Read Order ===
    
    // Combined field (4 bytes): bits 3-31 = Target, bits 0-2 = LoopMode
    public AnimationTarget Target;
    public AnimationLoopMode LoopMode;
    
    // Key count (2 bytes), then keys
    public List<AnimKey> Keys { get; set; } = new();
    
    // Cached description for UI (computed once during parsing)
    public string TargetDescription { get; set; } = string.Empty;
}


[Flags]
public enum AnimationTarget : int
{
    // NOTE: This is a BITSET (flags enum). Multiple flags can be combined.
    // When a flag is SET, that component is INTERPOLATED between stages.
    // When a flag is NOT SET, that component is COPIED from the source stage (no interpolation).
    // If ALL flags are 0, the ENTIRE stage is copied without any interpolation.
    
    Ambient = 1,      // 0x01 - Interpolates Ambient RGB (Interpolate.c lines 23-37)
    Diffuse = 2,      // 0x02 - Interpolates Diffuse RGB (Interpolate.c lines 7-21)
    Specular = 4,     // 0x04 - Interpolates Specular RGB (Interpolate.c lines 39-53)
    Emissive = 8,     // 0x08 - Interpolates Emissive RGB (Interpolate.c lines 55-69)
    Power = 16        // 0x10 - Interpolates Ambient.A and sets Power (Interpolate.c lines 71-76)
}


public enum AnimationLoopMode : int
{
    Loop = 0,
    PingPong = 1,
    Clamp = 2,
    Random = 3
}

public struct AnimKey
{
    // === File Read Order (6 bytes per key) ===
    public ushort StageIndex;  // Read first
    public ushort DurationMs;  // Read second
    public ushort InterpolationCurve;  // Read third - Always 0 (linear interpolation) in all 1848 keys
}
