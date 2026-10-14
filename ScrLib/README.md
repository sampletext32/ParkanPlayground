# ScrLib: скрипты миссий (`.scr`, `.fml`, `varset.var`)

![SCR Explorer: секции скрипта ИИ, инструкции и названия встроенных функций](../docs/images/scr-explorer.png)
*SCR Explorer: секции скрипта ИИ, инструкции и названия встроенных функций*


Стратегический ИИ клана («SuperAI», библиотека `ai.dll`) управляется скриптами из папки `MISSIONS/SCRIPTS`. Один скрипт состоит из трёх файлов
с одним именем:

| Файл | Что хранит | Парсер |
|---|---|---|
| `NAME.scr` | бинарный файл: секции-обработчики событий и проблем, в них - инструкции | `ScrLib.ScrParser` |
| `NAME.fml` | текстовые формулы: по одной на вызов «формулы» из `.scr` | `FmlParser.ReadFile` → `FmlFile` (список `FmlFormula`) |
| `varset.var` | текстовый набор переменных и констант, общий для всех скриптов | `VarsetLib.VarsetParser` |

В первой части игры 50 пар `.scr` + `.fml`, один `varset.var` (у второй части `magic` скриптов 73, формат шире и здесь не разбирается); кроме них там же лежат 49 файлов `.trf` (дерево исследований, см. `ResTreeLib`).
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
| `Init` | 50 | один раз при старте (`ISuperAI::ExecuteInitEntryOnce`, флаг `is_Init_executed`) |
| `Mission` | 50 | обработчик миссии, не чаще раза в 2 секунды (`TryExecuteMissionEntry`); индекс - `Mission_index` |
| `Problems0` | 50 | на каждом шаге ИИ: создаёт и продлевает проблемы (`Problems0_index`) |
| `Fort_Task_Complete`, `Fort_Captured` | 50 | события зданий: код 3 - задание выполнено, код 7 - здание захвачено (`Fort_Task_Complete` обязательна: без неё игра завершает работу с ошибкой скрипта) |
| `Mech_GeneratorFound`, `Mech_Mineral_Found`, `Mech_Task_Complete` | 50 | события юнитов: коды 1 - найден генератор, 2 - найдена руда, 3 - задание выполнено (`Mech_GeneratorFound` обязательна) |
| `Hero_Teleported` | 50 | событие юнита с кодом 8 - герой телепортирован |
| `PBM_*_Start` | 11-16 | запуск решения проблемы: выполняется для проблем в состоянии NONE/UNSOLVED, самых приоритетных первыми |
| `PBM_*_Continue` | 11-16 | продолжение: выполняется на каждом шаге ИИ для проблем в состоянии SOLVING |

### Шаг ИИ (когда что выполняется)

Источник - `ISuperAI::AI_step` (`ai.dll` 0x02461730), вызывается каждый кадр, но работает по таймеру `next_step_time`: следующий шаг назначается
на `timeGetTime() + 8000 + rand() % 1000` мс, то есть примерно раз в 8-9 секунд реального времени. Каждое появление объекта клана
(`OnEntityRegistered`) сдвигает следующий шаг на 1,6 секунды раньше. «Шаг» в `RequestUnitsForProblem` (число шагов и убывание) - это шаг ИИ, а не кадр.
Шаг выполняется только при `ai_mode != 0` (по умолчанию 2).

Порядок внутри шага:

1. **Обновление** (`SuperAI_State::UpdateEntitiesAndProblems`): у каждой управляемой сущности проверяется, что она жива и принадлежит клану
   (иначе `AIManagedEntity::ReleaseEntity`: открепление от проблемы); у каждой проблемы вызывается `Problem::ProcessProblemTick` (уменьшение шагов, проверка целей); пересчитываются
   переменные `dTechnologyFactor`, `dBaseFactor`, `dUnitBattleFactor`; обновляются отношения к другим кланам.
