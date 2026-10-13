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

## Не выяснено

- внутреннее устройство `BuildGlobalPath` (функция большая, разобрана по строкам и вызовам);
- смысл значений в `target_place` кроме положения и в полях точек пути;
- откуда берутся значения опций `min_cell_class` при поиске по шестиугольной сетке (`ArealMapLib/README.md`).
