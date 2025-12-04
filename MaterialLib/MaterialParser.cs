using System.Buffers.Binary;
using System.Text;

namespace MaterialLib;

public static class MaterialParser
{
    public static MaterialFile ReadFromStream(Stream fs, string fileName, int elementCount, int magic1)
    {
        var file = new MaterialFile
        {
            FileName = fileName,
            Version = elementCount,
            Magic1 = magic1
        };

        // Derived fields
        file.MaterialRenderingType = elementCount >> 2 & 0xf;
        file.SupportsBumpMapping = (elementCount & 2) != 0;
        file.IsParticleEffect = elementCount & 40;

        // Reading content
        var stageCount = fs.ReadUInt16LittleEndian();
        var animCount = fs.ReadUInt16LittleEndian();

        uint magic = (uint)magic1;

        // Defaults found in C code
        file.GlobalAlphaMultiplier = 1.0f; // field8_0x15c
        file.GlobalEmissiveIntensity = 0.0f; // field9_0x160
        file.SourceBlendMode = BlendMode.Unknown;   // field6_0x154
        file.DestBlendMode = BlendMode.Unknown;   // field7_0x158

        if (magic >= 2)
        {
            file.SourceBlendMode = (BlendMode)fs.ReadByte();
            file.DestBlendMode = (BlendMode)fs.ReadByte();
        }

        if (magic > 2)
        {
            file.GlobalAlphaMultiplier = fs.ReadFloatLittleEndian();
        }

        if (magic > 3)
        {
            file.GlobalEmissiveIntensity = fs.ReadFloatLittleEndian();
        }

        // --- 2. Material Stages ---
        const float Inv255 = 1.0f / 255.0f;
        const float Field7Mult = 0.01f;
        Span<byte> textureNameBuffer = stackalloc byte[16];

        for (int i = 0; i < stageCount; i++)
        {
            var stage = new MaterialStage();

            // === FILE READ ORDER (matches decompiled.c lines 159-217) ===
            
            // 1. Ambient (4 bytes, A scaled by 0.01) - Lines 159-168
            stage.AmbientR = fs.ReadByte() * Inv255;
            stage.AmbientG = fs.ReadByte() * Inv255;
            stage.AmbientB = fs.ReadByte() * Inv255;
            stage.AmbientA = fs.ReadByte() * Field7Mult; // 0.01 scaling

            // 2. Diffuse (4 bytes) - Lines 171-180
            stage.DiffuseR = fs.ReadByte() * Inv255;
            stage.DiffuseG = fs.ReadByte() * Inv255;
            stage.DiffuseB = fs.ReadByte() * Inv255;
            stage.DiffuseA = fs.ReadByte() * Inv255;

            // 3. Specular (4 bytes) - Lines 183-192
            stage.SpecularR = fs.ReadByte() * Inv255;
            stage.SpecularG = fs.ReadByte() * Inv255;
            stage.SpecularB = fs.ReadByte() * Inv255;
            stage.SpecularA = fs.ReadByte() * Inv255;

            // 4. Emissive (4 bytes) - Lines 195-204
            stage.EmissiveR = fs.ReadByte() * Inv255;
            stage.EmissiveG = fs.ReadByte() * Inv255;
            stage.EmissiveB = fs.ReadByte() * Inv255;
            stage.EmissiveA = fs.ReadByte() * Inv255;

            // 5. Power (1 byte → float) - Line 207
            stage.Power = (float)fs.ReadByte();
            
            // 6. Texture Stage Index (1 byte) - Line 210
            stage.TextureStageIndex = fs.ReadByte();

            // 7. Texture Name (16 bytes) - Lines 212-217
            textureNameBuffer.Clear();
            fs.ReadExactly(textureNameBuffer);
            stage.TextureName = Encoding.ASCII.GetString(textureNameBuffer).TrimEnd('\0');

            file.Stages.Add(stage);
        }

        // --- 3. Animations ---
        for (int i = 0; i < animCount; i++)
        {
            var anim = new MaterialAnimation();

            uint typeAndParams = fs.ReadUInt32LittleEndian();
            anim.Target = (AnimationTarget)(typeAndParams >> 3);
            anim.LoopMode = (AnimationLoopMode)(typeAndParams & 7);

            ushort keyCount = fs.ReadUInt16LittleEndian();

            for (int k = 0; k < keyCount; k++)
            {
                var key = new AnimKey
                {
                    StageIndex = fs.ReadUInt16LittleEndian(),
                    DurationMs = fs.ReadUInt16LittleEndian(),
                    InterpolationCurve = fs.ReadUInt16LittleEndian()
                };
                anim.Keys.Add(key);
            }
            
            // Precompute description for UI to avoid per-frame allocations
            anim.TargetDescription = ComputeTargetDescription(anim.Target);
            
            file.Animations.Add(anim);
        }

        return file;
    }
    
    private static string ComputeTargetDescription(AnimationTarget target)
    {
        // Precompute the description once during parsing
        if ((int)target == 0)
            return "No interpolation - entire stage is copied as-is (Flags: 0x0)";
        
        var parts = new List<string>();
        
        if ((target & AnimationTarget.Ambient) != 0)
            parts.Add("Ambient RGB");
        if ((target & AnimationTarget.Diffuse) != 0)
            parts.Add("Diffuse RGB");
        if ((target & AnimationTarget.Specular) != 0)
            parts.Add("Specular RGB");
        if ((target & AnimationTarget.Emissive) != 0)
            parts.Add("Emissive RGB");
        if ((target & AnimationTarget.Power) != 0)
            parts.Add("Ambient.A + Power");
            
        return $"Interpolates: {string.Join(", ", parts)} | Other components copied (Flags: 0x{(int)target:X})";
    }
}

// Helper extensions
internal static class StreamExtensions
{
    public static float ReadFloatLittleEndian(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleLittleEndian(buffer);
    }
    
    public static ushort ReadUInt16LittleEndian(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }
    
    public static uint ReadUInt32LittleEndian(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }
}