2. **`PBM_*_Continue`** для всех проблем в состоянии SOLVING (`dCurrentProblem` = индекс проблемы).
3. **`Problems0`**: скрипт оценивает ситуацию и вызывает `RequestUnitsForProblem` - создаёт новые проблемы или продлевает существующие.
4. **`PBM_*_Start`**: из проблем в состоянии NONE/UNSOLVED выбираются самые приоритетные, для каждой выполняется её `_Start`. Если после скрипта
   проблема не стала SOLVING/SOLVED (скрипт не нашёл юнитов и вышел по `Return`), цикл останавливается: менее приоритетные проблемы не запускаются,
   пока не обеспечена более приоритетная. Иначе берётся следующий уровень приоритета.

События не привязаны к шагу: `NotifyEntityEvent` вызывается из `Behavior.dll` сразу и запускает секцию события (`dCurrentSender` = LogicID отправителя),
если в этот момент не выполняется другой скрипт. Скрипты не вкладываются: пока идёт один (`is_executing_entry`), остальные пропускаются.

Состояния проблемы (`Problem.state`): 0 NONE, 1 SOLVING, 2 SOLVED, 3 UNSOLVED. Решённая (`on_problem_solved`) и нерешённая (`on_problem_unsolved`) проблема
сразу очищается (`type = 0` - слот свободен), её юниты освобождаются, а при `ai_mode == 2` меху посылается приказ 9 (`ORDER_ROBOT_REPARE`).
В сохранение игры попадают только проблемы `PBM_BUILDING_INF_CAPTURE` (тип 14), остальные скрипт `Problems0` создаёт заново.

Всего в 50 файлах 637 секций и 5767 инструкций, от 9 до 39 секций в файле. Если версия скриптов не совпадает с таблицей игры
(число встроенных функций не 59), игра сообщает `Scripts are not up to date`.

## 2. Инструкции

Секция при каждом вызове выполняется заново с первой инструкции до конца, до `Return` или до перехода. Условия образуют стек: каждая инструкция
`Compare` увеличивает глубину, её результат равен «сравнение И результат на предыдущей глубине»; `EndIf` уменьшает глубину. Инструкции
`Execute`, `GotoInstruction`, `GotoEntry` и `Return` выполняются только на нулевой глубине или при истинном верхнем условии.

| Тип | Имя (`ScrInstructionType`) | Смысл |
|---|---|---|
| −1 | `Execute` | вызвать встроенную функцию `action_N` (номер `NativeActionIndex`), либо скопировать переменную (`ValueOrSourceVarIndex`), либо вычислить формулу из `.fml` (`FmlFunctionIndex`); результат - в переменную `ResultVarIndex` |
| 0 | `Compare` | сравнить две переменные (аргументы); операция в `CompareOperator`: 0 `<`, 1 `==`, 2 `>`, 3 `<=`, 4 `>=`, 5 `!=`; `DWORD` сравнивается как целое, остальное как float |
| 1 | `EndIf` | закрыть условие (глубина −1) |
| 2 | `Nop` | ничего (9 раз в данных) |
| 3 | `GotoInstruction` | перейти на инструкцию `args[0]` (с 1) в этой секции |
| 4 | `GotoEntry` | перейти к началу секции `args[0]` |
| 5 | `Return` | завершить секцию на этом вызове |
| 6 | `SetVarsetValue` | `var[ResultVarIndex] = ValueOrSourceVarIndex` |

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
это порядковый номер формулы (с 0, считая только строки `FUNCTION`), на который ссылается инструкция `Execute` (`FmlFunctionIndex`); парсер: `FmlParser`. Число формул в `.fml` совпадает с числом инструкций-формул в `.scr`
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

## 5. Встроенные функции (`NativeActionIndex`)

Таблица в `SuperAI::ctor` (`ai.dll` 0x02461000) содержит 59 функций `action_0..action_58`. Аргументы - индексы переменных `varset.var`. Для 44 из них известна роль
(`ScrLib.ScrActions`); остальные 15 пока безымянны (часть из них в данных не встречается).

