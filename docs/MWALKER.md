# MWalker: движение юнитов по ареалам (Behavior.dll)

Сводка по результатам реверса `Behavior.dll` (`MWalker`, поле `Behaviour.walker`, 0x168 байт). Имена функций и полей даны по Ghidra; всё, чего не
видно в коде, помечено «не выяснено».

## Состояние

| Смещение | Поле | Значение |
|---|---|---|
| +0x04 | `task_state` | `BehaviourTaskState` (36 байт) |
| +0x28 | `can_fly` | копия `CanFly` из `varset` (`BehaviourVarPart1+0xC`) |
| +0x2C | `stuck_counter` | счётчик тактов, пока юнит далеко от ожидаемой точки; при >1 путь сбрасывается |
| +0x54 | `target_accepted` | цель принята |
| +0x6C | `current_position` | точка, к которой идёт юнит; +0x88/+0x8C - логический ID объекта и номер узла (блокировка) |
| +0x90 | `previous_position` | предыдущая точка (+0xAC/+0xB0 - объект и узел) |
| +0xB8 | `target_place` | 9 `float`: положение цели и вспомогательные значения |
| +0xDC | `approach_radius` | 3 `float`: радиус подхода, ограничивается значениями из `varset` |
| +0xE8 | `global_route` | глобальный маршрут (точки по 0x24 байта) |
| +0xFC | `local_path` | локальный путь в обход препятствий (по 0x24) |
| +0x110 | `mover_segments` | отрезки для движителя (по 0x48, `MWalkerSegment`) |
| +0x164 | `recalc_requested` | нужно пересчитать путь |

## Порядок работы

1. `SetTarget`: цель переводится в ареал (`GetCoverArealID`); цель отвергается, если ареал не проходим (`Flag0x20 == 0`), а юнит не летает.
2. `BuildGlobalPath`: путь по графу ареалов; `BuildLocalPath`: обход зданий внутри ареала (контуры из `MBrokenAreal`).
3. `PushMoverSegments`/`PublishMoverSegments`: отрезки передаются движителю; `OnBehaviourTakt` по тактам выбирает следующую точку, `ClearMoverReachedPoints`
   убирает достигнутые.
4. `LeaveObstacleOrNonWalkable`: если юнит оказался в препятствии (виды Apex 9, 10) или в непроходимом ареале, он выводится в ближайшую допустимую точку
   (до 30 попыток на радиусе 30...500); сообщение «Warbot is absolutely in non-walkable».
5. Узлы проходов зданий блокируются по логическому ID юнита (`UnlockHallwayNode`, `UnlockAllHallwayNodes`, `IsNodeLockedByAlly`).

Поддерживается два режима: обычный и «визард» (`g_usewizard`): `OnTaktWizard`.

## BuildGlobalPath (Behavior.dll 0x10037640)

Проверено по декомпиляции; детали разбора веток по видам вершин не закончены.

1. Начало и цель переводятся в `ApexId`: если в `target_place[7]` не NaN, id берётся из него (объект/подзона), иначе `LocatePointInArealZones`
   (слот `ISystemArealMap` +0xC0). `GetApexNode` даёт узлы графа.
2. Владелец записывается в `apex_pathfinder` (+0x128 в `MWalker`, 0x3C байта): `CanFly` из `varset` и `IBehaviour` юнита. У летающих (`CanFly != 0`)
   поиска по графу нет: маршрут состоит из одной точки - цели.
3. `ApexPathfinder::FindPath` (A* по графу Apex, открытое множество - `MTree`, эвристика - слот vtable +0xC) складывает узлы пути в **статический**
   массив `DAT_10064f20` (поэтому функция не потокобезопасна).
4. По парам соседних узлов строятся точки `global_route` (`MWalkerPathPoint`, 0x24 байта) в зависимости от видов вершин (младшие 4 бита id: 1 ареал,
   2 подзона, 3/6/7 объект): координаты берутся у `ISystemArealMap` (слоты +0x1C, +0xD4) и у `IBehaviour` объекта (проходы зданий); при переходе
   через подзону с препятствием добавляется обход по периметру (`AppendSubArealBoundaryRoute`: вершины сдвигаются внутрь полигона по нормали).
5. В конец маршрута для наземных юнитов добавляется цель; если конечная вершина - подзона/объект, перед ней дописывается обход по периметру.
   Функция обёрнута в профилировочный счётчик (`rdtsc`, глобальный `DAT_10064f38`).
6. Ошибки пишутся в лог: `Bug in pathfinder 1..5` (несогласованные виды соседних вершин), `Bug in Global PathFinder`, `bCrosBadAreal` (отрезок
   пересекает проход здания - `SegmentTouchesBuildingHallway`).

Вспомогательные функции (названы в Ghidra): `Polygon2D_ContainsPoint` (чётно-нечётное правило, точки по 12 байт), `Segments2D_Intersect`,
`Segment2D_LineIntersectionParams`, `Vector2_NormalizeXY`, `SegmentTouchesBuildingHallway`, `AppendSubArealBoundaryRoute`.

## Не выяснено

- точное соответствие значений `target_place` и полей точек пути (`MWalkerPathPoint`) их смыслу;
- откуда берутся значения опций `min_cell_class` при поиске по шестиугольной сетке (`ArealMapLib/README.md`);
- эвристика и стоимость рёбер `ApexPathfinder` (методы vtable +0x8/+0xC не разобраны);
- что именно хранят узлы открытого множества (0x10 байт: флаг, родитель, левый и правый потомок).
