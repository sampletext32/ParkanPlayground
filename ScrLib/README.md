# ScrLib: скрипты миссий (`.scr`, `.fml`, `varset.var`)

![SCR Explorer: секции скрипта ИИ, инструкции и названия встроенных функций](../docs/images/scr-explorer.png)
*SCR Explorer: секции скрипта ИИ, инструкции и названия встроенных функций*


Стратегический ИИ клана («SuperAI», библиотека `ai.dll`) управляется скриптами из папки `MISSIONS/SCRIPTS`. Один скрипт состоит из трёх файлов
с одним именем:

| Файл | Что хранит | Парсер |
|---|---|---|
| `NAME.scr` | бинарный файл: секции-обработчики событий и проблем, в них - инструкции | `ScrLib.ScrParser` |
| `NAME.fml` | текстовые формулы: по одной на вызов «формулы» из `.scr` | не разбирается (читается как текст) |
| `varset.var` | текстовый набор переменных и констант, общий для всех скриптов | `VarsetLib.VarsetParser` |

В игре 50 пар `.scr` + `.fml`, один `varset.var`; кроме них там же лежат 49 файлов `.trf` (дерево исследований, см. `ResTreeLib`).
Скрипт назначается клану в файле миссии `.tma`: строка `ScriptsString` записи клана (`MissionTmaLib.ClanInfo`, читается при версии клана больше 1)
хранит путь без расширения, например `MISSIONS\SCRIPTS\default`, `tut1_pl`, `tut1_en`; если имя не задано, `SuperAI::ctor` берёт `default`.
Дерево исследований клана задаёт соседняя строка `ResearchNResPath` (`auto.trf`, `data.trf`).

Статус утверждений в этом документе: всё ниже получено по декомпилированному коду `ai.dll` (`SuperAI::ctor`, `AIActions::*`) и проверено на 50 файлах;
поведение в самой игре не наблюдалось. Названия встроенных функций - наши, в игре строковых имён у них нет.

## 1. Секции (обработчики)

`.scr` начинается с `magic` (59, число известных игре встроенных функций) и числа секций. Секция - именованный список инструкций.
Имя секции - это название события или проблемы, по которому её находит игра (`AIActions::FindEntryIndex`).

| Секция | Файлов | Когда выполняется |
|---|---|---|
| `Init` | 50 | при создании клана (`is_Init_executed`) |
| `Mission` | 50 | главный обработчик миссии; `SuperAI::ctor` запоминает его индекс как `Mission_index` |
| `Problems0` | 50 | выбор проблем клана (`Problems0_index`) |
| `Fort_Task_Complete`, `Fort_Captured` | 50 | завершение задания / захват укрепления (`Fort_Task_Complete` обязательна: без неё игра завершает работу с ошибкой скрипта) |
| `Mech_GeneratorFound`, `Mech_Mineral_Found`, `Mech_Task_Complete` | 50 | события юнитов: найден генератор, найдена руда, задание выполнено (`Mech_GeneratorFound` обязательна) |
| `Hero_Teleported` | 50 | герой телепортирован |
| `PBM_*_Start`, `PBM_*_Continue` | 11-16 | начало и продолжение решения проблемы (например `PBM_BUILDING_INF_CAPTURE`, `PBM_BUILDING_PROTECT`, `PBM_ROBOT_NEEDED`) |

Всего в 50 файлах 637 секций и 5767 инструкций, от 9 до 39 секций в файле. Если версия скриптов не совпадает с таблицей игры
(число встроенных функций не 59), игра сообщает `Scripts are not up to date`.

## 2. Инструкции

Секция при каждом вызове выполняется заново с первой инструкции до конца, до `Return` или до перехода. Условия образуют стек: каждая инструкция
`Compare` увеличивает глубину, её результат равен «сравнение И результат на предыдущей глубине»; `EndIf` уменьшает глубину. Инструкции
`Execute`, `GotoInstruction`, `GotoEntry` и `Return` выполняются только на нулевой глубине или при истинном верхнем условии.

| Тип | Имя (`ScrEntryInnerType`) | Смысл |
|---|---|---|
| −1 | `Execute` | вызвать встроенную функцию `action_N` (номер `ScriptIndex`), либо скопировать переменную (`UnkInner3`), либо вычислить формулу из `.fml` (`UnkInner7`); результат - в переменную `UnkInner2` |
| 0 | `Compare` | сравнить две переменные (аргументы); операция в `UnkInner5`: 0 `<`, 1 `==`, 2 `>`, 3 `<=`, 4 `>=`, 5 `!=`; `DWORD` сравнивается как целое, остальное как float |
| 1 | `EndIf` | закрыть условие (глубина −1) |
| 2 | `Nop` | ничего (9 раз в данных) |
| 3 | `GotoInstruction` | перейти на инструкцию `args[0]` (с 1) в этой секции |
| 4 | `GotoEntry` | перейти к началу секции `args[0]` |
| 5 | `Return` | завершить секцию на этом вызове |
| 6 | `SetVarsetValue` | `var[UnkInner2] = UnkInner3` |

