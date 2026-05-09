using System.Buffers.Binary;
using System.Text;

namespace MaterialLib;

public static class MaterialParser
{
    public static MaterialFile ReadFromStream(Stream fs, string fileName, int elementCount, int magic1)
    {
        var materialRenderingType = elementCount >> 2 & 0xf;
        var supportsBumpMapping = (elementCount & 2) != 0;
        var isParticleEffect = elementCount & 40;

        // Reading content
        var stageCount = fs.ReadUInt16LittleEndian();
        var animCount = fs.ReadUInt16LittleEndian();

        uint magic = (uint)magic1;

        // Значения по умолчанию из C-кода.
        var globalAlphaMultiplier = 1.0f; // field8_0x15c
        var globalEmissiveIntensity = 0.0f; // field9_0x160
        var sourceBlendMode = BlendMode.Unknown;   // field6_0x154
        var destBlendMode = BlendMode.Unknown;   // field7_0x158

        if (magic >= 2)
        {
            sourceBlendMode = (BlendMode)fs.ReadByte();
            destBlendMode = (BlendMode)fs.ReadByte();
        }

        if (magic > 2)
        {
            globalAlphaMultiplier = fs.ReadFloatLittleEndian();
        }

        if (magic > 3)
        {
            globalEmissiveIntensity = fs.ReadFloatLittleEndian();
        }

        // Стадии материала.
        const float Inv255 = 1.0f / 255.0f;
        const float Field7Mult = 0.01f;
        Span<byte> textureNameBuffer = stackalloc byte[16];
        List<MaterialStage> stages = [];

        for (int i = 0; i < stageCount; i++)
        {
            // Порядок чтения соответствует файлу, а не C struct layout.
            
            // Ambient: 4 байта, A масштабируется на 0.01.
            var ambientR = fs.ReadByte() * Inv255;
            var ambientG = fs.ReadByte() * Inv255;
            var ambientB = fs.ReadByte() * Inv255;
            var ambientA = fs.ReadByte() * Field7Mult; // 0.01 scaling

            // Diffuse: 4 байта.
            var diffuseR = fs.ReadByte() * Inv255;
            var diffuseG = fs.ReadByte() * Inv255;
            var diffuseB = fs.ReadByte() * Inv255;
            var diffuseA = fs.ReadByte() * Inv255;

            // Specular: 4 байта.
            var specularR = fs.ReadByte() * Inv255;
            var specularG = fs.ReadByte() * Inv255;
            var specularB = fs.ReadByte() * Inv255;
            var specularA = fs.ReadByte() * Inv255;

            // Emissive: 4 байта.
            var emissiveR = fs.ReadByte() * Inv255;
            var emissiveG = fs.ReadByte() * Inv255;
            var emissiveB = fs.ReadByte() * Inv255;
            var emissiveA = fs.ReadByte() * Inv255;

            // Power: 1 байт -> float.
            var power = (float)fs.ReadByte();
            
            // Texture stage index: 1 байт.
            var textureStageIndex = fs.ReadByte();

            // Texture name: 16 байт.
            textureNameBuffer.Clear();
            fs.ReadExactly(textureNameBuffer);
            var textureName = Encoding.ASCII.GetString(textureNameBuffer).TrimEnd('\0');

            stages.Add(new MaterialStage(
                ambientR,
                ambientG,
                ambientB,
                ambientA,
                diffuseR,
                diffuseG,
                diffuseB,
                diffuseA,
                specularR,
                specularG,
                specularB,
                specularA,
                emissiveR,
                emissiveG,
                emissiveB,
                emissiveA,
                power,
                textureStageIndex,
                textureName));
        }

        // Анимации.
        List<MaterialAnimation> animations = [];
        for (int i = 0; i < animCount; i++)
        {
            uint typeAndParams = fs.ReadUInt32LittleEndian();
            var target = (AnimationTarget)(typeAndParams >> 3);
            var loopMode = (AnimationLoopMode)(typeAndParams & 7);

            ushort keyCount = fs.ReadUInt16LittleEndian();
            List<AnimKey> keys = [];

            for (int k = 0; k < keyCount; k++)
            {
                keys.Add(new AnimKey(
                    fs.ReadUInt16LittleEndian(),
                    fs.ReadUInt16LittleEndian(),
                    fs.ReadUInt16LittleEndian()));
            }
            
            // Описание вычисляется один раз при парсинге, чтобы UI не выделял память каждый кадр.
            var targetDescription = ComputeTargetDescription(target);
            
            animations.Add(new MaterialAnimation(target, loopMode, keys, targetDescription));
        }

        return new MaterialFile(
            fileName,
            elementCount,
            magic1,
            materialRenderingType,
            supportsBumpMapping,
            isParticleEffect,
            sourceBlendMode,
            destBlendMode,
            globalAlphaMultiplier,
            globalEmissiveIntensity,
            stages,
            animations);
    }
    
    private static string ComputeTargetDescription(AnimationTarget target)
    {
        // Precompute the description once during parsing
        if ((int)target == 0)
            return "Без интерполяции - вся стадия копируется как есть (Flags: 0x0)";
        
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
            
        return $"Интерполируется: {string.Join(", ", parts)} | Остальные компоненты копируются (Flags: 0x{(int)target:X})";
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
