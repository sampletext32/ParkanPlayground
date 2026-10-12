# MissionTmaLib: файл миссии (`.tma`)

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
| 2 | список записей `UnknownClanTreeInfoPart`: число, вектор из трёх float и два float (смысл не выяснен) |
| 3 | `ResearchNResPath` - дерево исследований (`auto.trf`, `data.trf`; может быть пустым); см. [ResTreeLib](../ResTreeLib/README.md) |
| 4 | `Brains` - число «мозгов» ИИ |
| 5 | `AlliesMap` - таблица союзников: имя клана → 0 или 1 (версии 7 и выше парсер не принимает) |

Тип клана (`ClanType`): `Environment` 0, `Player` 1, `AI` 2, `Neutral` 3. Тип объекта (`GameObjectType`): `Building` 0, `Warbot` 1, `Tree` 2, `Stone` 3.
`GameObjectInfo.OwningClanIndex` равен −1 у объектов без владельца (у части объектов окружения он бывает отличен от −1).

Просмотр: панель `Mission TMA Explorer` в NResUI (`File` → `Open Mission TMA`).