| № | Имя | Аргументы (по использованию в данных) | Результат |
|---|---|---|---|
| 0 | `BindGlobalSuperAI` | одна переменная, игнорируется | `true` |
| 2 | `RequestUnitsForProblem` | `PBM_*`, приоритет, число шагов, убывание за тик, `target_x`, `target_y`, `target_extra` (подробно - в разделе «Функция №2» ниже) | - |
| 3 | `GetDistributorSurplus` | - | float: избыток энергии клана = `power_available - power_usage` из итогов `Distributor` ([docs/DISTRIBUTOR.md](../docs/DISTRIBUTOR.md)) |
| 6 | `AssignUnitToCurrentProblem` | LogicID юнита, `UNIT_NORMAL`/`UNIT_CRITICAL` | - (привязывает юнит к текущей проблеме `dCurrentProblem` и ставит ему критичность; без критичных юнитов проблема в состоянии SOLVING становится UNSOLVED) |
| 7 | `DeleteCurrentProblem` | - | - |
| 8 | `SetCurrentProblemStatus` | `ST_SOLVING`/`ST_SOLVED` | - |
| 9 | `Nop` | - | - |
| 11 | `CheckBuildLimitForType` | тип здания/робота | лимит достигнут или нет |
| 12 | `GetCurrentProblemPriority` | - | float |
| 13 | `FindFreeUnitOfType` | `UNIT_FREE_UNIT`/`UNIT_ANY_UNIT`, тип | LogicID юнита |
| 14 | `FindNearestUnitOfType` | `UNIT_ANY_UNIT`/`UNIT_ANY_CAPTURER`/..., тип | LogicID юнита |
| 15 | `IssueOrderToUnit` | LogicID юнита, `ORDER_*`, `INSERT_ORDER_*`, параметр приказа, `fSuccess`, `fSurvive`, `fTime`, `fIndependence`, `TARGET_BY_*`, цель (подробно - «Приказы» ниже) | 1 принят, 0 отклонён, 5 юнит не найден |
| 16 | `GetStorageResourceAmount` | `BUILDING_STORAGE`, `RESOURCE_ORE` | float |
| 17 | `GetMineResourceAmount` | `BUILDING_MINE`, `RESOURCE_LEFT_ORE` | float |
| 18 | `FindBuildPlaceNearBase` | X, Y (оба - выходные) | - (место пишется в аргументы; `0, 0` - свободного места нет). Спираль вокруг базы до радиуса 1500: проходимость, покрытие ареалом, ровность (`GetTerrainHeightAt`), дальше 250 от других объектов, месторождений и недавно занятых точек; найденная точка запоминается на 60 секунд (`RegisterRecentSite`) |
| 19 | `GetClanBasePlaceAndId` | три переменные для X, Y базы и индекса клана | - |
| 20 | `RegisterProblemTargetPoint` | проблема, X, режим, значение | - |
| 24 | `DisbandGroup` | номер группы | - |
| 25 | `CreateGroup` | `TAKE_*`/`CREATE_EMPTY`, число | номер группы |
| 27 | `AddExpression` | `ACTION_*`, `EXP_TARGET_BY_*`, цель | - |
| 28 | `IssueOrderToGroup` | номер группы текущей проблемы, далее как у №15 (цель `TARGET_BY_NAME` не поддерживается) | 1 все приняли, 0 отклонён, 5 юнит не найден |
| 29 | `GetCurrentProblemField` | 0, 1 или 2 | float/int: поле проблемы |
| 30 | `SendGameMessage` | вид сообщения, значение | - |
| 31 | `CountClanUnitsByClass` | клан, маска `CLASS_*` | число |
| 32 | `ArealContainsLogicId` | ареал, значение | true/false |
| 34 | `CountOwnUnitsByType` | `BUILDING_*` | число |
| 35 | `FindNearestEnemyUnit` | дальность, переменная под расстояние | LogicID |
| 36 | `FindWeakestEnemyTarget` | радиус (float), переменная под значение веса | LogicID вражеского объекта, вокруг которого суммарный вес минимален; вес пишется во второй аргумент |
| 37 | `FindNearestOwnBuilding` | переменная под расстояние | LogicID |
| 38 | `CountSummaryClanBF` | клан, `FREE_UNITS`/`ALL_UNITS` | число |
| 40 | `FindFreeDepositOrEnemyMine` | X, Y, вес (все выходные) | `ERROR` при найденном свободном месторождении (его X, Y и вес - в аргументы); иначе LogicID здания на месторождении чужого клана с минимальным весом вокруг |
| 41 | `StartSelectedResearch` | LogicID института | bool: исследование выбранной схемы запущено |
| 43 | `SelectCheapestScheme` | - | - (один раз в `Init`: перебирает схемы `.dat`, запоминает самую дешёвую по `CalcFullResearchCost`) |
| 44 | `CalcWeightAroundPoint` | радиус, клан или `ERROR` (все чужие), `TARGET_BY_LOGIC_ID`/`TARGET_BY_PLACE`, LogicID или X, Y | сумма весов объектов в радиусе (целое) |
| 45 | `GetProblemAssignedWeight` | - | сумма весов юнитов, назначенных на текущую проблему |
| 47 | `FindEnemyNearBase` | запас дальности (в данных 0), переменная под вес | LogicID первого вражеского объекта ближе `запас + furthest_building_distance` от базы, иначе `ERROR` |
| 49 | `CountFreeMinds` | - | число |
| 50 | `GetUnitFromProblem` | `ORDER_ROBOT_*` | LogicID |
| 51 | `SetEntityFreeOrReserved` | LogicID, `TRUE`/`FALSE` | - |
| 52 | `QueryBehaviourByLogicId` | LogicID | значение или ошибка |
| 54 | `GetUnitOrderState` | LogicID | - |
| 56 | `IsDistributorFlagSet` | - | bool: название историческое - реализация возвращает `scheme_deposit_manager.fAll_schemes_checked` (проверены ли все схемы), с распределителем не связана |
| 57 | `HostCallKind2` | два значения | - |
| 58 | `GetUpgradableBuildingByIndex` | порядковый номер | LogicID n-го собственного здания, которое можно улучшить, или `ERROR` |

