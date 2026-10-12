using Common;

namespace ControlLib;

/// <summary>
/// Запись NDP (76 байт): вклад детали в жизнь и подвижность и ресурс, играющий при её разрушении. Игра хранит индекс ресурса в кэше
/// <c>g_ndp_damage_effect_resource_cache</c> (<c>ControlNdpRef.damage_effect_resource_index</c>); рабочий флаг записи 0x10 = DEPLETED (деталь разрушена),
/// такие записи пропускают функции подсчёта радиуса взрыва.
/// </summary>
/// <param name="Offset">Смещение записи от начала файла (для отладки).</param>
/// <param name="Unknown0">+0x00: назначение не выяснено. В данных: 0 - 1029 записей, 1 - 68, 112 - 48.</param>
/// <param name="MaxVolume">+0x04: максимальный объём детали (запас жизни); жизнь детали пропорциональна объёму.
/// В данных от 1 до 1e9 (очень большие значения у неразрушимых деталей); нулей нет.</param>
/// <param name="MobilityContributionPerVolume">+0x08: вклад детали в подвижность на единицу объёма (идёт в <c>mobility_integrity_amount</c> и сигнатуру 0x45).
/// В данных 0..50000, ноль у 207 записей.</param>
/// <param name="Resource">+0x0C: ресурс взрыва детали (EXPL, <c>explode_*.exp</c>). Пуст у 131 записи; архивы: <c>system.rlb</c> - 865, <c>static.rlb</c> - 93, <c>weapon.rlb</c> - 56;
/// все непустые - файлы <c>.exp</c> (1014).</param>
public sealed record NdpRecordRaw(
    long Offset,
    uint Unknown0,
    float MaxVolume,
    float MobilityContributionPerVolume,
    ResourceNamedRef Resource);
