namespace ControlLib;

/// <summary>
/// Код команды списка CTL (<c>EControlCommandOpcode</c> в Ghidra). Выполняется <c>CControlBase::ExecuteControlCommandList</c> (Control.dll 0x100028A0).
/// Статистика по 1271 команде в 449 файлах CTL первой части: «N команд / M файлов»; опкоды без отметки в данных не встречаются.
/// В перечислении нет значений 6, 0x10, 0x1A, 0x1C и 0x24, в данных они не встречаются.
/// </summary>
public enum EControlCommandOpcode : uint
{
    /// <summary>Сбросить команды движения (<c>RESET_MOTION_COMMANDS</c>). 152 команды / 54 файла.</summary>
    CONTROL_CMD_RESET_MOTION_COMMANDS = 0,

    /// <summary>Пометить корень «центральным». 41 команда / 41 файл.</summary>
    CONTROL_CMD_MARK_ROOT_CENTRAL = 1,

    /// <summary>Снять пометку «центральный» с корня. В данных не встречается.</summary>
    CONTROL_CMD_UNMARK_ROOT_CENTRAL = 2,

    /// <summary>Породить эффект по границам детали: <c>arg0</c> - деталь от первой детали msh_tag, <c>arg3</c> - local_id; масштаб = радиус сферы детали, позиция = её центр (SPACE_LOCAL). 85 команд / 48 файлов.</summary>
    CONTROL_CMD_SPAWN_EFFECT_ON_JOINT_BOUNDS = 3,

    /// <summary>Породить эффект по трём контрольным точкам: <c>arg0..2</c> - индексы CPT от первой CPT msh_tag, <c>arg3</c> - local_id; матрица эффекта строится из трёх CPT
    /// (см. README, §7.4). Самая частая команда: 418 команд / 197 файлов.</summary>
    CONTROL_CMD_SPAWN_ORIENTED_EFFECT_FROM_CPTS = 4,

    /// <summary>Породить эффект по границам всего объекта: <c>arg3</c> - local_id (в данных 9000); радиус и центр сферы объекта (SPACE_WORLD). 4 команды / 4 файла.</summary>
    CONTROL_CMD_SPAWN_EFFECT_ON_OBJECT_BOUNDS = 5,

    /// <summary>Уничтожить деталь. В данных не встречается.</summary>
    CONTROL_CMD_KILL_PIECE = 7,

    /// <summary>Удалить эффект: <c>arg0</c> - local_id. 22 команды / 9 файлов.</summary>
    CONTROL_CMD_REMOVE_EFFECT = 8,

    /// <summary>Загрузить ресурс предмета. В данных не встречается.</summary>
    CONTROL_CMD_ITEM_LOAD_RESOURCE = 9,

    /// <summary>Запустить эффект: <c>arg0</c> - local_id, <c>arg1</c> - источник силы (<c>EffectStrengthSource</c>: Life_0to1 ×147, Constant ×30, Life_1minus ×4, Life_Fract ×3). 184 команды / 64 файла.</summary>
    CONTROL_CMD_START_EFFECT = 0xA,

    /// <summary>Запросить остановку эффекта: он доигрывает цикл и перезапускается с источником из файла (<c>arg0</c> - local_id). 26 команд / 10 файлов.</summary>
    CONTROL_CMD_REQUEST_STOP_EFFECT = 0xB,

    /// <summary>Задать тангенциальное ускорение. 14 команд / 13 файлов.</summary>
    CONTROL_CMD_SET_TANG_ACCEL = 0xC,

    /// <summary>Задать нормальную скорость. В данных не встречается.</summary>
    CONTROL_CMD_SET_NORM_SPEED = 0xD,

    /// <summary>Привязать эффект к детали: <c>arg0</c> - local_id, <c>arg1</c> - деталь; по её анимации считаются источники силы AnimParam/AnimMinClamp/AnimMaxClamp. 2 команды / 1 файл.</summary>
    CONTROL_CMD_ATTACH_EFFECT_TO_PIECE = 0xE,

    /// <summary>Запланировать удаление объекта: ставит <c>clan_index = 0xFFFE</c>, затем удаляет через <c>DetachCleanupDelayMs</c>. 56 команд / 53 файла.</summary>
    CONTROL_CMD_SCHEDULE_DETACH_OR_KILL = 0xF,

    /// <summary>Принудительно уничтожить объект, игнорируя неуязвимость. 50 команд / 50 файлов.</summary>
    CONTROL_CMD_FORCE_KILL_IGNORE_INVULNERABILITY = 0x11,

    /// <summary>Включить эффект (<c>arg0</c> - local_id). 33 команды / 25 файлов.</summary>
    CONTROL_CMD_ACTIVATE_EFFECT = 0x12,

    /// <summary>Выключить эффект (<c>arg0</c> - local_id). 66 команд / 36 файлов.</summary>
    CONTROL_CMD_DEACTIVATE_EFFECT = 0x13,

    /// <summary>Поставить здание на землю. В данных не встречается.</summary>
    CONTROL_CMD_PLACE_BUILDING_ON_TERRAIN = 0x14,