Распределение в данных (5767 инструкций): `Execute` 3952 (1877 встроенных, 1975 формул, 100 копирований), `Compare` 680, `EndIf` 682, `Return` 266,
`SetVarsetValue` 153, `GotoEntry` 16, `GotoInstruction` 9, `Nop` 9. У `Compare` всегда два аргумента, у `Goto*` номера всегда в пределах секции или файла.

## 3. Формулы `.fml`

Текстовый файл: первая строка `//FormulaSet export file`, далее по одной формуле на строку:

```
FUNCTION( , 5000 + 2000*fHazard,  )
FUNCTION( , dTemp3 + 0.5*dTemp3*fDifficulty,  )
FUNCTION( , dBuildingProtectHits - dT2,  )
```

Первое и третье поле (имя и комментарий) во всех 1975 формулах пусты; второе - арифметическое выражение над переменными из `varset.var`
(операции: сложение, вычитание, умножение, деление, степень, смена знака, нормирование, взятие знака, приведение к булеву, модуль). Индекс формулы -
это номер строки, на который ссылается инструкция `Execute` (`UnkInner7`). Число формул в `.fml` совпадает с числом инструкций-формул в `.scr`
(в сумме по 50 файлам 1975).

## 4. Переменные `varset.var`

Текстовый файл, строки вида `VAR( тип, имя, значение по умолчанию [, минимум, максимум, комментарий])` (также `STRING( размер, имя, значение, комментарий)`).
Типы: `float`, `DWORD`, `BOOL`, `STRING`. Индекс переменной = номер её объявления; инструкции `.scr` ссылаются на переменные по индексу
(в данных 220 переменных). В файле есть:

- **Рабочие переменные скрипта:** `f0..f9`, `d0..d9`, `fTemp*`, `dTemp*`, `fT`, `dT*`.
- **Переменные, которыми управляет код (`dCurrentProblem`, `dCurrentSender`, `fDifficulty`, `dMax*`):** см. комментарии в файле: `dMaxBuilder`, `dMaxTower`,
  `dMaxTransport`, `dMaxMine`, `dMaxPlant`, `dMaxStorage` - пределы числа юнитов; `dTechnologyFactor`, `dBaseFactor`, `dUnitBattleFactor`, `dArealFactor` -
  только чтение.
- **Настройки характера клана:** `fHazard`, `fAvidity`, `fPry`, `fAgressive`, `fPlentyEnergyLevel`, `fPlentyResourceSpace`, `fPlentyResourceAmount`,
  `dBuildingProtectHits`, `dPlaceProtectHits`.
- **Константы:** типы проблем `PBM_*` (0..15), приказы `ORDER_ROBOT_*`/`ORDER_BUILDING_*`, способы указания цели `TARGET_BY_*`, режимы вставки приказа
  `INSERT_ORDER_*`, типы зданий и роботов `BUILDING_*`/`ROBOT_*`, классы `CLASS_*`, ресурсы `RESOURCE_*`, критерии выбора юнита `SELECT_*`,
  статусы проблемы `ST_*`, сообщения `SYSTEM_MESSAGE`, `MESSAGE_INFO`, `OBJECTIVE_*`, `MISSION_*`, выражения `EXP_TARGET_*`/`ACTION_*`.

Те же `VAR(...)` есть и в бинарном варианте в `behpsp.res` (`VarsetLib.BinaryVarsetFileParser`).

## 5. Встроенные функции (`ScriptIndex`)

Таблица в `SuperAI::ctor` (`ai.dll` 0x02461000) содержит 59 функций `action_0..action_58`. Аргументы - индексы переменных `varset.var`. Для 36 из них известна роль
(`ScrLib.ScrActions`); остальные 23 пока безымянны (часть из них в данных не встречается).

