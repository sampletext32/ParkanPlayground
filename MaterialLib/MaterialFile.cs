namespace MaterialLib;

/// <summary>Файл материала MAT0.</summary>
/// <param name="FileName">Имя файла из NRes metadata, не из payload материала.</param>
/// <param name="Version">Значение attr1 / ElementCount из NRes metadata.</param>
/// <param name="Magic1">Значение attr2 / Magic1 из NRes metadata.</param>
/// <param name="BlendMode">Производное от Version: (Version >> 2) &amp; 0xF. Индекс профиля состояний рендера Ngi32 (wanted_device_caps);
/// в данных встречаются только 0, 1, 2.</param>
/// <param name="SupportsBumpMapping">Производное от Version: (Version &amp; 2) != 0.</param>
/// <param name="SurfaceMaterial">Класс поверхности (материал для попаданий/взрывов); читается, если Magic1 >= 2, иначе None (0xFF).
/// В Ghidra World3D.dll поле названо SourceBlendMode по ошибке: значения в данных 0..10 и 255, не D3DBLEND; IMatManager::GetMaterialMetaByIndex
/// возвращает указатель на него, и Control.dll по нему выбирает слот EXPL.</param>
/// <param name="Reserved05">Второй байт после AnimCount; читается, если Magic1 >= 2, иначе 0xFF. Движок записывает его в DestBlendMode, но в данных всегда 255 (все 628 материалов); назначение неизвестно.</param>
/// <param name="GlobalAlphaMultiplier">Глобальный alpha multiplier; читается, если Magic1 > 2. Значение по умолчанию: 1.0.</param>
/// <param name="GlobalEmissiveIntensity">Глобальная emissive intensity; читается, если Magic1 > 3. Значение по умолчанию: 0.0.</param>
/// <param name="Stages">Стадии материала в порядке чтения из файла.</param>
/// <param name="Animations">Анимации материала в порядке чтения из файла.</param>
public record MaterialFile(
    string FileName,
    uint Version,
    int Magic1,
    MaterialBlendMode BlendMode,
    bool SupportsBumpMapping,
    SurfaceMaterial SurfaceMaterial,
    byte Reserved05,
    float GlobalAlphaMultiplier,
    float GlobalEmissiveIntensity,
    List<MaterialStage> Stages,
    List<MaterialAnimation> Animations);

/// <summary>
/// Режим смешивания материала: биты 2..5 версии MAT0 = индекс в таблице профилей состояний рендера Ngi32.dll
/// (<c>g_DefaultRenderStateProfiles</c>, <c>3DRender::SelectRenderStateProfile</c>). Для каждого профиля заданы
/// SRCBLEND, DESTBLEND, ALPHAFUNC, ALPHATESTENABLE, ALPHABLENDENABLE; если устройство режим не поддерживает,
/// <c>CShade::InitAlphaBlendModeTranslateTable</c> подставляет ближайший меньший поддержанный.
/// </summary>
public enum MaterialBlendMode : uint
{
    /// <summary>ONE/ZERO, тест и смешивание выключены: непрозрачные материалы (53 в Material.lib).</summary>
    Opaque = 0,

    /// <summary>SRCALPHA/ZERO, alpha-test ALPHAFUNC = GREATEREQUAL: вырезка по альфе (SKY, EXPL1, SHOOT1, B_PG2..., 184).</summary>
    SrcAlphaZeroAlphaTest = 1,

    /// <summary>SRCALPHA/ONE, alpha-test GREATEREQUAL: аддитивное, взвешенное альфой. Материалы эффектов (JET*, LIGHT1, LASER_R, GLOW_R, 67).</summary>
    Additive = 2,

    /// <summary>ZERO/SRCCOLOR: умножение фона на цвет источника. В данных не встречается.</summary>
    Multiply = 3,

    /// <summary>SRCALPHA/INVSRCALPHA: обычная прозрачность. В данных не встречается.</summary>
    Alpha = 4,

    /// <summary>DESTCOLOR/SRCCOLOR: модуляция x2. В данных не встречается.</summary>
    Modulate2X = 5,
}

/// <summary>
/// Класс поверхности материала (байт 4 элемента MAT0). Числа проверены по данным (Material.lib, 628 материалов) и по кодам слотов EXPL;
/// названия выведены из имён материалов и суффиксов эффектов взрыва. Для None (255) используется слот 0 EXPL.
/// </summary>
public enum SurfaceMaterial : byte
{
    Class0 = 0,   // L08, L09, TOK41_PLC
    Stone = 1,    // "st": ландшафт L*, STONE*, WATER_BOT (37)
    Class2 = 2,   // ландшафт L00/L01/L19/L20/L28/L32/L40
    Class3 = 3,   // L23, L26
    Class4 = 4,   // L18, L27
    Metal = 5,    // "m": R_*, B_* (262)
    Wood = 6,     // "w": деревья, кора (60)
    Water = 7,    // "wb/wl/wm/wt": WATER, WATER_M
    Class8 = 8,   // "al": B_MTP_* (23)
    Class9 = 9,   // "an": BIRD_*, TOK11, TOK42W (12)
    Class10 = 10, // "s": TOK51, B_A_BRIGE
    None = 0xFF,
}

