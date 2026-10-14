# План реверса (Ghidra + C#)

Рабочий список: что уже разобрано, что в работе, что дальше. Обновляется по ходу. Правила: имена и структуры - в Ghidra, подтверждённые факты - в документацию
и C#; неподтверждённое помечается «не выяснено».

## Сделано

- ai.dll (SuperAI): шаг ИИ, проблемы, приказы, каталог схем и месторождения; скрипты `.scr`/`.fml` - `ScrLib`.
- ArealMap.dll: формат `Land.map` (`ArealMapLib`), граф Apex, сетка маршрутов `MRoutingGraph`/`HexGrid`, `MBrokenAreal`, проходимость `Flag0x20`.
- Behavior.dll: `Behaviour` (жизненный цикл, ресурсы, отношения кланов), `MWalker` (структура, порядок работы) - `docs/MWALKER.md`.
- AniMesh.dll/Control.dll: анимация MSH - общая шкала времени, клипы = состояния CTL (диапазоны, вес по скорости); плеер NResUI.
- Ngi32.dll (частично): NL-архивы и способы упаковки (`RsLiLib`), классы `NResFile`/`3DRender`/`CSoundPool`/`CListenerTransform`; из 144 безымянных функций осталось 98
  (в основном CRT 0x1002C000+ и растровые функции).
- services.dll (частично): менеджер ресурсов и формат `.cfg`, сеть (по именам), конфигурация - `docs/SERVICES.md`.
- World3D.dll (частично): очередь объектов и сеть, ввод и таблицы `.tbl` (`docs/WORLD3D.md`).
- Wizard.dll: арбитр управления объектом и ведение по очереди точек (`docs/WIZARD.md`); Distributor в Behavior.dll: энергия и руда клана (`docs/DISTRIBUTOR.md`).
- MisLoad.dll: формат `.tma` (`MissionTmaLib`: места миграции, привязка к зданию, настройки, залежи), передача ареалов и мест миграции в `SystemArealMap`.

## В работе

1. **`MWalker::BuildGlobalPath`** (Behavior.dll 0x10037640) - основной поток и вспомогательные функции разобраны (`docs/MWALKER.md`), `ApexPathfinder` создан;
   осталось: ветки по видам вершин, смысл полей `MWalkerPathPoint`, методы vtable `ApexPathfinder` (эвристика, стоимость рёбер).

## Дальше

2. **Ngi32.dll** - ядро движка: рендер, звук, сеть, NRes/RsLi, файлы, логгер. Назвать функции, описать структуры (`NResFile`, `Queue`, `Logger`...), свести с
   `NResLib`/`RsLiLib` и документацией формата.
3. **services.dll** (806 неименованных функций, в основном STL): остались дисплей, формат `.ini`, `CGUIServer`.
   **Net.dll / Effect.dll / Joystick.dll** в проекте Ghidra открываются только после обновления языка (`tools/upgrade_project_language.py`, нужен пароль сервера).
4. **Terrain.dll** (частично, `docs/TERRAIN.md`): `PlaceBuilding` и двери зданий разобраны; `CheckMaxBasementAngle` разобран; остались грани ландшафта (`GetWalkFace`,
   `FindNearestIntersectedFaceInLandscape`), вызывающий и порог проверки уклона (искали в ai/Behavior/World3D/Wizard - там нет; вероятно exe), рендеринг.
5. **World3D.dll** (остаток: материалы, рендер, `ManEntry`), **iron3d.dll**.
6. Нерешённое: читатели `Mask0x28`; откуда берутся опции `min_cell_class` и кто вызывает `MRoutingGraph::FindPath`; смысл значений `target_place`.

## Не делаем

- Вторую часть игры (IS2) не разбираем и не импортируем в проект Ghidra первой части.
