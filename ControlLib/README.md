# Control: система управления объектами (Control.dll)

Заметки по реверсу `Control.dll` игры Parkan: Iron Strategy и по форматам, которые она читает (CTL, CPT, NDP, записи агентов).
Адреса даны для `Control.dll` (image base `0x10000000`), если не указана другая библиотека. Статус утверждений:
**проверено** (по коду и данным), **гипотеза** (данные согласуются, кода нет), **не выяснено**.

Код ридеров: `ControlResourceReaders.cs` (`CtlFile`, `CptFile`, `NdpFile`), `AgentRecord.cs`, `ControlResourceDump.cs`.

## Содержание

1. [Общая модель](#1-общая-модель)
2. [Основные понятия](#2-основные-понятия)
3. [Команды CTL](#3-команды-ctl)
4. [Записи агентов](#4-записи-агентов-objectsrlb-fortifrlb)
5. [Система предметов CItem](#5-система-предметов-citem)
6. [Щит, дефлектор, радар](#6-щит-дефлектор-радар)
7. [Оружие и эффекты выстрела](#7-оружие-и-эффекты-выстрела)
8. [Имена в Ghidra](#8-имена-в-ghidra)
9. [Не выяснено](#9-не-выяснено)

---

## 1. Общая модель

Control — слой игровой логики, который связывает схему объекта с ресурсами. Робот — это не один файл: дерево схемы
(`UNITS\*.dat`, см. `CpDatLib`) решает, какие ресурсы загрузить и как их присоединить друг к другу.

| Ресурс | Что хранит | Тип NRes |
|---|---|---|
| MSH | меш: дерево деталей (pieces), анимация | `MESH` |
| WEA | таблица материалов (грузится отдельно, к Control не относится) | `WEAR` |
| CPT | именованные контрольные точки: якоря эффектов, оси стволов, контакт колёс, камера, центр башни | `CTPT` |
| NDP | жизнь/повреждения по деталям и ресурс взрыва детали (`explode_*.exp`) | `NDPR` |
| CTL | корневые лимиты, граф состояний, привязки деталей, предметы, списки команд | `CTLD` |

**Порядок загрузки корневого объекта:** MSH → WEA → CPT (в `CPT refs`) → NDP (в `NDP refs`) → CTL (лимиты, граф состояний,
матрица весов рёбер, привязки деталей, записи предметов, таблица хуков команд, списки команд) → значения по умолчанию
привязок и предметов → список команд `on_load`.

**Присоединённые объекты** (башни, орудия) используют те же CPT/NDP/CTL, но их локальные индексы деталей переносятся в меш родителя:
локальная деталь 0 — виртуальный корень присоединения; деталь N > 0 → первая деталь этого msh_tag + N − 1. Запись NDP 0 для
присоединённых объектов пропускается.

**`AttachInd` схемы** (поле узла дерева):

| Категория узла | Что выбирает `AttachInd` |
|---|---|
| Part / Armour / Ammo | существующий слот предмета CTL, который настраивается или заменяется |
| Turret / Cannon | деталь-гнездо (attach piece) родительского меша |

Утверждение хорошо согласуется с данными шасси, башен, орудий и боеприпасов (гипотеза, подтверждена косвенно).

## 2. Основные понятия

### 2.1 CControlBase и лимиты движения

`CControlBase` владеет всем состоянием управления одного составного объекта: лимитами, CPT refs, NDP refs, привязками, предметами,
графом состояний, списками команд и ссылками на ресурсы/эффекты.

| Поле | Смысл |
|---|---|
| `base_control_limits` | неизменные лимиты из корня CTL (потолок, база для расчётов) |
| `runtime_control_limits` | рабочие лимиты: копия базовых, пересчитываются от повреждений, мощности двигателя, целостности колёс и скорости |

Эффективная скорость зависит от: мощности двигателей, средней целостности левых/правых колёс, `mobility integrity / max`,
текущего множителя скорости (`RecomputeEffectiveMovementLimits`, 0x10014630).

### 2.2 Критическое разрушение (critical decay)

| Что | Условие / эффект |
|---|---|
| Вход | суммарная жизнь ниже критического порога **или** целостность колёс катастрофически низка (у 4-колёсного — ≤ 1 колесо) |
| При входе | выполняется список `on_enter_critical_decay`; текущая жизнь запоминается как `critical_decay_reference_life_amount`; ремонтные системы принудительно выключаются (`SetItemRuntimeState(0)`) |
| Каждый тик | урон = `Settings.critical_decay_rate_per_second × reference_life × dt` |

### 2.3 NDP

Запись NDP = вклад детали в жизнь/подвижность и ресурс эффекта, который играет, когда деталь разрушена (`ControlNdpRef.damage_effect_resource_index`
— индекс кэша `g_ndp_damage_effect_resource_cache`; ресурсы — EXPL, `explode_*.exp`). Флаг `0x10` = DEPLETED (разрушена): функции подсчёта радиуса
взрыва пропускают такие записи, если не запрошены все.

### 2.4 CPT

| Поле записи | Смещение | Смысл |
|---|---|---|
| `OwnerPieceIndex` | +0x04 | деталь-владелец точки |
| `TargetPieceIndex` | +0x08 | связанная визуальная/функциональная деталь (у колёс шасси точка принадлежит корню, а цель — колесо) |
| Position, Direction | далее | положение и направление; **длина Direction масштабирует эффект** (см. §7.4) |

Различие Owner/Target не окончательное (гипотеза); при отладке показывать оба.

### 2.5 Привязки деталей (piece bindings)

Привязка связывает локальную деталь, необязательные CPT, диапазон анимации (`Anim1/Anim2 Start/End`), значение по умолчанию и флаги.
Предметы ссылаются на привязки; значение по умолчанию служит запасным значением и начальным прогрессом анимации.

| Флаг | Смысл |
|---|---|
| 0x01 | значение по умолчанию циклическое (wrap) |
| 0x02 | инвертировать значение по умолчанию |
| 0x04 | пропустить инициализацию анимации |
| 0x08 | авто-добавление/ссылка из системы предметов (цель не окончательна) |
| 0x100 | условная ссылка, добавляется в рантайме по флагам детали MSH |

Поле привязки `+0x24` (`ChannelSpeed`) — скорость канала анимации, ед./с (используется в §7.1).

### 2.6 Время и сеть

- `CControlClassic::UpdateTick(now_ms)` получает **абсолютное** время в мс, не дельту.
- Единицы: `current_control_time_ms`, `last_slow_update_time_ms`, `control_state_sim_time_ms` — мс; `ProcessNdpLifeAndMobilityState(dt)` и `UpdateItemRuntimeEffects(dt)` — секунды.
- Сетевая выборка положения: `Vector3 position; Quaternion rotation; float strafe_angle_z;` (0x20 байт). Интерполяция — по нормированной доле, не по секундам.

## 3. Команды CTL

Таблица хуков — 21 слот: индекс списка команд или −1 (нет списка).

| Слот | Смысл |
|---|---|
| 0 | `on_load` |
| 2, 3, 4 | состояния снаряда BULL (см. §7.3) |
| 6 | `on_enter_critical_decay` |
| 7 | `on_exit_critical_decay` |
| остальные | не расшифрованы |

Запись команды — 0x64 байта: условие (флаги, маска, инверсия) + опкод + 5 аргументов `arg0..arg4` (int или float, по опкоду) + `ResourceNamedRef`
(архив + имя, по 32 байта).

### Команды эффектов (`CControlBase::ExecuteControlCommandList`, 0x100028A0)

Проверено по коду и по 449 CTL первой части (файлы `is2_bases` — вторая часть, формат иной, движок первой части их не читает).
Эффект адресуется 32-битным id `(msh_tag << 16) | local_id`. У spawn-команд `local_id = arg3`; `0xFFFFFFFF` = «следующий из счётчика объекта»
(в данных не встречается). Ресурс — FXID в `effects.rlb`.

| Опкод | Команда | Аргументы и действие |
|---|---|---|
| 3 | SPAWN_EFFECT_ON_JOINT_BOUNDS | `arg0` = деталь от первой детали msh_tag, `arg3` = local_id; масштаб = радиус сферы детали, позиция = её центр (SPACE_LOCAL) |
| 4 | SPAWN_EFFECT_FROM_3_JOINTS | `arg0..2` = индексы CPT от первой CPT msh_tag, `arg3` = local_id; матрица из трёх CPT (§7.4); `arg4` игнорируется: в ветке не читается, в данных 0 у 806 из 937 записей, остальное разнородное с ASCII-мусором |
| 5 | SPAWN_EFFECT_ON_OBJECT_BOUNDS | `arg3` = local_id (в данных 9000); радиус и центр сферы всего объекта (SPACE_WORLD) |
| 8 | REMOVE_EFFECT | `arg0` = local_id |
| 10 | START_EFFECT | `arg0` = local_id, `arg1` = источник силы (`EffectStrengthSource`): Life_0to1 ×147, Constant ×30, Life_1minus ×4, Life_Fract ×3 |
| 11 | REQUEST_STOP_EFFECT | `arg0` = local_id; эффект доигрывает цикл и перезапускается с источником из файла |
| 14 | ATTACH_EFFECT_TO_PIECE | `arg0` = local_id, `arg1` = деталь; по её анимации считаются источники силы 4/16/17 |
| 18 / 19 | ACTIVATE / DEACTIVATE_EFFECT | `arg0` = local_id |

`IEffectManager::SetConstantStrength` из команд не вызывается: эффект стартует с Life-источником либо с Constant = значение по умолчанию (0).

Прочие опкоды: 0 RESET_MOTION, 1/2 MARK/UNMARK_ROOT_CENTRAL, 7 KILL_PIECE, 9 ITEM_LOAD_RESOURCE, 12/13 SET_TANG_ACCEL/NORM_SPEED, 15 SCHEDULE_DETACH_OR_KILL
(ставит `clan_index` = 0xFFFE, затем удаление через `DetachCleanupDelayMs`), 17 FORCE_KILL, 20 PLACE_BUILDING_ON_TERRAIN, 21 DAMAGE_OBJECTS_IN_BOUNDS, 22 SPAWN_OBJECT_FROM_3_JOINTS,
23/30 EXECUTE_ITEM_ACTION, 27 SWAP_NDP_RESOURCE (подмена EXPL и немедленный взрыв, §7.5), 29 INHERIT_CLAN_FROM_PARENT (`clan_index` = клан родителя через `ILifeSystem` слот 0x2C; в данных 4 раза: `ws_al_01`, `ws_al_02`, `ws_hm_01`, `ws_hm_02`), 40/41 QUEUE_PATH_TO_(RANDOM_)CONTROL_STATE, 42/43 NET_SYNC_FLAG.

## 4. Записи агентов (objects.rlb, fortif.rlb)

Агент — запись NRes без заголовка: массив 64-байтных ссылок (32 B архив + 32 B ресурс, ASCII). Тип записи — 4CC типа NRes, число ссылок = размер / 64.
Читает `LoadAgent` (AniMesh.dll) → `CAgent::try_load` (`niOpenResFileEx(name, 4)`).

| Ссылка | Смещ. | Роль | Вызов в `try_load` |
|---|---|---|---|
| 0 | 0x00 | MSH | `LoadAniMesh(data, data+0x20)` |
| 1 | 0x40 | WEA | `LoadMatManager(data+0x40, data+0x60)` |
| 2 | 0x80 | CPT | `LoadControlSystem(…, cpt)` |
| 3 | 0xC0 | NDP | `LoadControlSystem(…, ndp)` |
| 4 | 0x100 | CTL | `LoadControlSystem(ctl, …, AgentType)` |
| 5 | 0x140 | поведение (`behpsp.res` + `chas_*.var`, не NRes) | `CreateBehaviour` (только BTLU) |
| 6 | 0x180 | таблица `*.tbl` из корня игры | `CreateManManager` (только BTLU) |

### Типы записей

| Тип | Число | Размер | Что это | Откуда ссылка | Загрузчик |
|---|---|---|---|---|---|
| FORT | 30 | 128 | здание: ссылка 0 = BTLU, 1 = BASE (`.bas`) | корень cp-схемы с `ClassBuilding` (66/66) | `Terrain.LoadBuilding` → `CBuilding::ctor` (Terrain.dll 0x10033920) |
| BTLU | 55 | 448 / 320 | юнит (робот r_*, животное a_*); у зданий — внутренняя запись (bu_*) | корень схемы робота/животного (279/279) | `AniMesh.LoadAgent` |
| INTO | 192 | 320 | внутренний узел: двигатель, радар, щит, батарея, ремонт, броня, патроны (**INT**ernal **O**bject) | узлы Armour/Part/Ammo, редко Cannon (15) | `LoadAgent` через `CreateObject` |
| EXTO | 127 | 320 | внешний узел: башня, орудие, детали (**EXT**ernal **O**bject) | узлы Turret (352), Cannon (596), редко Armour (18) | то же |
| BULL | 53 | 320 | снаряд | `Resource` предмета CTL типа 0x02/0x1E (125/125) | `CreateObject(LOGIC_BULLET_AGENT_BULL)` |
| STAT | 66 | 384 | статический объект (камни, деревья) | имя в `data.tma` миссий | `LoadAgent` (LOGIC_STATIC_AGENT_STAT) |
| WPNS | 5 | 320 | оружие (корневое) | источник не найден | `LoadAgent` (LOGIC_WEAPON_AGENT_WPNS) |
| SUNO | 2 | 128 | солнце: ссылка 0 = `.sun`, 1 = `SunImage_01` | не проверено | — |

Выбор загрузчика — по `Comp.ini`: CID 1 (FORT) → `Terrain.LoadBuilding`; CID 3/4 (PUPPET/STATIC) → `AniMesh.LoadAgent`; CID 7 → `MisLoad.LoadResearch`.

### Откуда берутся имена записей (проверено по 347 схемам UNITS\**\*.dat)

| Тип узла схемы (`DatEntryType`) | Тип записи |
|---|---|
| Turret (1) | EXTO (352) |
| Armour (2) | INTO (266) или EXTO (18) |
| Part (3) | INTO (2135) |
| Cannon (4) | EXTO (596) или INTO (15) |
| Ammo (5) | INTO (384) |

Ни один узел схемы не ссылается на BULL/WPNS/STAT. Иначе:

- **BULL**: имя в `Resource` предмета CTL (+0x70) типа 0x02 (119) и 0x1E (6, `bld_*`), в CTL guns.rlb (109), animals.rlb (5), turrets.rlb (4); ссылка всегда `objects.rlb` → BULL.
- Другие внешние ссылки CTL: предмет 0x09 → FXID щита (65), команды SPAWN_*/START_* → FXID (1238), команда 27 → `weapon.rlb` EXPL (45).
- **STAT**: имена (`s_stone_04`, …) прописаны в `data.tma` миссий в записях Tree/Stone.
- **WPNS / SUNO**: ссылок нет, WPNS читает неизвестный код, SUNO — атмосфера.
- **INTO/EXTO** (гипотеза владельца, данные согласуются): имена предметов CTL шасси (`i_eng_l`, `i_pws_l`, `i_dsh_l`, `i_arm_*`, `i_rdr_*`, `i_def_*`, `i_fsh_*`, `i_rps_*`) совпадают с префиксами `i_*` узлов схемы (Armour 266/271, Part 271).

### Проверки и аномалии данных

| Наблюдение | Подробности |
|---|---|
| Ссылки 0–4 указывают на ресурс правильного типа | у всех записей, с точностью до регистра |
| Битые ссылки | 6 INTO (`i_c06_l_01/02/df`, `i_c07_l_01/02/df`) → `guns.rlb`/`o_c06_l_*` которых нет (есть `o_c06_b_*`); `SunImage_01` в SUNO; в схемах `fr_l_mast`, `fr_l_tele`, `u_tel_def_a_03` |
| Ссылка 6 у 25 BTLU | испорчено имя архива (`\0ehpsp.res`); значимо только имя ресурса (`m1.tbl`, `m2.tbl`, `hero.tbl`) |
| Ссылка 5 | не NRes; читается загрузчиком Behavior.dll |

Ошибки загрузки (`Unable to open library/file`) завершают игру, поэтому битые ссылки в игре, видимо, не используются.

`CBuilding::ctor(this, archive, FORT_name, owner, player)`: ссылка 0 FORT — BTLU (в `LoadAgent`), ссылка 1 — BASE (контуры здания, `BasLib`);
затем открывается MSH-ссылка BTLU как NRes в памяти (`niOpenResInMem`), хранится в `NResWithCTL`.

**Создание снаряда-прототипа** (`CItemType2_and_0x1e_ProjectileWeapon::InitAfterLoad`, 0x10023FE0): `CreateObject(LOGIC_BULLET_AGENT_BULL, bullet_archive_name,
bullet_entry_name, …)` создаёт снаряд, читает его `ILifeSystem` (`BASE_CONTROL_LIMITS`, `SUM_NDP_VOLUME`, свойства 0xA5/0x80/0x81/0x82) в параметры
предмета и сразу удаляет (`DeleteGameObject`). Настоящие выстрелы создаёт `SpawnProjectile` (§7.1).

## 5. Система предметов CItem

Предмет — экземпляр `CItemBase` (IItem). `CControlClassic::LoadCtlItemRecordAndAdvance` (0x100102E0) создаёт его по `item_type`, кладёт в `items_vector`
и вызывает `ParseCtlItemRecordAndInit(raw, first_new_piece_binding_index, true)`.

### 5.1 Фабрика типов

| Тип | Класс | Размер | vtable |
|---|---|---|---|
| 0x01 | `CItemType1_TurretDrive` (наследует `CItemType0x16`, ctor 0x1002B470) | 0x104 | 0x1003EA70 |
| 0x02, 0x1E | `CItemType2_and_0x1e_ProjectileWeapon` | 0x174 | — |
| 0x04 | `CItemType4_TargetingSystem` | 0xE0 | 0x1003E2F0 |
| 0x05 | `CItemType5_Engine` | 0xA0 | 0x1003E808 |
| 0x08 | `CItemType8_SensorRadar` | 0xCC | 0x1003E518 |
| 0x09 | `CItemType9_ForceShield` | 0x11C | 0x1003E738 |
| 0x0A | `CItemType0x0a_DetectionShield` | 0x98 | 0x1003E8D8 |
| 0x0F | `CItemType0xf_RepairSystem` | 0x9C | 0x1003E658 |
| 0x13 | `CItemType0x13_Battery` | 0x98 | 0x1003E620 |
| 0x15 | конструктор Engine, указатель в `item_type_0x15_deflector` (дефлектор) | 0xA0 | — |
| 0x16 | `CItemType0x16` (решатель упреждения) | 0xC0 | 0x1003E9A0 |
| 0x1A | `CItemType0x1a_EngineAlt` | 0xA4 | — |
| 0x1B | `CItemBase` (броня) + `ControlItemDescriptor` (0x10 байт) | 0x98 | 0x1003E220 |
| 3, 0xC, 0xD, 0x11, 0x14, 0x18, 0x19, 0x1D | конструктор Engine с обычным vptr (поведение `CItemBase`) | 0xA0 | — |

В данных (485 CTL): тип 1 ×59, 2 ×119, 3 ×42, 4 ×50, 5 ×38, 8 ×71, 9 ×65, 0xA ×30, 0xC ×56, 0xD ×39, 0xF ×63, 0x11 ×8, 0x13 ×129, 0x14 ×54, 0x15 ×70,
0x16 ×1, 0x18 ×4, 0x19 ×24, 0x1A ×30, 0x1B ×42, 0x1D ×2, 0x1E ×6.

Назначение типов: 0x01 привод башни, 0x02 снаряд (орудие), 0x03 опора подвижности, 0x04 наведение, 0x05 двигатель, 0x08 радар, 0x09 силовой щит, 0x0A детектор-щит,
0x0F ремонт, 0x13 батарея, 0x14 колёсный блок, 0x15 дефлектор, 0x1B броня.

### 5.2 Запись предмета CTL (`CtlRawItem`)

| Смещ. | Поле | Смысл |
|---|---|---|
| +0x00 | `item_type` | |
| +0x04 | `local_piece_index` | |
| +0x08 | `flags` | режим выхода и сигналы (§5.5) |
| +0x0C/+0x10/+0x14 | int | у 0x02/0x1E: −1 или 1/6/2, +0x0C = список команд такта 3; у 0x19: 9..13 = номера состояний (`method_21b80` с +0x10 при включении, +0x14 при выключении) |
| +0x18 | начальный `runtime_state` | −1 = не менять; у 0x18 = 33, у 0x1D = 9, у 0x19 = 0 |
| +0x1C | float (`Param1C`) | 0 или кратно 12.5 (двигатели 450..1350, радары 175..1150, щиты 200..290, батареи 175..700); читателя нет |
| +0x20 | `Param20` (селектор 0x30) | базовая мощность в секунду; у батареи — максимум выдачи |
| +0x24 | `Param24` | у оружия копируется в +0x9C; назначение не выяснено |
| +0x28 / +0x2C | `SignalAWeight` / `SignalBWeight` | веса сигналов A и B (колёса 0.5 или 1.0) |
| +0x30 | `common_params[16]` | CP0..CP15 |
| +0x70 | `Resource` | архив + имя (щит → FXID, оружие → BULL) |
| +0xB0 | число привязок, далее индексы привязок и имя предмета | |

### 5.3 Vtable `IItem`

| Слот | Смещ. | Метод | Назначение |
|---|---|---|---|
| 0 | 0x00 | delete | |
| 1 | 0x04 | `ParseCtlItemRecordAndInit` | разбор записи, копирует `common_params`, вызывает слот 2 |
| 2 | 0x08 | `InitAfterLoad` | пост-инициализация: ForceShield — `LoadShieldEffects`; Repair — `DetectHallwayOwner`; ProjectileWeapon — 0x10023FE0 (читает BULL); Targeting — создаёт камеру (0x10022A60); Guidance — кэш `cos(CP0)` в +0x98 (0x10026280); иначе `noop` (0x1002B460) |
| 3 | 0x0C | `GetEffectHandleRange` | диапазон данных эффекта (у щита — два `char[32]`: archive/resource) |
| 4 | 0x10 | `SetItemResourceRef` | ресурс предмета; вызывает слот 2, ставит `NetSynchroMode \|= 0x400000`; по умолчанию `noop` (0x100097C0) |
| 5 | 0x14 | `GetPowerDelta(dt)` | `−dt·Param20` при `runtime_state != 0` и жизни > 0; отрицательное — потребление, положительное — выработка |
| 6 | 0x18 | `ApplyPowerRatio(dt, ratio)` | получает долю выданной энергии (кладёт `item_factor`) |
| 7 | 0x1C | `UpdateRuntime(time_ms)` | `state_transition_time` = (t − start) / длительность, до 1 |
| 8 | 0x20 | `AdvanceBindingTimeline(dt_ms)` | целевые значения каналов анимации из сигналов управления |
| 9 | 0x24 | `SetRuntimeState(state)` | меняет `runtime_state`, сетевой флаг 0x10000; у оружия 0x1000 = «выстрел» |
| 10 | 0x28 | `GetPrimaryBindingValueOffset` | заглушка 0.0 (0x10011650) |
| 11 | 0x2C | `GetNetSyncDirtyAsFloat` | 1.0, если `NetSynchroMode & 0xFFF0000` |
| 12, 13 | 0x30, 0x34 | Serialize / DeserializeToNetwork | |

### 5.4 Распределение энергии

`CControlBase::UpdateItemRuntimeEffects` (0x10010180): у всех предметов вызывается слот 5; выработка и потребление группируются по категории типа
(`g_control_item_category_by_type`, 0x1003E1A0). `method_10c20` выдаёт энергию категориям по очереди; при нехватке слот 6 получает долю `min(1, доступно / нужно)`.

| Порядок | Категория (значение) | Типы | Бит |
|---|---|---|---|
| 1 | Movement/drive (3) | 5, 0x10, 0x14, 0x1A | 8 |
| 2 | Internal passive (0) | 0, 3, 6, 7, 0xB, 0xC, 0xD, 0xF, 0x12, 0x17, 0x19, 0x1C, 0x1D, 0x1F | 1 |
| 3 | Sensors (2) + Defense (5) | 4, 8, 0x11 / 9, 0xA, 0x15, 0x1B | 4 \| 32 |
| 4 | Weapon/actuator (4) | 1, 2, 0x16, 0x18, 0x1E | 16 |
| остаток | Power supply (1) | 0xE, 0x13 | 2 |

### 5.5 Флаги предмета (`method9`, `method_29050`)

| Биты | Смысл |
|---|---|
| 0..7 | режим выхода: 0 — интегрирование (скорость = A·wA + B·wB; прогресс 0..1 по `runtime_state & 3`: 1 вперёд, 2 назад; `& 0xC`: 4 — зацикливание, 8 — смена направления); иначе — значение сигнала с таким номером |
| 8..15 | сигнал A |
| 16..23 | сигнал B |
| 0x01000000 / 0x02000000 | левая / правая группа колёс |
| 0x04000000 | инверсия |
| 0x08000000 | признак привода башни (`QueryItemProperty(0x75)`) |
| 0x80000000 | пороговый режим: канал = 1, если значение ≥ default_value |

Номера сигналов: 0, 1 — константа (по умолчанию 1.0 для A, 0 для B); 2..4 — `NormSpeed.x/y/z`; 5..7 — `−NormSpeed.x/y/z`; 8..10 —
`InputEulerTarget[i] / visual_euler_angle_limit[i]`; 0xB..0xD — `TangSpeed[i] / max_tangential_speed[i]`; 0xE — `|TangSpeed| / |max_tangential_speed|`.

### 5.6 Параметры и свойства

`ResolveItemFloatParam(selector_and_flags)` (0x10021560) — главный читатель `common_params`; аргумент — enum `EControlItemFloatParam`, значения комбинируются через `|`.

| Значение | Имя | Смысл |
|---|---|---|
| 0x00..0x0F | `ITEM_PARAM_0..15` | `common_params[i]` |
| 0x10 | `ITEM_PARAM_LIFE_RATIO` | доля жизни детали |
| 0x20 | `ITEM_PARAM_ITEM_FACTOR` | `item_factor` |
| 0x30 | `ITEM_PARAM_BASE_POWER` | `Param20` |
| 0x100 | `ITEM_PARAM_X_LIFE` | × доля жизни (только 0..15) |
| 0x200 | `ITEM_PARAM_X_FACTOR` | × `item_factor` (только 0..15) |
| 0x400 | `ITEM_PARAM_IF_ACTIVE` | 0, если `runtime_state == 0` или жизнь ≤ 0 |

Часто встречаются `0x100`, `0x101`, `0x200`, `0x500` (`P0·жизнь`, только в активном состоянии: мощность двигателя), `0x10`, `0x30`.
`SetParam(param, value)` (0x10021660) пишет только `common_params[0..15]` и `item_factor`. `ResolveFloatParamOfSpecificItem(type, selector)` (0x10010E20)
суммирует по всем предметам типа.

**`item_factor`** — доля фактически выданной энергии (0..1); у батареи — уровень заряда (при загрузке `SetParam(0x20, 1.0)`).

### 5.7 Смысл `common_params` по классам

| Класс | Параметры |
|---|---|
| Батарея 0x13 | `Param20` = макс. выдача; CP0 = ёмкость (−1 — бесконечная). Выдача = min(жизнь·Param20·factor·dt, жизнь·CP0·factor); слот 6 уменьшает factor на выдача/(CP0·жизнь) |
| Ремонт 0xF | CP0 = скорость ремонта/с × жизнь; CP1 = цена энергии за единицу (0 — бесплатно). Потребление `−dt·Param20 − CP1·min(CP0·жизнь·dt, доступно)`; слот 6 → `DistributeNdpLifeDelta`; состояние 0x40 переключает 0x20 ↔ 0 |
| Силовой щит 0x09 | CP0 = ёмкость сектора; CP1 = скорость восстановления × жизнь; CP2 = цена энергии. 6 секторов; потребление `−dt·Param20 − CP2·min(CP1·жизнь·dt, CP0·(6 − Σсекторов))` |
| Детектор-щит 0x0A | состояния 0x1000 вкл, 0x2000 выкл, 0x4000 переключить; потребление `−dt·Param20 − CP4·dt` во включённом. CP0/1/2 ослабляют сигнатуры 0x45/0x43/0x44; CP3 — общее ослабление: `сигн × (1 − CPi·factor·жизнь) × (1 − CP3)`, каждый множитель ≥ 0. При нехватке энергии → 0x2000 |
| Радар 0x08 | CP0/1/2 — чувствительность каналов (биты 0x400000/0x100000/0x200000 → сигнатуры 0x45/0x43/0x44); CP3 — радиус обнаружения (свойство 0x50); CP4 — период пересканирования. Обнаружено, если `CPi·factor·жизнь·сигн·(1 − d²/R²) ≥ 1` |
| Оружие 0x02/0x1E | см. §7.2 |
| Привод башни 0x01 | сам читает только `LIFE_RATIO`; у присоединённых орудий берёт CP6, CP7 для баллистики |
| Наведение 0x04 | CP0 = 0.1, CP1 = 1000, CP2 = 1.3, CP3 = 1, CP4 = 150, CP5 = 1 (одинаково у всех 50). `InitAfterLoad` создаёт `LOGIC_CAMERA`, CP0..2 → `ICamera`; смысл не выяснен |
| Guidance 0x11 | CP0 — угол; кэш `cos(CP0)` в +0x98 |
| Тип 0x06 | CP0·жизнь и CP1·жизнь суммируются в свойствах 0x33/0x34, 0x3A = ΣCP1/ΣCP0; класс не выяснен |
| Тип 0x16 | решатель упреждения (`TryComputeInterceptPoint`), `common_params` не читает |
| Дефлектор 0x15 | CP0..5 — коэффициенты шести секторов щита (0.36..1.0): поглощение = `заряд·CP[i]·factor·жизнь`; сектор по доминирующей локальной оси: 0 +Y, 1 −Y, 2 −X, 3 +X, 4 +Z, 5 −Z |
| Броня 0x1B | CP0 = 0..30, CP1 = 0.22..1.0 (делитель): эффективная жизнь = жизнь / CP1 (0 → `FLT_MAX`); `ResolveItemFloatParam(0,1,2)` → `ControlItemDescriptor` (+0xC, +0x4, +0x8) |
| Двигатель 0x05 | CP0 = 0.7..1.0; мощность (свойство 0x79) = `CP0·жизнь·Param20` (`0x500`); свойство 0x7E агента = Σ`Param20`; множитель скорости = Σ`ResolveFloatParamOfSpecificItem(Engine, 0x500)` × средняя целостность ведущих групп. 0x1A: +0xA0 — доп. потребление (`GetPowerDelta = −(Param20 + field_0xA0)·dt`) |

### 5.8 Сигнатуры юнита

Три числа (свойства `GetBaseProperty`), по которым радары обнаруживают цель, а детектор-щиты их ослабляют. Физический смысл (оптика/тепло/радио) по коду не выводится.

| Свойство | Из чего | Канал радара | Параметр |
|---|---|---|---|
| 0x45 | `noncentral_mobility_contribution` | 0x400000 | CP0 |
| 0x43 | `derived_force_or_consumption_x × noncentral_scaled_xy_area` | 0x100000 | CP1 |
| 0x44 | `derived_force_or_consumption_y × noncentral_scaled_volume` | 0x200000 | CP2 |

### 5.9 `QueryItemProperty` (`IItemManager`, 0x1000ECE0)

| Свойство | Значение |
|---|---|
| 0x54 | CP7 — скорость снаряда |
| 0x55 | `round(CP1)` — боезапас |
| 0x62 / 0x63 | `item_factor` / `Param20` |
| 0x65 / 0x66 | частота стрельбы (`GetPrimaryFireRate` / `GetSecondaryFireRate`) |
| 0x68 | CP3 (объём снаряда) |
| 0x70 | длительность текущего перехода, мс |
| 0x75 | признак привода башни |
| 0x76 | CP11 (радиус поражения) |
| 0x77 | ремонт/щиты |
| 0x78 | среднее `(CP0+CP1+CP2)·factor·жизнь/3` детектор-щита |
| 0x79 | мощность двигателя / поглощение дефлектора |

## 6. Щит, дефлектор, радар

Методы `ILifeSystem` агента (у `CControlBULL` и `CControlClassic` одни реализации; префикс `ILifeSystem_of_CControlBULL` взят из типа `this`):

| Слот | Адрес | Метод | Что делает |
|---|---|---|---|
| 9 | 0x10013220 | `ScanRadarTargets(filter, channel_mask, out_flags, out_handles)` | делегирует `CItemType8_SensorRadar::ScanTargets`; возвращает число целей |
| 17 | 0x10013290 | `EvaluateRadarDetectionOfObject(target, channel_mask)` | вектор от центра сферы до цели → `EvaluateTargetDetection`; маска обнаруживших каналов |
| 18 | 0x10013410 | `ProcessShieldHitAtPoint(world_point, out_local_dir, out_absorbed)` | выбирает сектор, пишет `заряд·CP[i]·factor·жизнь`, при > 0 вызывает `PlayHitEffect`; возвращает индекс сектора или −1 |
| 19 | 0x10013950 | `DepleteShieldSector(sector, msg)` | шлёт `IBehaviour` команду 0x19 и обнуляет заряд (`AddSectorChargeDelta(−заряд, record=true)`) |
| 20 | 0x100139A0 | `SerializeSelf()` | в режиме 2 собирает дельты секторов и жизни NDP в сообщение `0x80000023` |

**Эффективная жизнь:** `ComputeEffectiveLifeWithDefenses(use_max)` (0x10017F80) = `жизнь/Armour.CP1 + щит.CP0·Σдефлектор.CP[0..5]` (максимум)
либо `… + Σ(уровень_сектора)·щит.CP0·Σ(CP[i]·factor·жизнь)/6` (текущее). `ComputeEffectiveVolumeWithDefenses` (0x100181B0) — то же для объёма.

**Поля `CItemType9_ForceShield`:** +0x98 `sector_levels[6]` (0..1), +0xB0 `pending_sector_delta[6]`, +0xC8 `changed_sector_mask`, +0xCC..0xD4 три хэндла эффектов (−1 — нет),
+0xD8 индекс следующего эффекта, +0xDC `archive_name[32]`, +0xFC `resource_name[32]`.

**`PlayHitEffect(world_transform, scale)`** (0x100282A0, два аргумента): по кольцу (`next_effect_index % 3`) запускает один из трёх эффектов —
`SetEffectTransform(SPACE_WORLD)`, `SetEffectScale(scale ×3)`, `StartEffect`; `scale` — радиус сферы владельца. `LoadShieldEffects` назначает хэндл
`0x20000000 + слот + 1` при первой загрузке и `прежний + 1` при перезагрузке; при повторной загрузке слот 0 получает номер слота 1, и `RemoveEffect(старый)`
следующего слота может снять только что созданный эффект (не проверено).

**Эффект щита** (проверено): FXID `f_shield_{b,g,r,y}` (здания: `fortif.rlb` 27, `intsys.rlb` 1) и `r_shield_{b,g,r,y}` (роботы: `bases.rlb` 18, `intsys.rlb` 19).
Все 8 файлов по 416 байт: заголовок (`Duration` 1.0 с, источник из файла 0, но `StartEffect(handle, 1)` подменяет на Life) + компонент типа 9 (плоскость WORLD,
материал `shield_{r,g,b,y}`) + компонент типа 2 (звук `hit_shld.wav`). Геометрия в файле постоянна; волну рисует материал `MAT0 SHIELD_*`: 10 стадий текстуры
`EFFECT5.0` (страницы 0..9, кольца), цвет задаёт Ambient. Кадр выбирается по **фазе** (= жизнь эффекта 0..1), поэтому один удар = 1 с = один проход по 10 кадрам
(≈ 0.1 с/кадр); три экземпляра нужны только для перекрытия подряд идущих ударов.

## 7. Оружие и эффекты выстрела

Ниже — полная цепочка выстрела; её воспроизводит панель `Weapon Viewer` (`NResUI/EffectPlayer/WeaponCatalog.cs`, `WeaponViewerPanel.cs`).

### 7.1 Автомат выстрела

`SetRuntimeState(0x1000)` (0x100249C0) — команда «выстрел»: `state_transition_mode = 2`, `state_transition_time = 0`, +0x170 = CP9 (время жизни).
`AdvanceShotStateMachine(binding_index)` (0x10024A20) вызывается по окончании перехода канала; счётчик такта по привязке (массив +0x124) идёт 4 → 0.
Длительность перехода: `|Δ| · 1000 / (speed_scale · ChannelSpeed)` мс (`ExtendTransitionEndTimeForBinding`, 0x10021AC0).

| Такт | Действие | Длительность, с | Значение канала (источник `AnimParam`) |
|---|---|---|---|
| 3 | `ExecuteCommandListForItem(+0x0C предмета)` — обычно START_EFFECT звука | 0.5 / `ChannelSpeed` (0.125 при 4.0) | 0 → 0.5 |
| 2 | `SpawnProjectile` | CP2 мс (пауза) | 0.5 |
| 1 | канал → 1.0 | 0.5 / `ChannelSpeed` | 0.5 → 1.0 |
| 0 | сброс канала в 0, боезапас −1 (+0x98, если не −1 и не 0) | — | 0 |

Эффекты вспышки, созданные у детали орудия (`gunf_*`, `gun_*_fx`: `duration = 0`, источник `AnimParam`), видны, пока значение канала в окне 0.01..1.0:
≈ `2·0.5/Param24 + CP2` секунд на выстрел, затем пропадают. Снаряд появляется через `0.5/Param24` после команды огня.
`SpawnProjectile(muzzle_binding_record)` (0x10024C60): строит ориентацию дула, `CreateObject(LOGIC_BULLET_AGENT_BULL, …)`, задаёт скорость и свойства
(`IControl::SetBaseProperty`), `AddObjectToGame`, запускает эффект выстрела.
`ResetShotState()` (0x10024A00) вызывается из `CItemType1_TurretDrive::SyncTargetToAttachedWeapons` (0x1002C610): копирует в орудие цель
(+0x98/+0x9C привода → +0x10C/+0x110), углы и сбрасывает автомат.

### 7.2 Параметры оружия (тип 0x02 / 0x1E)

| CP | Смысл |
|---|---|
| 0 | задержка между выстрелами, мс: `GetPrimaryFireRate = 1/(1/anim_rate + CP0·0.001)` (0x10025340, свойство 0x65) |
| 1 | боезапас (−1 — бесконечный, свойство 0x55) |
| 2 | пауза такта 2, мс (`anim_channel_transition_end_time_ms = CP2 + start_time_ms`) |
| 4 | `> 1.0` ставит флаг +0x16C (назначение не выяснено) |
| 5 | вторая задержка, как CP0 (`GetSecondaryFireRate`, 0x10025380, свойство 0x66) |
| 3, 6..11 | **не из CTL**: записываются в `InitAfterLoad` из снаряда-прототипа BULL (значения CTL затираются): CP3 — объём (0x68); CP6 — 1, если `movement_physics_mode != 0` (гравитация); CP7 — `max_tangential_speed.y` (скорость вылета, 0x54); CP8 — `min(BULL 0x81, field12_0x58)` (предел дальности, не проверено); CP9 — время жизни `BULL 0x80 × 0.001` с (+0x170); CP10 — `cos(BULL 0x82)` (−1 — не задано); CP11 — радиус поражения (`NDP_DAMAGE_EFFECT_BOUNDS_RADIUS_ALL`, 0x76) |

### 7.3 Цепочка «орудие → снаряд → эффекты»

Проверено по CTL guns.rlb (`o_gun_*`, 55 орудий) и weapon.rlb.

| Звено | Откуда | Что в нём |
|---|---|---|
| CTL орудия | `guns.rlb`, `o_gun_*.ctl` (+ `.cpt` рядом) | предмет 0x02: `Resource` = `objects.rlb` / BULL; CP0 пауза, CP1 боезапас; список 0 (`on_load`): SPAWN_* (код 4) вспышки (`gunf_laser_r`, `gun_la01_fx`, …) и звука (`gun_las_02_sfx`); иногда список 1 — дым |
| BULL | `objects.rlb` | ссылка 4 → `weapon.rlb` / `<имя>.ctl`; ссылка 2 → CPT снаряда |
| CTL снаряда | `weapon.rlb` | `RootParams.MaxTangentialSpeed.Y` — скорость; `DetachCleanupDelayMs` (у лазеров 1000); список 0: SPAWN_* эффектов полёта; списки 1..3 — состояния (ниже) |
| EXPL | `weapon.rlb`, имя в команде 27 | слот 0 и слоты 1..11 по материалу поверхности 0..10 (см. `ExplLib`) |

Эффекты полёта по типам снарядов: лазер `las_tail_{r,g,b,h,v}` (шлейф), пуля `b_bullight_*` / `b_raillight_*`, ракета `engine_mis_*` + `smoke_ms_*`/`smoke_rk_*` + `gunf_missile_*`,
огнемёт `flamer_01`, тазер `tas_tail`, строитель `builder_tail`. Скорости: лазер/тазер 10000, пули 250..3000, рельса 500, огнемёт 40, ракета 45 (ускорение 10).

**Состояния снаряда** (слоты таблицы хуков 2, 3, 4; список выполняется при входе в состояние, `CControlBULL::UpdateTick`, 0x1000DBF0):

| Список | Типичное содержимое |
|---|---|
| 1 | эффект полёта: DEACTIVATE (шарики, local_id 1) или START_EFFECT (лазер, local_id 0, Life); затем FORCE_KILL (17) |
| 2 | то же + команда 27 (взрыв EXPL) |
| 3 | то же + SCHEDULE_DETACH_OR_KILL (15, удаление через `DetachCleanupDelayMs`) |

**Лазерный выстрел:** вспышка `gunf_laser_*` + звук у дула; `las_tail_*` создаётся с источником Constant = 0 (окно 0..1, значение 1 → луч виден целиком от дула
до снаряда); при попадании START_EFFECT с Life — значение падает 1 → 0 за 0.75 с (луч затухает на месте); снаряд живёт ещё 1000 мс; в точке попадания `exp_*_las_*` по материалу.

**Многоствольные орудия** (`o_gun_ba_23`: три ствола): у предмета несколько привязок (по одной на ствол), у каждой своя CPT дула (`PrimaryCptRefIndex`, иначе `SecondaryCptRefIndex`: CPT 0, 3, 6) и свой автомат выстрела (`AdvanceShotStateMachine(binding_index)`); эффект вспышки на каждый ствол создаётся отдельной командой 4 с local_id 0, 1, 2, а список команд предмета запускает их все. Снаряд рождается в положении CPT дула своей привязки, поэтому шлейф и взрыв идут от соответствующего ствола. Режим стрельбы определяет флаг предмета `0x02000000` (`CONTROL_ITEM_FLAG_RIGHT_DRIVE_GROUP`) в `CItemType2_and_0x1e_ProjectileWeapon::AdvanceBindingTimeline` (0x100244D0): с флагом автомат выстрела проходит по **всем** привязкам сразу (залп; у `o_gun_ba_21..23`, `o_gun_ma_21/22` и у большинства лазеров), без флага стреляет одна привязка за раз по кругу (`field12_0x120` = следующая; `o_gun_fa_09`: 9 труб). Привязки с флагом `0x40` (`COPY_PREVIOUS_VALUE`) пропускаются. Weapon Viewer по умолчанию показывает залп или один ствол соответственно, выбор «Ствол» переключает. Без флага `AdvanceBindingTimeline` стреляет привязкой `current_binding_index`; когда автомат её выстрела завершает такт 0 (возвращает true), индекс переходит на `(i + 1) % число_привязок_предмета`, пропуская привязки с `COPY_PREVIOUS_VALUE` (0x40). Порядок стволов = порядок `piece_binding_refs` в записи предмета CTL (`CItemBase::ParseCtlItemRecordAndInit`), первый выстрел - привязкой 0.

### 7.4 Размещение эффектов по CPT и масштаб

Команда 4 строит матрицу эффекта из трёх CPT: позиция = среднее позиций.

| Направления трёх CPT | Матрица |
|---|---|
| все равны | `BuildFromDirection(dir/|dir|)`, масштаб `|dir|` |
| два равны | `BuildBasisFromPrimaryAxis` |
| все различны | **колонки матрицы = векторы направлений**: их длина масштабирует весь эффект |

В данных: орудия `|dir|` 0.05..0.3 (`o_gun_ta_03` 0.05, `o_gun_ma_20` 0.11), снаряды 0.02..1 (`bb_t_01` 0.02, `bb_l_01` 0.03, `bm_m_04` 0.2, `bl_*` 1.0).
Оси: у орудий +X эффекта = +Y орудия (вперёд), у пуль +X эффекта = −Y пули (назад, поэтому шлейфы и факелы тянутся за снарядом).
Пример: `b_bullight_r` длиной 350 у `bb_t_01` на деле 350 × 0.02 = 7 ед. длиной и 0.06 шириной. Масштаб заголовка FXID (+0x30; ≠ 1.0 у 134 из 476 файлов,
например `gunf_laser_*` = 50, `gun_la01_fx` = 1.25, `gunf_missile_b` = 3) умножается на то же; к шлейфам не применяется.

### 7.5 Взрыв при попадании

Команда 27 (`SWAP_NDP_RESOURCE`, 0x100028A0) подставляет EXPL детали снаряда и вызывает `CommitNdpDamageStateTransition` (0x10015B50).
Масштаб взрыва и радиус урона по площади:

| Объект | Масштаб |
|---|---|
| снаряд (`LOGIC_BULLET_AGENT_BULL`) | `EXPL+0x08` как есть: `bb_*` 1.0, `bm_t_01r` 3.0, `br_b_01r` 7.0, `bm_m_04r` 60.0 |
| остальные | радиус сферы детали × `EXPL+0x08` |

Эффект ставится в `SPACE_WORLD` в центр сферы детали, ось по режиму `EXPL+0x14`, все размеры компонентов FXID умножаются на масштаб
(`SetEffectScale` ещё умножает на масштаб заголовка FXID). Общий `exp_m_mis` (`expl5` 1 → 5) даёт 15 ед. у `bm_t_01r` и до 300 ед. у `bm_m_04r`
(EXPL тип 3, урон 15000 — вероятно, стратегическая ракета). `ComputeNdpDamageEffectBoundsRadius` (0x10017D90) использует то же поле.

### 7.6 Привод башни (`CItemType1_TurretDrive`)

| Метод | Адрес | Что делает |
|---|---|---|
| `ResolveAimMode` | 0x1002CB10 | `runtime_state` 0x400/0x800 → слежение: 0x800 — точка далеко вдоль ствола (×1e6), иначе `TryComputeInterceptPoint`; при неудаче каналы по умолчанию (0x100). Возвращает 0x100/0x400 |
| `AdvanceBindingTimeline` | 0x1002B8B0 | режимы 0x100/0x200 берут цели каналов 0/1 (азимут/угол места) из ввода (+0xA0, инверсия 0x04000000); 0x400 считает углы по точке цели |
| `StepChannelTowardTarget` | 0x1002CD30 | ограничение скорости канала, циклические каналы |
| `SolveBallisticElevation` | 0x1002C6C0 | для каналов ≥ 2 (орудия): квадратное уравнение времени полёта с `g·CP6` и скоростью CP7, множитель 0.83 |
| `SyncTargetToAttachedWeapons` | 0x1002C610 | копирует цель и углы в орудия, `ResetShotState` |
| `IsTargetDetectedAndVisible` | 0x1002ABD0 | цель обнаружена радаром (слот 17) и нет пересечения луча (маска 0x41A) |
| `InitAfterLoad` | 0x1002B5A0 | начальные углы каналов, списки орудий (`unknown_vector_0xd8`) и приводов 0x18 (`unknown_vector_0xe8`) по каналам ≥ 2 |

База `CItemType0x16` (ctor 0x1002B470 вызывается из ctor привода) даёт `TryComputeInterceptPoint` (0x1002A470), поля цели +0x98/+0xA0/+0xAC
и `SerializeToNetwork` (0x1002AAF0, биты `NetSynchroMode` 0x40000/0x80000/0x100000).

### 7.7 Модель орудия: система координат CPT, поза, страницы текстур

| Что | Как устроено |
|---|---|
| Система координат CPT | положение и направление CPT заданы в системе детали-владельца (`OwnerPieceIndex`; локальный индекс детали = индекс узла в MSH орудия). Положение дула в объекте = положение CPT × матрица этой детали в позе |
| Поза деталей | деталь с привязкой стоит на анимации в момент `Anim2Start + default · (Anim2End − Anim2Start)` (Anim2 не задан - Anim1); остальные - на ключе по умолчанию. `o_gun_ba_23`: привязка детали 2 (подъём ствола), диапазон 1..3, значение 0.111 → время 1.222: ствол горизонтален; ключ по умолчанию (≈ 80°) смотрит вверх |
| Страницы текстур | UV модели заданы относительно страницы стадии материала (`TextureStageIndex` → прямоугольник TEXM): итоговый UV = `page.xy + uv · page.size` в долях текстуры |
| Кадры материала | стадии модели - кадры анимации материала, а не варианты цвета клана; подробности - §7.8 |

### 7.8 Выбор стадии материала у модели

Какую стадию (страницу текстуры) материала рисует деталь, решают три независимых механизма; выбирает их `CShade::SubmitMeshPieceBatches` (Terrain 0x1002A2B0):

| Механизм | Условие | Как выбирается стадия |
|---|---|---|
| Время | у батча MSH 0x0D нет флага 0x4 | `MatManager::AnimateFrame(материал, material_animation_index)`: позиция = `(g_game_time − lastUsedTime) % длительность` по ключам анимации (loop 0 цикл, 1 ping-pong, 2 один раз, 3 случайно) |
| Фаза детали | у батча флаг 0x4 | `GetMaterialPhase(материал, material_animation_index, phase)`, `phase` = `CAniMesh.pieces[i].material_phase_0x128` (0..1); пишется `IAnimation::SetPieceMaterialPhase` из `CItemBase::ApplyInterpolatedAnimChannelValues`, если у привязки канала флаг 0x10 |
| Индекс анимации | всегда | `material_animation_index` = `CControlBase.material_animation_index` (+0x550, слот ILifeSystem 0x40 `SetMaterialAnimationIndex`, читает `CAniMesh::Render` через слот 0x3C); значение вне `0..animCount−1` заменяется на 0 |

Клан хранится отдельно: `CControlBase.clan_index` (+0x54C), пишет `SetClanIndex` (слот ILifeSystem 0x28; `Behaviour::SetClan`, Behavior.dll 0x100094A0), 0xFFFE = без клана не перезаписывается. Оба поля реплицируются по сети парой (`IScalable_of_CControlBase::SerializeSelf` 0x1001B9B0, бит `NetSynchroMode` 0x400: `clan_index`, `material_animation_index`, затем `spawned_object_command_refs`). Что именно пишет вызов слота 0x40 и связан ли +0x550 с кланом, по коду не установлено: вызывающий не найден (`ArealMap!CreateObjectFromScheme` 0x10015780 и `Behaviour::SetProperty` 0x1000ADB0/`SetClan` слот не вызывают).

Данные миссий (29 `.tma`, 796 объектов): в записи объекта нет поля, похожего на индекс анимации материала (настройки: `Invulnerability`, `Life state`, `LogicalID`, `ClanID`, `Type`, `MaxSpeedPercent`, `MaximumOre`, `CurrentOre`…; `UnknownInt4` = 0/1, `UnknownInt5/6` = -1 у 779 из 796); кланов в миссии 1..5, `OwningClanIndex` = -1..4. Это не противоречит гипотезе «индекс = клан» (вариантов у `R_LBL_01` восемь), но и не доказывает её.

Известные слоты `ILifeSystem` (vtable `CControlBULL`, Control.dll 0x1003D51C):

| Смещение | Метод |
|---|---|
| 0x0C / 0x10 | `QueryNdpLifeStateRatio` (0x100116C0) / `SetNdpLifeStateRatio` (0x10011700) |
| 0x14 / 0x18 | `GetBaseProperty` / `SetBaseProperty` |
| 0x1C | `Kill` |
| 0x28 / 0x2C | `SetClanIndex` (0x10013260) / `GetClanIndex` (0x10009800) |
| 0x3C / 0x40 | `GetMaterialAnimationIndex` (0x10009810) / `SetMaterialAnimationIndex` (0x10009820) |
| 0x50 / 0x54 | `SerializeSelf` / `QueryBoundingSphere` |

`GMSG` 0x80000023 (`CControlClassic::SendMsg` 0x10034330; приходит через `Queue::ExecuteCommand`, World3D 0x10006C40) - пакет урона из сети: маска в 1-м слове; бит 0x1 - маска секторов щита и по дельте заряда на каждый установленный бит (`AddSectorChargeDelta`, ×`ITEM_PARAM_0` щита), бит 0x10 - пары (индекс NDP, дельта жизни) до -1 (`ApplyNdpLifeDelta` + `CommitNdpDamageStateTransition`).

Данные (Material.lib, 628 материалов; WEA орудий в `guns.rlb`):

| Материал | Стадии / анимации | Смысл |
|---|---|---|
| `R_GB50`, `R_GB51`, `R_GB52` | 4 стадии (страницы 10..13, 14..17, 18..21 `NP10.0`), 1 анимация, ключи 50/100/150/200 мс, loop 0 | 4 кадра свечения ствола одного цвета (оранжевый / зелёный / синий ряд текстуры), цикл 200 мс; `R_GB52` стоит в WEA `o_gun_ba_09`, `o_gun_ba_23`, `o_gun_fa_05`, `o_gun_fa_07` |
| `R_GB16` | то же, loop 1 (ping-pong) | то же |
| `R_LBL_01`, `B_LBL_01` | 8 стадий (страницы `PG27.0` 0,6,5,4,3,2,1,7), 8 анимаций по одному ключу | выбор одного из 8 вариантов индексом анимации; материал у `r_l_04`, `r_m_04`, `r_b_04` (метка) |
| 43 материала с 2 стадиями и 2 анимациями (ландшафт `L00`..`L40`) | 2 стадии, 2 анимации | вариант по индексу анимации |
| остальные 583 материала | 1 анимация (стадий 1..29) | статические |

Флаг привязки 0x10 в данных стоит только у 12 привязок в CTL `r_l_04`, `r_m_04`, `r_b_04`, то есть у моделей с `R_LBL_01`; у орудий его нет (их привязки 0x08 «прикрепить к предмету» и 0x48).

## 8. Имена в Ghidra

Имена применены в Control.dll (`set_function_this_type` переносит функцию в класс, затем простое имя).

| Адрес | Имя |
|---|---|
| 0x10013220 / 0x10013290 / 0x10013410 / 0x10013950 / 0x100139A0 | `ILifeSystem_of_CControlBULL::` `ScanRadarTargets` / `EvaluateRadarDetectionOfObject` / `ProcessShieldHitAtPoint` / `DepleteShieldSector` / `SerializeSelf` |
| 0x10025DB0 / 0x100260D0 | `CItemType8_SensorRadar::ScanTargets` / `EvaluateTargetDetection` |
| 0x10022A60 / 0x100225B0 / 0x100225F0 / 0x10022BB0 | `CItemType4_TargetingSystem::` `InitAfterLoad` / `delete` / `AdvanceBindingTimeline` / `SetRuntimeState` |
| 0x10026280 | `CItemType0x11_Guidance::InitAfterLoad` |
| 0x10025340 / 0x10025380 | `…ProjectileWeapon::GetPrimaryFireRate` / `GetSecondaryFireRate` |
| 0x10024A00 / 0x10024A20 / 0x10024C60 / 0x100249C0 | `…ProjectileWeapon::` `ResetShotState` / `AdvanceShotStateMachine` / `SpawnProjectile` / `SetRuntimeState` |
| 0x10023EE0 / 0x10024240 / 0x100243E0 | `…ProjectileWeapon::` `delete` / `GetEffectHandleRange` (имена BULL +0x128/+0x148) / `DeserializeFromNetwork` (боезапас +0x98; на 0 → состояние 0x200) |
| 0x10011660 / 0x10023510 | `CItemType0x1a_EngineAlt::GetPowerDelta` / `CItemType5_Engine::GetPowerDelta` (потребление ∝ `max\|TangSpeed\|/max(max_tangential_speed)` × `Param20`) |
| 0x10017F80 / 0x100181B0 | `CControlBase::ComputeEffectiveLifeWithDefenses` / `CControlClassic::ComputeEffectiveVolumeWithDefenses` |
| 0x100282A0 | `CItemType9_ForceShield::PlayHitEffect` |
| 0x1002A470 / 0x1002A3F0 / 0x1002AAF0 | `CItemType0x16::` `TryComputeInterceptPoint` / `GetOwnerBoundingSphereCenter` / `SerializeToNetwork` |
| 0x1002CB10 / 0x1002CD30 / 0x1002C610 / 0x1002C6C0 / 0x1002ABD0 / 0x1002B5A0 | `CItemType1_TurretDrive::` `ResolveAimMode` / `StepChannelTowardTarget` / `SyncTargetToAttachedWeapons` / `SolveBallisticElevation` / `IsTargetDetectedAndVisible` / `InitAfterLoad` |
| 0x10021AC0 / 0x10021B80 / 0x10021BD0 / 0x10025CD0 | `CItemBase::` `ExtendTransitionEndTimeForBinding` / `ExecuteCommandListForItem` / `ResolveObjectPieceWorldPoint` / `GetPieceAnimChannelForBinding` |
| 0x10013260 / 0x10009800 / 0x10009820 / 0x10009810 | `SetClanIndex` (слот ILifeSystem 0x28, поле `clan_index` +0x54C) / `GetClanIndex` / `SetMaterialAnimationIndex` / `GetMaterialAnimationIndex` (поле `material_animation_index` +0x550) |
| 0x10021290 | `CItemBase::ApplyInterpolatedAnimChannelValues` (флаг привязки 0x10 → фаза материала детали) |
| 0x100028A0 / 0x10015B50 / 0x10017D90 | `CControlBase::` `ExecuteControlCommandList` / `CommitNdpDamageStateTransition` / `ComputeNdpDamageEffectBoundsRadius` |
| 0x1000DBF0 / 0x1000E3D0 | `CControlBULL::` `UpdateTick` / `frame_update` |
| 0x100234E0 / 0x100253C0 / 0x1002D610 / 0x10025B20 | свободные: `PowIfBaseNotOne` (`base^exponent`, либо `exponent` при `base == 1.0`; затухание дрожания камеры) / `Vector3_DotProduct` / `ProjectPointOntoPlane` / `BuildPlacementFromDirection` |

Связанные имена в других библиотеках (подробнее в `FxidLib/README.md`): `Effect.dll` — `FxManager::ComputeEffectStrength` (0x10005F40), `EffectType3::RenderContents`
(0x10011380), `EffectType5::Render` (0x10002DA0), `EffectType8::EvaluateAtTime` (0x100123B0), `EffectContents::Draw` (0x10009FE0); `World3D.dll` —
`LoadMaterialByName` (0x10004630), `MatManager::GetMaterialPhase` (0x10003930), `MatManager::AnimateFrame` (0x10003490); `AniMesh.dll` — `CAniMesh::SetPieceMaterialPhase` (0x10005AE0, `IAnimation` слот 0x2C).

**Структуры и поля, названные позднее (Ghidra):**

| Библиотека | Тип | Что названо |
|---|---|---|
| Control.dll | `CItemType2_and_0x1e_ProjectileWeapon` | +0x98 `ammo_count`, +0x9C `charge_store`, +0x10C `target_object_handle`, +0x110 `target_piece_index`, +0x11C `ready_to_fire_maybe`, +0x120 `current_binding_index`, +0x124 `shot_tick_counters` (int[]), +0x168 `burst_remaining`, +0x16C `burst_mode_flag_maybe`, +0x170 `shot_time_remaining_sec` |
| Control.dll | `ExplData` (792 байта) | EXPL: `damage_kind`, `life_amount_offset`, `scale_multiplier`, `direction_mode`, `slot_default`, `slot_by_surface[11]` (используется в `ActiveDamageExplosion.expl_data`) |
| Control.dll | `RawControlPieceAnimChannelRecord` | +0x14 `anim_blend_weight`, +0x24 `channel_speed` |
| Effect.dll | `EffectRenderContext` | контекст отрисовки: `shade`, `mat_manager`, `parent_transform`, `camera_local_position` (+0x48), `camera_world_position`, `lod_level`, `fxid_flags`, `not_occluded` - типизирует `Render`/`RenderContents` типов 3, 4, 5, 7, 8, 9 |
| Effect.dll | `EffectType4FileData` | тип 3 + `reference_distance` (+0xC8) |
| Effect.dll | `EFxidEffectFlags`, `EFxManagerEnvFlags` | флаги заголовка FXID и `FxManager.environment_flags` |

## 9. Не выяснено

- Смысл CP0..CP2 камеры у `Targeting` (0x04) и флага +0x16C у оружия (CP4 > 1).
- Класс типа 0x06 (пара CP0/CP1 с отношением, свойства 0x33/0x34/0x3A).
- Физический смысл трёх сигнатур юнита (0x43/0x44/0x45).
- Потребители `CtlRawItem` +0x1C (`mobility_contribution`) и +0x24 (`param_24`: у оружия копируется в +0x9C).
- CP1+ у Engine, остальные методы `TurretDrive` (слот 14+), остальные слоты таблицы хуков команд.
- Как именно игра переводит значение канала анимации в `GetPieceAnimTransitionProgress` для `AnimParam` (форма кривой вспышки).
- Кто вызывает `SetMaterialAnimationIndex` (слот ILifeSystem 0x40) и какое значение пишет (связь с кланом, 8 вариантов `R_LBL_01`).
- Размер взрыва `bm_m_04r` (масштаб 60 → до 300 ед. в поперечнике): подтверждения по игре нет.
