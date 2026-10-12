# WeaLib: наборы материалов `.wea`

`.wea` - текстовая таблица материалов и карт освещения объекта. Игра загружает её отдельно от MSH/CTL (`World3D.dll`, `MatManager::LoadWear`).
Парсер: `WeaParser` → `WeaFile` (`WeaMaterialRef`, `WeaLightmapRef`). Правила (номер строки не используется, до 40 наборов, раздел `LIGHTMAPS`),
кэши и поведение менеджера материалов - в [MaterialLib/README.md](../MaterialLib/README.md), §3.

## Формат

Загружается в `World3D.dll/LoadMatManager`

По сути это текстовый файл состоящий из 2 частей:
- Материалы
  ```
  {count}
  {id} {name}
  ```
- Карты освещения
  ```
  LIGHTMAPS
  {count}
  {id} {name}
  ```

Номер в строке `{id}` загрузчик отбрасывает: материалы идут по порядку строк. Анимация материалов, кэши и режимы смешивания - `MaterialLib/README.md`.