**Что такое «вес».** Функции 36, 40, 44, 45, 47 (и 35, 37) считают силу как сумму весов; вес объекта - `(f24 + 0.8) * f20`, где `f20`, `f24` - два float записи
об объекте в списке ареала (`IArealMap`, запись 0x28 байт: LogicID, клан, x, y, ...). По смыслу (`dBuildingProtectHits`, `TAKE_BY_HITS`) это оценка по hits юнита;
точный смысл двух float по коду не подтверждён. Фильтр по клану: `ERROR` - все кланы с враждебным отношением (см. «Отношения между кланами»), иначе только указанный клан.
Типичный сценарий вражеского скрипта: `dTemp = action_36(300, dTemp3)` → `RequestUnitsForProblem(PBM_ATTACK_UNIT, ..., dTemp, NONE, dTemp3)`; при защите
`action_45` сравнивается с `dBuildingProtectHits`, а нехватка добирается `CreateGroup(TAKE_BY_HITS, нехватка)`.

**Не разобраны:** 1, 4, 5, 10, 21, 22, 23, 26, 33, 39, 42, 46, 48, 53, 55. Ни одна из них не встречается ни в одном из 75 `.scr` (проверено по [дампам](../docs/scripts/README.md): `grep "act=N "`), поэтому игра их не использует; реверс этих функций остановлен как не имеющий практической ценности. Если понадобятся, они лежат в таблице `SuperAI::ctor` (`ai.dll` 0x02461000) как `action_N`.


## Функция №2: `RequestUnitsForProblem`

