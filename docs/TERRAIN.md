# Terrain.dll: ландшафт, здания, атмосфера

Сводка по реверсу `Terrain.dll` (проект Ghidra `parkan1`, 1697 функций, из них безымянных 575 - в основном вспомогательные структуры, триангуляция и
растеризация). Описано только подтверждённое кодом, строками отладки или данными.

## Классы

| Класс | Назначение |
|---|---|
| `CLandscape` | ландшафт (`Land.msh`, `Land.map`): грани, поиск грани под точкой, врезка основания здания (`PlaceBuilding`) |
| `CTerrain` | поиск граней ландшафта (`FindFaceInFan`), `PlaceBasement` |
| `CBuilding` | здание: контуры основания (`BASOuterContour`/`BASInnerContour`, [BasLib](../BasLib/README.md)), двери, состояние, сфера строительства |
| `CCamera`, `CBufferingCamera`, `CameraFrustum` | камера и пирамида видимости |
| `CAtmosphere`, `CSun`, `CSky`, `CRain`, `CSnow`, `CLightning`, `CAtmData` | атмосфера по расписанию [SkeLib](../SkeLib/README.md) |
| `CShade`, `CShader`, `CPrimitive*`, `PrimLayer*` | рендеринг земли и объектов |
| `CLightManager`, `LightSource` | источники света |

Загрузчики по `Comp.ini`: `LoadLandscape` (CID 0), `LoadBuilding` (CID 1, `CBuilding::ctor` 0x10033920), `LoadCamera` (CID 2), `CreateAtmosphere` (CID 5),
`CreateShader` (CID 6).

## Врезка основания здания (`CLandscape::PlaceBuilding`, 0x1000A740)

Здание ставится не поверх ландшафта: под его контуром грани ландшафта заменяются новыми (платформа), а вокруг достраивается «патч» из треугольников.
Порядок по отладочным строкам и вызовам:

1. Запрашиваются `IGameObject`, `IBasement` и `IBuilding` объекта (иначе фатальная ошибка «Unable to retrieve ...»).
2. `CTerrain::PlaceBasement`: ищется грань, содержащая вершины внешнего контура («Seeking face with vertex»), и по соседним граням собирается область;
   ошибки: «Ran out of map», «Orientation fault 1/2», «Illegal value».
3. Грани, целиком попавшие внутрь контура, и грани, пересечённые контуром, помечаются на удаление («Face deleted INSIDE/ISECTED»); считаются внешние
   треугольники.
4. Проверяются итоговые внешний и внутренний контуры («Final outer contour and inner contour ...»); ориентация граней приводится к единой за несколько
   итераций («Orientation resolved in N iterations»).
5. Строятся новые грани основания и патч вокруг; триангуляция Делоне с перестановкой рёбер по критерию вписанной окружности
   (`Triangulation_RestoreDelaunayAroundEdge`, 0x10002A30); старые грани заменяются («Replacing face»).
6. Проверка: несвязанные рёбра и изолированные грани дают предупреждения («Unconnected edges», «Isolated faces»); число видимых граней патча не должно
   быть нулём.
7. Объект ставится `PlaceObject(SPACE_WORLD)`; для `IControl` объекта выполняется вставка (`CLandscape::Insert`).

Не выяснено: точные структуры граней и буферов патча, формулы выбора высоты платформы, связь с `CheckMaxBasementAngle`.

## Двери зданий (`CBuilding`)

`CBuilding::InitDoorsAndBoundsPieces` (0x100350F0) просматривает предметы здания (`IItemManager`): предметы типа **0x0B** (в данных нет) и **0x0C**
собираются в списки `also_doors` и `doors`, предметы **0x0D** - в третий список; для каждого запоминаются связанные детали меша. Дверь стартует в состоянии 2.
`CBuilding::AfterPlacementUpdated` (0x10036300) при перемещении другого объекта (события 1..3 - появление, перемещение, исчезновение) сравнивает капсулу детали
двери (`ComputeJointBoundingCapsule`) со сферой объекта; если расстояние меньше суммы радиусов, дверь из состояния 2 или 0 переходит в 3 и запоминает объект,
когда объект уходит - освобождается. Запись двери занимает 0x74 байта.

Тип класса здания (`probably_class_type`) определяется `IBehaviour::TransmitInitToAll`; если поведения нет, это `MainTeleport`.

## Не выяснено

- формат граней и сетки ландшафта в памяти, поиск грани по точке (`GetWalkFace`, `FindNearestIntersectedFaceInLandscape`) по данным;
- расчёт `CheckMaxBasementAngle`;
- устройство `CShade`/`CShader` и слоёв примитивов `PrimLayer*`;
- остальные безымянные функции (в основном вспомогательные контейнеры и растеризация).
