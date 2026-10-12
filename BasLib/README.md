# BasLib: контуры оснований зданий (`.bas`)

Файл `.bas` (Basement) описывает контуры здания на земле. Читается в `Terrain.dll` (`LoadBuilding` → `CBuilding::ctor`): на него ссылается запись `FORT`
(`fortif.rlb`, вторая ссылка; первая - BTLU-здание). Парсер: `BasParser.ReadFile` → `BasContours` (`InnerContour[]`, `OuterContour[]`).

## Формат

Читается в `Terrain.dll/LoadBuilding/CBuilding::ctor`

Описывает контуры объекта

Формат
```
Длина динамическая

(4 байта) Кол-во внутренних контуров -> IC
{
  (4 байта) Кол-во сегментов контура -> N
    Vector3[N+1] Точки контура
    int[N] - непонятно
    int[N] - тоже непонятно, но выглядит как порядок вершин (e.g. [0 2 1])
}[IC]
(4 байта) Кол-во внешних контуров -> OC
{
  (4 байта) Кол-во сегментов контура -> N
    Vector3[N+1] Точки контура
}[OC]
```

## Использование

- Запись `FORT`, которая ссылается на `.bas`: см. [ControlLib/README.md](../ControlLib/README.md), §4.
