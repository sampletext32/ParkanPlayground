using Common;

namespace MissionTmaLib;

/// <summary>Настройка объекта миссии.</summary>
/// <param name="SettingType">Тип значения: 0 = float, 1 = int, 2 = bool (хранится как int).</param>
/// <param name="Value">Текущее значение.</param>
/// <param name="Min">Минимум.</param>
/// <param name="Max">Максимум.</param>
/// <param name="Name">Имя настройки: Invulnerability, Life state, LogicalID, ClanID, Type, MaxSpeedPercent, MaximumOre, CurrentOre и др.</param>
public record GameObjectSetting(int SettingType, IntFloatValue Value, IntFloatValue Min, IntFloatValue Max, string Name);
