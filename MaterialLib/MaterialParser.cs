using Common;

namespace MaterialLib;

/// <summary>
/// Разбор содержимого материала MAT0 (<c>MatManager::LoadMaterialByName</c>, World3D.dll). Метаданные NRes
/// (версия и <c>Magic1</c>) в содержимом отсутствуют и передаются отдельно.
/// </summary>
public static class MaterialParser
{
    /// <summary>Читает материал из потока, установленного на начало содержимого элемента.</summary>
    /// <param name="fs">Поток с содержимым материала.</param>
    /// <param name="fileName">Имя материала из метаданных NRes.</param>
    /// <param name="elementCount">attr1 метаданных NRes (<see cref="MaterialFile.Version"/>).</param>
    /// <param name="magic1">attr2 метаданных NRes (<see cref="MaterialFile.Magic1"/>).</param>
    public static MaterialFile ReadFromStream(Stream fs, string fileName, uint elementCount, int magic1)
    {
        var blendMode = (MaterialBlendMode)(elementCount >> 2 & 0xf);
        var supportsBumpMapping = (elementCount & 2) != 0;

        // Чтение содержимого.
        var stageCount = fs.ReadUInt16LittleEndian();
        var animCount = fs.ReadUInt16LittleEndian();

        uint magic = (uint)magic1;

        // Значения по умолчанию из C-кода.
        var surfaceSpeedScale = 1.0f; // field8_0x15c: множитель скорости юнита на поверхности
        var surfaceLifeDrainPerSec = 0.0f; // field9_0x160: потеря жизни в секунду
        var surfaceMaterial = SurfaceMaterial.None;   // field6_0x154 (в Ghidra SourceBlendMode)
        byte reserved05 = 0xFF;                       // field7_0x158 (в Ghidra DestBlendMode)

        if (magic >= 2)
        {
            surfaceMaterial = (SurfaceMaterial)fs.ReadByte();
            reserved05 = (byte)fs.ReadByte();
        }

        if (magic > 2)
        {
            surfaceSpeedScale = fs.ReadFloatLittleEndian();
        }

        if (magic > 3)
        {
            surfaceLifeDrainPerSec = fs.ReadFloatLittleEndian();
        }

        // Стадии материала.
        const float Inv255 = 1.0f / 255.0f;
        const float Field7Mult = 0.01f;
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

            // Texture name: 16 байт, ASCII до первого нуля.
            var textureName = fs.ReadFixedString(16);

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
            blendMode,
            supportsBumpMapping,
            surfaceMaterial,
            reserved05,
            surfaceSpeedScale,
            surfaceLifeDrainPerSec,
            stages,
            animations);
    }
    
    /// <summary>Описание маски интерполяции для интерфейса.</summary>
    private static string ComputeTargetDescription(AnimationTarget target)
    {
        // Описание вычисляется один раз при разборе.
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
            parts.Add("Ambient.A");
            
        return $"Интерполируется: {string.Join(", ", parts)} | Остальные компоненты копируются (Flags: 0x{(int)target:X})";
    }
}