    /// <summary>Нанести урон объектам в границах. В данных не встречается.</summary>
    CONTROL_CMD_DAMAGE_OBJECTS_IN_BOUNDS = 0x15,

    /// <summary>Породить объект по трём контрольным точкам. В данных не встречается.</summary>
    CONTROL_CMD_SPAWN_OBJECT_FROM_3_CPTS = 0x16,

    /// <summary>Выполнить действие предмета (вариант «ложь»). В данных не встречается.</summary>
    CONTROL_CMD_EXECUTE_ITEM_ACTION_FALSE = 0x17,

    /// <summary>Установить флаг предмета 0x100. В данных не встречается.</summary>
    CONTROL_CMD_SET_ITEM_FLAG_0x100 = 0x18,

    /// <summary>Снять флаг предмета 0x100. В данных не встречается.</summary>
    CONTROL_CMD_CLEAR_ITEM_FLAG_0x100 = 0x19,

    /// <summary>Подменить ресурс EXPL у NDP и немедленно взорвать (см. README, §7.5). 45 команд / 45 файлов.</summary>
    CONTROL_CMD_SWAP_NDP_RESOURCE = 0x1B,

    /// <summary>Взять клан родителя (<c>clan_index</c> через <c>ILifeSystem</c> слот 0x2C). 4 команды / 4 файла: <c>ws_al_01</c>, <c>ws_al_02</c>, <c>ws_hm_01</c>, <c>ws_hm_02</c>.</summary>
    CONTROL_CMD_INHERIT_CLAN_FROM_PARENT = 0x1D,

    /// <summary>Выполнить действие предмета (вариант «истина»). 8 команд / 4 файла.</summary>
    CONTROL_CMD_EXECUTE_ITEM_ACTION_TRUE = 0x1E,

    /// <summary>Задать преобразование объекта. 20 команд / 4 файла.</summary>
    CONTROL_CMD_SET_OBJECT_TRANSFORMATION = 0x1F,

    /// <summary>Выполнить команду предмета, режим 0. 4 команды / 4 файла.</summary>
    CONTROL_CMD_EXECUTE_ITEM_COMMAND_MODE0 = 0x20,

    /// <summary>Выполнить команду предмета, режим 1. 4 команды / 4 файла.</summary>
    CONTROL_CMD_EXECUTE_ITEM_COMMAND_MODE1 = 0x21,

    /// <summary>Установить флаг предмета 0x200. 4 команды / 4 файла.</summary>
    CONTROL_CMD_SET_ITEM_FLAG_0x200 = 0x22,

    /// <summary>Распределить урон по частям. 4 команды / 4 файла.</summary>
    CONTROL_CMD_DISTRIBUTE_DAMAGE = 0x23,

    /// <summary>Задать целевую точку предмета: точка = позиция объекта + (<c>arg1..arg3</c>) · 0.001. 2 команды / 1 файл.</summary>
    CONTROL_CMD_SET_ITEM_TARGET_POINT = 0x25,

    /// <summary>Задать флаги предмета: <c>arg0</c> - предмет, <c>arg1</c> - флаги. 2 команды / 1 файл.</summary>
    CONTROL_CMD_SET_ITEM_FLAGS = 0x26,

    /// <summary>Циклически сменить преобразование объекта: <c>(текущее + arg0) % arg1</c>. 1 команда / 1 файл (<c>ws_fl_01</c>, слот хука 8).</summary>
    CONTROL_CMD_CYCLE_OBJECT_TRANSFORMATION = 0x27,

    /// <summary>Поставить в очередь путь к состоянию управления. В данных не встречается.</summary>
    CONTROL_CMD_QUEUE_PATH_TO_CONTROL_STATE = 0x28,

    /// <summary>Поставить в очередь путь к случайному состоянию управления. В данных не встречается.</summary>
    CONTROL_CMD_QUEUE_PATH_TO_RANDOM_CONTROL_STATE = 0x29,

    /// <summary>Установить сетевой флаг синхронизации 0x10000000. В данных не встречается.</summary>
    CONTROL_CMD_SET_NET_SYNC_FLAG_0x10000000 = 0x2A,

    /// <summary>Снять сетевой флаг синхронизации 0x10000000. В данных не встречается.</summary>
    CONTROL_CMD_CLEAR_NET_SYNC_FLAG_0x10000000 = 0x2B,

    /// <summary>Не выяснено. В данных не встречается.</summary>
    CONTROL_CMD_UNKNOWN_0x2c = 0x2C,

    /// <summary>Заполнить батареи (<c>ITEM_PARAM_ITEM_FACTOR</c> = 1.0 у всех батарей 0x13). 4 команды / 4 файла.</summary>
    CONTROL_CMD_FILL_BATTERIES = 0x2D,

    /// <summary>Пустая команда. 12 команд / 4 файла.</summary>
    CONTROL_CMD_NOOP_0x2e = 0x2E,

    /// <summary>Задать режим сетевой синхронизации <c>NetSynchroMode</c>. 4 команды / 4 файла.</summary>
    CONTROL_CMD_SET_NET_SYNCHRO_MODE = 0x2F,
}