Самая частая встроенная функция (202 вызова из 1877): создаёт проблему клана или продлевает уже существующую. Реализация - `SuperAI::ActionRequestUnitsForProblem`
(`ai.dll` 0x02469232) и `SuperAI_State::apply_action` (0x024657d0).

### Аргументы

Все семь аргументов - индексы переменных Varset.

| № | Что это | Тип переменной | Куда попадает в `Problem` | Значения в данных |
|---|---|---|---|---|
| 0 | тип проблемы `PBM_*` | DWORD | `nProblem_type`; **имя** переменной определяет секции скрипта | `PBM_ROBOT_NEEDED` 131 вызов, `PBM_BUILDING_INF_CAPTURE` 16, `PBM_BUILDING_NEEDED` 11, ... (14 типов) |
| 1 | приоритет | float | `priority` | 0.45 .. 1.0: чаще всего 0.5 (25), 0.52 (18), 0.8 (15), 0.55 (7) |
| 2 | число шагов | число | `remaining_steps` и `nInitial_steps` | 25 (91 раз), 1 (42), 5 (45), 15, 30, 14, 51, 35 |
| 3 | убывание за тик | число | `step_decrement` | пара «шаги / убывание»: 25/24 (91), 1/0 (42), 5/2 (42), 15/14 (8), 30/10 (5), 14/13 (4), 51/50 (4), 35/34 (3), 5/4 (3) |
| 4 | `target_x` | DWORD или float | `flTarget_x` | у `PBM_ROBOT_NEEDED` - класс юнита (`ROBOT_BATTLEUNIT`, `ROBOT_BUILDER`...), у `PBM_BUILDING_NEEDED` - тип здания (`BUILDING_PLANT`), у `PBM_ATTACK_UNIT` - LogicID цели |
| 5 | `target_y` | DWORD или float | `flTarget_y` | почти всегда `NONE` |
| 6 | `target_extra` | DWORD или float | `flTarget_extra` | у `PBM_ROBOT_NEEDED` - критерий выбора `SELECT_*`, у `PBM_ATTACK_UNIT` - требуемая сила (результат №36 / №47) |

Переменные аргументов 4-6 в float переводятся так: DWORD -> число, float -> как есть, любой другой тип (целое со знаком, bool, строка) -> 0.
Поэтому для целей и классов в `varset.var` используются именно DWORD-константы.

### Что делает

1. Из **имени** переменной аргумента 0 строятся имена секций `<имя>_Start` и `<имя>_Continue` (для `PBM_ROBOT_NEEDED` это `PBM_ROBOT_NEEDED_Start`
   и `PBM_ROBOT_NEEDED_Continue`) и ищутся в скрипте. Найденные индексы запоминаются в `start_problem_entry_indices[тип]` и
   `continue_problem_entry_indices[тип]`. **Если в скрипте нет любой из двух секций, запрос молча отбрасывается.** Поэтому в каждом `.scr`
   есть пары секций (пусть и пустые, например `PBM_BASE_DEFENCE_Continue` с 0 инструкций).
2. Строится `Problem` с полями из таблицы выше.
3. Тождество проблемы - тройка (`nProblem_type`, `target_x`, `target_y`). Если проблема с такой тройкой уже есть в `ProblemsArray`:
   - при `priority` не меньше текущего - приоритет заменяется, а к `remaining_steps` прибавляется `nInitial_steps` (проблема «продлевается»);
   - иначе запрос игнорируется.
4. Если такой проблемы нет - она добавляется в `ProblemsArray`.
5. Результат функции - `FALSE`.

Дальнейшая жизнь проблемы: каждый тик (`Problem::Update`) `remaining_steps` уменьшается на `step_decrement`; при значении меньше 1 проблема считается решённой.
Поэтому пара 25/24 даёт проблему, которая без повторного запроса истекает через два тика, а пара 1/0 (`step_decrement` = 0) по шагам не истекает и живёт,
пока не выполнится цель. Повторный запрос с той же тройкой продлевает проблему (пункт 3); скрипты секции `Problems0` именно так и составлены - условие проверяется
заново и запрос повторяется. `<имя>_Start` - начало решения проблемы, `<имя>_Continue` - продолжение; обе читают параметры проблемы через `GetCurrentProblemField`.

