using Common;

namespace ControlLib;

/// <summary>
/// Команда списка CTL (0x64 байта): условие, опкод, пять аргументов и ссылка на ресурс. Значение аргументов зависит от опкода (int или float);
/// эффект адресуется 32-битным идентификатором <c>(msh_tag &lt;&lt; 16) | local_id</c>, у команд порождения <c>local_id = arg3</c>.
/// </summary>
/// <param name="Offset">Смещение записи от начала файла (для отладки).</param>
/// <param name="Condition">+0x00: условие выполнения команды (см. <see cref="ControlCommandConditionRaw"/>).</param>
/// <param name="Opcode">+0x0C: код команды.</param>
/// <param name="Arg0">+0x10: аргумент 0 (у SPAWN_*: индекс детали/CPT; у START_/REMOVE_EFFECT: local_id эффекта).</param>
/// <param name="Arg1">+0x14: аргумент 1 (у START_EFFECT: источник силы <c>EffectStrengthSource</c>; у ATTACH_EFFECT_TO_PIECE: деталь).</param>
/// <param name="Arg2">+0x18: аргумент 2 (у команды 4: третий индекс CPT).</param>
/// <param name="Arg3">+0x1C: аргумент 3 (у SPAWN_*: local_id эффекта; 0xFFFFFFFF - «следующий из счётчика объекта», в данных не встречается).</param>
/// <param name="Arg4">+0x20: аргумент 4. У команды 4 игнорируется: в данных 0 у 806 из 937 записей, остальное - разнородное с ASCII-мусором из неочищенного буфера.</param>
/// <param name="Resource">+0x24: ресурс команды (FXID эффекта в <c>effects.rlb</c>, EXPL в <c>weapon.rlb</c>). Непуст у 552 команд: <c>effects.rlb</c> - 507, <c>weapon.rlb</c> - 45.</param>
public sealed record ControlCommandRecordRaw(
    long Offset,
    ControlCommandConditionRaw Condition,
    EControlCommandOpcode Opcode,
    uint Arg0,
    uint Arg1,
    uint Arg2,
    uint Arg3,
    uint Arg4,
    ResourceNamedRef Resource);