/// <summary>Стадия материала. Порядок полей соответствует порядку чтения из файла, а не C struct layout.</summary>
/// <param name="AmbientR">Ambient R, читается первым цветовым блоком.</param>
/// <param name="AmbientG">Ambient G, читается первым цветовым блоком.</param>
/// <param name="AmbientB">Ambient B, читается первым цветовым блоком.</param>
/// <param name="AmbientA">Ambient A, читается первым цветовым блоком и масштабируется на 0.01.</param>
/// <param name="DiffuseR">Diffuse R, читается вторым цветовым блоком.</param>
/// <param name="DiffuseG">Diffuse G, читается вторым цветовым блоком.</param>
/// <param name="DiffuseB">Diffuse B, читается вторым цветовым блоком.</param>
/// <param name="DiffuseA">Diffuse A, читается вторым цветовым блоком.</param>
/// <param name="SpecularR">Specular R, читается третьим цветовым блоком.</param>
/// <param name="SpecularG">Specular G, читается третьим цветовым блоком.</param>
/// <param name="SpecularB">Specular B, читается третьим цветовым блоком.</param>
/// <param name="SpecularA">Specular A, читается третьим цветовым блоком.</param>
/// <param name="EmissiveR">Emissive R, читается четвертым цветовым блоком.</param>
/// <param name="EmissiveG">Emissive G, читается четвертым цветовым блоком.</param>
/// <param name="EmissiveB">Emissive B, читается четвертым цветовым блоком.</param>
/// <param name="EmissiveA">Emissive A, читается четвертым цветовым блоком.</param>
/// <param name="Power">Power, один байт -> float.</param>
/// <param name="TextureStageIndex">Индекс texture stage. 255 = не задано/default, 0..47 = ссылка на texture stage.</param>
/// <param name="TextureName">Имя текстуры, строка из 16 байт.</param>
public record MaterialStage(
    float AmbientR,
    float AmbientG,
    float AmbientB,
    float AmbientA,
    float DiffuseR,
    float DiffuseG,
    float DiffuseB,
    float DiffuseA,
    float SpecularR,
    float SpecularG,
    float SpecularB,
    float SpecularA,
    float EmissiveR,
    float EmissiveG,
    float EmissiveB,
    float EmissiveA,
    float Power,
    int TextureStageIndex,
    string TextureName);

/// <summary>Анимация материала.</summary>
/// <param name="Target">Целевые компоненты анимации: биты 3..31 combined field.</param>
/// <param name="LoopMode">Режим повтора: биты 0..2 combined field.</param>
/// <param name="Keys">Ключи анимации.</param>
/// <param name="TargetDescription">Кэшированное описание target для UI.</param>
public record MaterialAnimation(
    AnimationTarget Target,
    AnimationLoopMode LoopMode,
    List<AnimKey> Keys,
    string TargetDescription);


[Flags]
public enum AnimationTarget : int
{
    // Это bitset: несколько флагов могут комбинироваться.
    // Установленный флаг означает интерполяцию компонента между стадиями.
    // Неустановленный флаг означает копирование компонента из исходной стадии.
    // Если все флаги равны 0, вся стадия копируется без интерполяции.
    
    Ambient = 1,      // 0x01: интерполирует Ambient RGB.
    Diffuse = 2,      // 0x02: интерполирует Diffuse RGB.
    Specular = 4,     // 0x04: интерполирует Specular RGB.
    Emissive = 8,     // 0x08: интерполирует Emissive RGB.
    Power = 16        // 0x10: интерполирует Ambient.A и задает Power.
}


public enum AnimationLoopMode : int
{
    Loop = 0,
    PingPong = 1,
    Clamp = 2,
    Random = 3
}

/// <summary>Ключ анимации материала (length = 6).</summary>
/// <param name="StageIndex">[0x00..0x02] Индекс стадии материала.</param>
/// <param name="DurationMs">[0x02..0x04] Длительность в миллисекундах.</param>
/// <param name="InterpolationCurve">[0x04..0x06] Кривая интерполяции. В исследованных данных всегда 0.</param>
public readonly record struct AnimKey(ushort StageIndex, ushort DurationMs, ushort InterpolationCurve);