### Типичный пример (из `13e.scr`, секция `Problems0`)

```
fTemp  = action_12 GetCurrentProblemPriority()
fTemp  = fTemp + 0.00001
dTemp1 = 25      ; число шагов
dTemp2 = 24      ; убывание за тик
action_2 RequestUnitsForProblem(PBM_ROBOT_NEEDED, fTemp, dTemp1, dTemp2, ROBOT_BATTLEUNIT, NONE, SELECT_SMALLEST)
```

## Отношения между кланами

В `SuperAI_State.clan_relations[клан]` (в Ghidra так названо поле `array_of_64_1`) для каждого другого клана хранится запись из четырёх значений:
«настроение» 0..1 (float), накопленные уменьшение и увеличение, и **отношение** (`[3]`): 0 - враждебный, 1 - нейтральный, 2 - дружественный.
Функции №35, №36, №40, №44 (с `ERROR`) и №47 считают противниками только кланы с отношением 0. Отношение выводится из настроения порогами
0.3333 и 0.6666 (`SuperAI_State::UpdateClanRelations`), начальное настроение - 0.1667, то есть по умолчанию клан враждебен.

## Приказы (`IssueOrderToUnit` №15 и `IssueOrderToGroup` №28)

Обе функции собирают команду `BehaviourCommand` (0x7C байта) и отправляют её поведению юнита (`IBehaviour`, слот 0xC); №28 отправляет её каждому живому юниту
группы текущей проблемы (`dCurrentProblem`). Приказ выполняется только при `ai_mode == 2` или `global_index == 0`, иначе результат 0.

| Аргумент | Что это | Поле команды |
|---|---|---|
| 0 | LogicID юнита (№15) или номер группы (№28) | - |
| 1 | `ORDER_ROBOT_*` / `ORDER_BUILDING_*` | `order_type` |
| 2 | режим вставки: `INSERT_ORDER_TO_END` 1, `TO_START` 2, `REPLACE` 3 | второй аргумент отправки |
| 3 | параметр приказа: радиус патруля (150, 200...), класс здания для строительства по имени, часто `NONE` | `order_param` |
| 4-7 | `fSuccess`, `fSurvive`, `fTime`, `fIndependence` | `success`, `survive`, `time`, `independence` |
| 8 | вид цели: `TARGET_BY_LOGIC_ID` 0x201, `TARGET_BY_PLACE` 0x202, `TARGET_BY_TYPE` 0x203, `TARGET_NOT_DEFINED` 0x204, `TARGET_BY_NAME` 0x205 | `target_kind` |
| 9, 10 | цель: LogicID, либо X и Y (для 0x202), либо тип/класс (0x203), либо критерий `SELECT_*` (0x205) | `target` |

`TARGET_BY_NAME` (только в №15; в данных используется с `ORDER_BUILDING_CONSTRUCT`): аргумент 3 - класс здания (`BUILDING_*`), аргумент 9 - критерий `SELECT_*`;
игра берёт из каталога схем лучшую схему этого класса (`SchemeDepositManager::FindBestScheme`) и подставляет имя её файла как цель. Если подходящей схемы нет, результат 0.
Приказ `ORDER_ROBOT_BUILD` (7) с целью-местом расширяет `furthest_building_distance` (максимальное расстояние построек от базы).

## Каталог схем и месторождения (`SchemeDepositManager`)

Объект `SuperAI_State.scheme_deposit_manager` (36 байт) хранит два списка:

