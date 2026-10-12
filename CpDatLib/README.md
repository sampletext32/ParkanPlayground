# CpDatLib: схемы объектов (`.dat`)

Схема `cp` (`.dat` в `UNITS\\...`) - дерево частей объекта: шасси, башни, орудия, броня, боеприпасы. Из неё игра узнаёт, какие ресурсы (MSH, CPT, NDP, CTL)
загрузить и как присоединить друг к другу. Парсер: `CpDatParser` → `CpDatScheme` (`SchemeType` корня и дерево `CpDatEntry` с типом узла `DatEntryType`:
`Chassis`, `Turret`, `Armour`, `Part`, `Cannon`, `Ammo`).

`SchemeType` кодирует класс объекта в корне (`ClassBuilding 0x80000000`, `ClassRobot 0x01000000`, `ClassAnimal 0x20000000`) и конкретный вид: типы
зданий (`BunkerSmall`, `Generator`, `Mine`, `Storage`, `Plant`, `Hangar`, `TowerMedium`, `TowerLarge`, `MainTeleport`, `Institute`, `Bridge`, `Ruine`) и роботов
(`RobotTransport`, `RobotBuilder`, `RobotBattleunit`, `RobotHq`, `RobotHero`).

Как схемы связаны с записями агентов (откуда берутся имена `INTO`/`EXTO`) - [ControlLib/README.md](../ControlLib/README.md), §4.

## Загрузка схемы в игре

`cp` файл - схема. Он содержит дерево частей объекта.

`cp` файл читается в `ArealMap.dll/CreateObjectFromScheme`

В зависимости от типа объекта внутри схемы (байты 4..8) выбирается функция, с помощью которой загружается схема.

Функция выбирается на основе файла `Comp.ini`.
- Для ClassBuilding (0x80000000) - вызывается функция c классом 3 (по таблице ниже Building).
- Для всех остальных - функция с классом 4 (по таблице ниже Agent).

На основе файла `Comp.ini` и первом вызове внутри функции `World3D.dll/CreateObject` ремаппинг id:



| Logic ID |  ClassName   | Function                       | Description                  |
|:--------:|:------------:|--------------------------------|------------------------------|
|    1     |  Landscape   | `terrain.dll LoadLandscape`    | LOGIC_LANDSCAPE              |
|    2     |    Agent     | `animesh.dll LoadAgent`        | LOGIC_WEAPON_AGENT_WPNS      |
|    3     |   Building   | `terrain.dll LoadBuilding`     | LOGIC_BUILDING_HALLWAY       |
|    4     |    Agent     | `animesh.dll LoadAgent`        | LOGIC_BATTLE_UNIT_AGENT_BTLU |
|    5     |    Camera    | `terrain.dll LoadCamera`       | LOGIC_CAMERA                 |
|    7     |  Atmosphere  | `terrain.dll CreateAtmosphere` | LOGIC_ATMOSPHERE             |
|    9     |    Agent     | `animesh.dll LoadAgent`        | LOGIC_BULLET_AGENT_BULL      |
|   0xa    |    Agent     | `animesh.dll LoadAgent`        | LOGIC_STATIC_AGENT_STAT      |
|   0xb    |   Research   | `misload.dll LoadResearch`     | LOGIC_RESEARCH               |
|   0xc    |    Agent     | `animesh.dll LoadAgent`        | LOGIC_AGENT_0xC              |

Всем этим функциям передаётся `nres_file_name, nres_entry_name, 0, player_index`

## `fr FORT` файл

Всегда 0x80 байт
Содержит 2 ссылки на файлы:
- `.bas`
- `.ctl` - вызывается `LoadAgent`
