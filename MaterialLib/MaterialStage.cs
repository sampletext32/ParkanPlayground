namespace MaterialLib;

/// <summary>
/// Стадия материала: набор цветов и текстура. Порядок полей соответствует порядку чтения из файла (34 байта на стадию), а не раскладке C-структуры.
/// Цветовые компоненты хранятся байтом и переводятся в float делением на 255, альфа ambient - умножением на 0.01.
/// </summary>
/// <param name="AmbientR">Ambient R (0..1).</param>
/// <param name="AmbientG">Ambient G (0..1).</param>
/// <param name="AmbientB">Ambient B (0..1).</param>
/// <param name="AmbientA">Ambient A: байт × 0.01. У 1806 стадий из 1811 равно 1.0 (байт 100), у остальных 0, 0.6, 0.8, 0.9, 0.95.
/// Единственная альфа, которую игра интерполирует (бит <see cref="AnimationTarget.AmbientAlpha"/>).</param>
/// <param name="DiffuseR">Diffuse R (0..1).</param>
/// <param name="DiffuseG">Diffuse G (0..1).</param>
/// <param name="DiffuseB">Diffuse B (0..1).</param>
/// <param name="DiffuseA">Diffuse A (0..1). Во всех 1811 стадиях 0.</param>
/// <param name="SpecularR">Specular R (0..1).</param>
/// <param name="SpecularG">Specular G (0..1).</param>
/// <param name="SpecularB">Specular B (0..1).</param>
/// <param name="SpecularA">Specular A (0..1). Во всех 1811 стадиях 0.</param>
/// <param name="EmissiveR">Emissive R (0..1).</param>
/// <param name="EmissiveG">Emissive G (0..1).</param>
/// <param name="EmissiveB">Emissive B (0..1).</param>
/// <param name="EmissiveA">Emissive A (0..1). Во всех 1811 стадиях 0.</param>
/// <param name="Power">Блеск specular (байт, как есть). В данных: 0 - 1414 стадий, 3 - 256, 4 - 93, 6 - 28, 5 - 20. Анимация его <b>никогда</b> не интерполирует.</param>
/// <param name="TextureStageIndex">Номер страницы в атласе TEXM (<c>PageHeader</c> текстуры) или 255, если страниц нет.
/// В данных: 255 - у 477 стадий, остальные 0..55 и 63.</param>
/// <param name="TextureName">Имя текстуры в <c>Textures.lib</c> (до 15 символов с суффиксом палитры <c>.0</c>); пусто - стадия без текстуры.
/// В данных 288 различных имён, пустое имя у 7 стадий; суффикс <c>.0</c> у всех, поэтому палитры из <c>Palettes.lib</c> не используются.</param>
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