- **Схемы** - файлы `.dat` каталога юнитов и зданий; `SchemeRecord` по 0x124 байта: путь, признак «можно построить» (`is_buildable`), `class_code`, `level`, `hits`, `factor`.
  Каталог строится один раз (`LoadSchemeCatalog`, из `ActionSelectCheapestScheme`): перебираются `*.dat`, для каждого создаётся объект, снимаются `class_code`, `level`, `hits`, `factor`,
  результат кешируется в файле `preload.lda` рядом со схемами.
- **Месторождения** - записи `DepositRecord` по 0x18 байт (`RefreshDeposits`): занято ли, клан-владелец, положение, LogicID здания на месторождении.

Выбор лучшей схемы (`FindBestSchemeIndex`) по критерию `SELECT_*`: `BEST_WEAPON` 1, `BEST_ARMOR` 2, `BEST_RANGE` 3, `BEST_SPEED` 4, `BEST_COMBAT` 5, `SMALLEST` 6 (подробности критериев -
в комментарии к функции в Ghidra). Допускаются схемы, чей уровень не выше максимального уровня института клана (`FindMaxInstituteLevel`).

## Прочие механизмы `ai.dll`

- **Ум (`Mind`).** У ИИ клана есть фиксированное число «умов» - слотов управляемых мехов (`CountFreeMinds` считает свободные: `logic_id == -1`). Поведение меха
  при привязке к ИИ (`Behaviour::ReloadSuperAI` в `Behavior.dll`) занимает свободный ум и записывает в него свой LogicID; если свободных нет, игра завершает работу
  с ошибкой (сообщение «Behaviour panic: SuperAI have no ...» из `Behavior.dll`). Освобождается ум, когда мех пропал (`AIManagedEntity::ReleaseEntity` -> `ReleaseMindByLogicId`).
- **Недавно занятые точки** (`g_recent_sites`, 20 слотов): `FindBuildPlaceNearBase` и `FindFreeDepositOrEnemyMine` резервируют найденную точку на 60 секунд,
  чтобы несколько юнитов не выбрали одно место. Раньше эти данные были названы «таймерами» (`g_timers`).
- **Границы карты** для ИИ (`ComputeMapBounds`, `g_flMap_limit_x/y`) берутся из границы мира при создании ИИ; точки вне карты `FindBuildPlaceNearBase` отвергает.
- **Экспорты.** `CreateSuperAI` (ordinal 1) создаёт ИИ клана и запоминает последний аргумент - обратный вызов хоста, который сообщает о потере героя;
  `GetSuperAI(clan_index)` (ordinal 2) возвращает ИИ клана 0..63. Хранилище - `g_super_ais[64]`.
- **Клан-данные исследований.** Указатель на данные исследований клана (`SetClanResearchData` / `GetClanResearchData`) передаётся в менеджер схем
  и в поведения зданий (`Behaviour.field_0x50`).

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

- Панель `SCR Explorer` в NResUI: у каждой секции две вкладки - «Текст» (читаемый вид с отступами по условиям, формулами из `.fml`, именами переменных из
  `varset.var`; при наведении - подробности: роль аргумента, значение по умолчанию, расшифровка операции) и «Таблица» (сырые поля инструкции).
  Рядом с `.scr` автоматически загружается одноимённый `.fml`; имена переменных появляются, если открыт `varset.var` (`File` -> `Open Varset File`).
- `VARSET Explorer` - переменные и константы.
- Код разбора: `ScrParser.ReadFile`, `FmlParser.ReadFile`, `ScrFile`, `ScrEntry` (секция), `ScrInstruction` (инструкция), `ScrActions` (имена и аргументы
  встроенных функций), `ScrTextDumper` (текстовый вид); XML-комментарии к типам содержат ту же информацию.
- Готовые дампы всех скриптов - в [docs/scripts](../docs/scripts/README.md).

## Текстовые дампы

Готовые текстовые дампы всех скриптов (с формулами и именами переменных) лежат в [docs/scripts](../docs/scripts/README.md); их строит `ScrTextDumper`. Удобны для поиска (`grep`) и сверки с `ai.dll`.
