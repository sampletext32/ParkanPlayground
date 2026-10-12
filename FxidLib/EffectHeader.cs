namespace FxidLib;

/// <summary>
/// Заголовок FXID (60 байт, 0x3C), начало каждого файла эффекта. Разбирается в <c>EffectHandle::InitFromFxidData</c> (Effect.dll):
/// определяет число компонентов, длительность, флаги, источник силы и общий масштаб эффекта.
/// </summary>
/// <param name="ComponentCount">+0x00: число компонентов. В файлах от 1 до 27: 1 - 132 файла, 4 - 127, 2 - 67, 5 - 66, 3 - 51.</param>
/// <param name="StrengthSource">+0x04: источник силы эффекта (см. <see cref="EffectStrengthSource"/>).</param>
/// <param name="Duration">+0x08: длительность в секундах; движок считает <c>end_time_ms = Duration * 1000 + start_time_ms</c>.
/// 0.0 в 120 файлах, всегда с источником силы не по жизни (0, 4, 5, 14..17), так как источники по жизни делили бы 0/0.
/// Частые значения: 1.0 - 146 файлов, 1.5 - 56, 3.0 - 51, 2.0 - 18, 12.0 - 14, 0.5 - 14.</param>
/// <param name="StrengthJitter">+0x0C: амплитуда случайного смещения силы; работает только вместе с <see cref="FxidEffectFlags.RandomizeStrength"/>.
/// В файлах: 0 - 408, 0.2 - 36, 0.1 - 31, 0.4 - 1.</param>
/// <param name="Flags">+0x10: флаги поведения эффекта (см. <see cref="FxidEffectFlags"/>).</param>
/// <param name="SettingsId">+0x14: идентификатор настройки качества (<c>EffectDllSettings::GetDust</c> и <c>NextRandLODValue</c>, Effect.dll).
/// Младший байт (<see cref="SettingsCategory"/>, в файлах 0..19) - индекс таблицы из 20 флагов включения; запись 0 значит «эффект не создаётся при текущем уровне детализации».
/// Таблица заполняется <c>SetDefaultValues(preset)</c>: пресеты 1 и 2 включают всё, пресет 3 (минимальная детализация) выключает индексы 0, 1, 6, 7, 10, 11, 12, 16.
/// По порядку строк ресурсов индексы выглядят так: 0 Dust, 1 Smoke, 2 EngineFire, 3 Explode, 4 Shield, 5 Lights, 6 Dust2, 7 Smoke2, 8 Explode2, 9 Shield2,
/// 10 Lights2, 11 Smoke3, 12 EngineFire2, 13 Explode3, 14 GunFire1, 15 Lights3, 16 Smoke4, 17 Explode4, 18 GunFire2, 19 Lights4 (названия - гипотеза по порядку строк).
/// Второй байт (<see cref="SettingsLodClass"/>, 0..3) - класс эмиттеров частиц (типы 7, 8, 10): у каждой частицы случайный порог
/// <c>rand16/65536 * D[класс] − H[класс]</c> (D = 3, 3, 3.5, 5.5 для пресетов 1 и 3 и 1.5, 1.5, 1.75, 2.75 для пресета 2; H = 0, 0, −2, −3), частица
/// рисуется, только если уровень качества рендера (0..3, dword 0x18 контекста) не выше порога: классы 0 и 1 разрежаются с ростом уровня, классы 2 и 3 сохраняют частицы.
/// Старшие два байта во всех файлах равны 0. Категории в данных: 5 - 97 файлов, 10 - 61, 17 - 48, 18 - 42, 1 - 41, 7 - 36, 2 - 34, 16 - 30, 9 - 24, 3 - 18;
/// классы: 0 - 207, 3 - 126, 1 - 124, 2 - 19.</param>
/// <param name="RandShiftX">+0x18: амплитуда случайного смещения позиции по X (с <see cref="FxidEffectFlags.RandomOffset"/>); во всех файлах 0.</param>
/// <param name="RandShiftY">+0x1C: то же по Y; во всех файлах 0.</param>
/// <param name="RandShiftZ">+0x20: то же по Z; во всех файлах 0.</param>
/// <param name="PivotX">+0x24: локальная точка опоры X. В файлах 0 - 313, 0.5 - 92, 1.0 - 71.</param>
/// <param name="PivotY">+0x28: локальная точка опоры Y; во всех файлах 0.</param>
/// <param name="PivotZ">+0x2C: локальная точка опоры Z; во всех файлах 0.</param>
/// <param name="ScaleX">+0x30: базовый масштаб X. В 475 файлах равен по трём осям (22 значения от 0.1 до 50.0): 1.0 - 342 файла, 2.0 - 32, 0.75 - 30, 1.25 - 21, 0.5 - 13, 0.3 - 10, 50.0 - 5; в 1 файле оси различаются.</param>
/// <param name="ScaleY">+0x34: базовый масштаб Y (в 475 файлах равен <see cref="ScaleX"/>).</param>
/// <param name="ScaleZ">+0x38: базовый масштаб Z (в 475 файлах равен <see cref="ScaleX"/>).</param>
public readonly record struct EffectHeader(
    uint ComponentCount,
    EffectStrengthSource StrengthSource,
    float Duration,
    float StrengthJitter,
    FxidEffectFlags Flags,
    uint SettingsId,
    float RandShiftX,
    float RandShiftY,
    float RandShiftZ,
    float PivotX,
    float PivotY,
    float PivotZ,
    float ScaleX,
    float ScaleY,
    float ScaleZ)
{
    /// <summary>Младший байт <see cref="SettingsId"/>: индекс категории эффекта (0..19).</summary>
    public byte SettingsCategory => (byte)(SettingsId & 0xFF);

    /// <summary>Второй байт <see cref="SettingsId"/>: класс LOD частиц (0..3).</summary>
    public byte SettingsLodClass => (byte)((SettingsId >> 8) & 0xFF);
}
