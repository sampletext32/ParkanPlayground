# MissionTmaLib: файл миссии (`.tma`)

![Mission TMA Explorer: ареалы, кланы и объекты миссии `data.tma`](../docs/images/mission-tma.png)
*Mission TMA Explorer: ареалы, кланы и объекты миссии `data.tma`*


`.tma` (например `data.tma` каталога миссии) содержит данные миссии: ареалы, кланы и объекты. Парсер: `MissionTmaParser.ReadFile` → `MissionTmaParseResult`
(`MissionTma` + сообщение об ошибке). Игра читает файл в `MisLoad.dll`; порядок загрузки миссии описан в корневом [README.md](../README.md) (раздел «Как работает игра»).

## Три блока

| Блок | Тип | Что внутри |
|---|---|---|
| ареалы | `ArealsFileData` / `ArealInfo` | список областей карты; в демо и первой миссии кампании ареалов нет |
| кланы | `ClansFileData` / `ClanInfo` | набор версий (`ClanFeatureSet`) и записи кланов |
| объекты | `GameObjectsFileData` / `GameObjectInfo` | расстановка зданий, роботов, деревьев, камней; у объектов список настроек (`GameObjectSettings`) |

Версия записи клана (`ClanFeatureSet`) определяет, какие поля присутствуют:

| `ClanFeatureSet` больше | Поле клана |
|---|---|
| 1 | `ScriptsString` - скрипт ИИ клана без расширения (`MISSIONS\SCRIPTS\default`, `tut1_pl`, `tut1_en`); см. [ScrLib/README.md](../ScrLib/README.md) |
| 2 | `MigrationPlaces` - места миграции: резерв (int), позиция (3 float), радиус, базовый радиус подхода; из них `ISystemArealMap.CreateMigrationArealsForClan` строит ареалы миграции, их использует AI (`MigrateTask`) |
| 3 | `ResearchNResPath` - дерево исследований (`auto.trf`, `data.trf`; может быть пустым); см. [ResTreeLib](../ResTreeLib/README.md) |
| 4 | `Brains` - число «мозгов» ИИ |
| 5 | `AlliesMap` - отношения: имя клана → 0 (враг) или 1 (союзник); версии 7 и выше парсер не принимает |

Тип клана (`ClanType`): `Environment` 0, `Player` 1, `AI` 2, `Neutral` 3. Тип объекта (`GameObjectType`): `Building` 0, `Warbot` 1, `Tree` 2, `Stone` 3.
`GameObjectInfo.OwningClanIndex` равен −1 у объектов без владельца (у части объектов окружения он бывает отличен от −1).

Просмотр: панель `Mission TMA Explorer` в NResUI (`File` → `Open Mission TMA`).

## Подробности по данным (проверено на 29 миссиях: 796 объектов, 94 клана, 31 залежь)

- Отношения кланов игра собирает в матрицу N×N: 0 - враг, 1 - союзник, 2 - свой клан; кланы, которых нет в `AlliesMap`, считаются как 1.
- `ClanInfo.Reserved` игра читает, но в структуру кладёт -1. `X`/`Y` - позиция клана.
- Объект (`GameObjectInfo`): `Flags`; `ShortName` - короткое имя (схема), у героя пустое; `HostBuildingLogicId` и `HostSlotIndex` - LogicalID здания-хозяина
  и номер слота в нём (для объектов, привязанных к зданию); `UnknownInt4` принимает 0 или 1, смысл не выяснен.
- Настройка (`GameObjectSetting`): тип 0 float, 1 int, 2 bool (хранится как int); три значения - текущее, минимум, максимум; затем имя
  (`Invulnerability`, `Life state`, `LogicalID`, `ClanID`, `Type`, `MaxSpeedPercent`, `MaximumOre`, `CurrentOre`).
- Залежь (`LodeInfo`): позиция; поле +0x0C - 0 или 1 (AI читает как bool); поле +0x10 из файла не читается; float +0x14 и int +0x18 - не выяснено.

В Ghidra (`MisLoad.dll`) названы структуры `ClanInfo`, `MigrationPlace`, `GameObjectFileData`, `LodeInfo` и вспомогательные функции.
