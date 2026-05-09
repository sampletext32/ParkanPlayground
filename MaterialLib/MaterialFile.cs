namespace MaterialLib;

/// <summary>Файл материала MAT0.</summary>
/// <param name="FileName">Имя файла из NRes metadata, не из payload материала.</param>
/// <param name="Version">Значение attr1 / ElementCount из NRes metadata.</param>
/// <param name="Magic1">Значение attr2 / Magic1 из NRes metadata.</param>
/// <param name="MaterialRenderingType">Производное от Version: (Version >> 2) &amp; 0xF. 0 = standard, 1 = special, 2 = particle.</param>
/// <param name="SupportsBumpMapping">Производное от Version: (Version &amp; 2) != 0.</param>
/// <param name="IsParticleEffect">Производное от Version: Version &amp; 40. 0 = normal, 8 = particle/effect.</param>
/// <param name="SourceBlendMode">Глобальный source blend mode; читается, если Magic1 >= 2. Значение по умолчанию: Unknown (0xFF).</param>
/// <param name="DestBlendMode">Глобальный destination blend mode; читается, если Magic1 >= 2. Значение по умолчанию: Unknown (0xFF).</param>
/// <param name="GlobalAlphaMultiplier">Глобальный alpha multiplier; читается, если Magic1 > 2. Значение по умолчанию: 1.0.</param>
/// <param name="GlobalEmissiveIntensity">Глобальная emissive intensity; читается, если Magic1 > 3. Значение по умолчанию: 0.0.</param>
/// <param name="Stages">Стадии материала в порядке чтения из файла.</param>
/// <param name="Animations">Анимации материала в порядке чтения из файла.</param>
public record MaterialFile(
    string FileName,
    int Version,
    int Magic1,
    int MaterialRenderingType,
    bool SupportsBumpMapping,
    int IsParticleEffect,
    BlendMode SourceBlendMode,
    BlendMode DestBlendMode,
    float GlobalAlphaMultiplier,
    float GlobalEmissiveIntensity,
    List<MaterialStage> Stages,
    List<MaterialAnimation> Animations);

/// <summary>
/// Режимы blend для материала. Управляют смешиванием source и destination цветов.
/// Формула: FinalColor = SourceColor * SourceBlend [operation] DestColor * DestBlend.
/// Соответствуют значениям Direct3D D3DBLEND.
/// </summary>
public enum BlendMode : byte
{
    /// <summary>Blend factor = (0, 0, 0, 0).</summary>
    Zero = 1,
    
    /// <summary>Blend factor = (1, 1, 1, 1).</summary>
    One = 2,
    
    /// <summary>Blend factor = (Rs, Gs, Bs, As).</summary>
    SrcColor = 3,
    
    /// <summary>Blend factor = (1-Rs, 1-Gs, 1-Bs, 1-As).</summary>
    InvSrcColor = 4,
    
    /// <summary>Blend factor = (As, As, As, As).</summary>
    SrcAlpha = 5,
    
    /// <summary>Blend factor = (1-As, 1-As, 1-As, 1-As).</summary>
    InvSrcAlpha = 6,
    
    /// <summary>Blend factor = (Ad, Ad, Ad, Ad).</summary>
    DestAlpha = 7,
    
    /// <summary>Blend factor = (1-Ad, 1-Ad, 1-Ad, 1-Ad).</summary>
    InvDestAlpha = 8,
    
    /// <summary>Blend factor = (Rd, Gd, Bd, Ad).</summary>
    DestColor = 9,
    
    /// <summary>Blend factor = (1-Rd, 1-Gd, 1-Bd, 1-Ad).</summary>
    InvDestColor = 10,
    
    /// <summary>Blend factor = (f, f, f, 1), где f = min(As, 1-Ad).</summary>
    SrcAlphaSat = 11,
    
    /// <summary>Blend factor = (As, As, As, As) для source и destination; устарело в D3D9+.</summary>
    BothSrcAlpha = 12,
    
    /// <summary>Blend factor = (1-As, 1-As, 1-As, 1-As) для source и destination; устарело в D3D9+.</summary>
    BothInvSrcAlpha = 13,
    
    /// <summary>Неизвестный или неинициализированный режим blend; значение по умолчанию 0xFF.</summary>
    Unknown = 0xFF
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