| № | Имя | Аргументы (по использованию в данных) | Результат |
|---|---|---|---|
| 0 | `BindGlobalSuperAI` | одна переменная, игнорируется | `true` |
| 2 | `RequestUnitsForProblem` | `PBM_*`, fTemp, dTemp1, dTemp2, класс юнита, NONE, `SELECT_*` | - |
| 3 | `GetDistributorSurplus` | - | float: остаток распределителя ресурсов |
| 6 | `SetUnitCriticalStatus` | LogicID юнита, `UNIT_NORMAL`/`UNIT_CRITICAL` | - |
| 7 | `DeleteCurrentProblem` | - | - |
| 8 | `SetCurrentProblemStatus` | `ST_SOLVING`/`ST_SOLVED` | - |
| 9 | `Nop` | - | - |
| 11 | `CheckBuildLimitForType` | тип здания/робота | лимит достигнут или нет |
| 12 | `GetCurrentProblemPriority` | - | float |
| 13 | `FindFreeUnitOfType` | `UNIT_FREE_UNIT`/`UNIT_ANY_UNIT`, тип | LogicID юнита |
| 14 | `FindNearestUnitOfType` | `UNIT_ANY_UNIT`/`UNIT_ANY_CAPTURER`/..., тип | LogicID юнита |
| 15 | `IssueOrderToUnit` | юнит, `ORDER_*`, `INSERT_ORDER_*`, проблема, fSuccess, fSurvive, fTime, fIndependence, `TARGET_BY_*`, цель | - |
| 16 | `GetStorageResourceAmount` | `BUILDING_STORAGE`, `RESOURCE_ORE` | float |
| 17 | `GetMineResourceAmount` | `BUILDING_MINE`, `RESOURCE_LEFT_ORE` | float |
| 19 | `GetClanBasePlaceAndId` | три переменные для X, Y базы и индекса клана | - |
| 20 | `RegisterProblemTargetPoint` | проблема, X, режим, значение | - |
| 24 | `DisbandGroup` | номер группы | - |
| 25 | `CreateGroup` | `TAKE_*`/`CREATE_EMPTY`, число | номер группы |
| 27 | `AddExpression` | `ACTION_*`, `EXP_TARGET_BY_*`, цель | - |
| 28 | `IssueOrderToGroup` | группа, `ORDER_*`, ... как у №15 | - |
| 29 | `GetCurrentProblemField` | 0, 1 или 2 | float/int: поле проблемы |
| 30 | `SendGameMessage` | вид сообщения, значение | - |
| 31 | `CountClanUnitsByClass` | клан, маска `CLASS_*` | число |
| 32 | `ArealContainsLogicId` | ареал, значение | true/false |
| 34 | `CountOwnUnitsByType` | `BUILDING_*` | число |
| 35 | `FindNearestEnemyUnit` | дальность, переменная под расстояние | LogicID |
| 37 | `FindNearestOwnBuilding` | переменная под расстояние | LogicID |
| 38 | `CountSummaryClanBF` | клан, `FREE_UNITS`/`ALL_UNITS` | число |
| 49 | `CountFreeMinds` | - | число |
| 50 | `GetUnitFromProblem` | `ORDER_ROBOT_*` | LogicID |
| 51 | `SetEntityFreeOrReserved` | LogicID, `TRUE`/`FALSE` | - |
| 52 | `QueryBehaviourByLogicId` | LogicID | значение или ошибка |
| 54 | `GetUnitOrderState` | LogicID | - |
| 56 | `IsDistributorFlagSet` | - | bool |
| 57 | `HostCallKind2` | два значения | - |
| 58 | `GetOwnBuildingByIndex` | порядковый номер | LogicID |

Функции, чьи имена пока не определены: 1, 4, 5, 10, 18, 21, 22, 23, 26, 33, 36, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 53, 55.

## 6. Проблемы (`Problem`)

«Проблема» клана - это запрос (нужен рудник, энергия, защита базы...), по которому ИИ подбирает юнитов и выдаёт приказы. Тип проблемы - `PBM_*` из `varset.var`
(`PBM_MINE_NEEDED` 1, `PBM_N_E_ENERGY` 2, `PBM_BUILDING_NEEDED` 3, `PBM_N_OPTIMAL_TRANSPORT` 4, `PBM_BUNKER_NEEDED` 5, `PBM_ROBOT_NEEDED` 6, `PBM_BUILDING_ATTACK` 7,
`PBM_BUILDING_PROTECT` 8, `PBM_BUILDING_CAPTURE` 9, `PBM_MAKE_RESEARCH` 10, `PBM_PLACE_PROTECT` 11, `PBM_BASE_DEFENCE` 12, `PBM_ATTACK_UNIT` 13,
`PBM_BUILDING_INF_CAPTURE` 14, `PBM_UPGRADE_NEEDED` 15).

Структура `Problem` в `ai.dll`: тип, три параметра цели (`target_x`, `target_y`, `target_extra`), приоритет, число шагов и убывание шага, статус (`ST_NONE`/`ST_SOLVING`/
`ST_SOLVED`/`ST_UNSOLVED`), назначенные юниты, цели-выражения `goal_expressions` (их добавляет `AddExpression`) и группы `groups`
(их создаёт `CreateGroup`). Критерий решения - `solve_mode`: 0 (`OR_TASK`) - достаточно выполнить любое выражение, 1 (`AND_TASK`) - все. Выражения проверяет
`Problem::CheckGoalExpression`: уничтожена цель, цель бездействует, здание захвачено, уничтожена группа, группа бездействует.

## 7. Как посмотреть

- Панель `ScrExplorer` в NResUI показывает секции и инструкции с названиями встроенных функций.
- `VarsetExplorerPanel` - переменные и константы.
- Код разбора: `ScrParser.ReadFile`, `ScrFile`, `ScrEntry`, `ScrEntryInner`, `ScrActions`; XML-комментарии к типам содержат ту же информацию.
