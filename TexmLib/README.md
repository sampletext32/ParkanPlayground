# TexmLib: текстуры TEXM

![TEXM Explorer: текстура `EFFECT5.0` с раскладкой атласа страниц](../docs/images/texm-explorer.png)
*TEXM Explorer: текстура `EFFECT5.0` с раскладкой атласа страниц*


`TEXM` - формат текстур игры (в NRes-архивах `Textures.lib`, `lightmap.lib`, `ui.lib`, `font.lib`; тип элемента `TEXM`). Парсер: `TexmParser.ReadFromStream` →
`TexmParseResult` (`TexmFile` + сообщение об ошибке).

## Структура файла

| Часть | Размер | Содержимое |
|---|---|---|
| заголовок (`TexmHeader`) | 32 байта | ASCII `Texm`, ширина, высота, число мип-уровней, поле stride (в коде `Stride`), `Magic1`, `FormatOptionFlags`, формат |
| таблица цветов | 1024 байта | только при формате 0: 256 цветов ARGB, текстура хранит индексы (8 бит) |
| мип-уровни | по размеру формата | от самого большого к самому маленькому |
| `Page` (необязательно) | 8 байт + 8 × число записей | атлас: список прямоугольников страниц (`PageItem`: x, ширина, y, высота) |

Форматы (`Extensions.AsStride`, байт на пиксель): `0x0` - 1 (индексированная), `0x58`, `0x235`, `0x22C`, `0x115C` - 2 (16 бит), `0x378`, `0x22B8` - 4 (32 бита).
Неизвестный формат вызывает исключение.

Страницы используются материалами: стадия материала указывает номер страницы (`MaterialStage.TextureStageIndex`), см. [MaterialLib](../MaterialLib/README.md).

## Lightmap (TEXM)

У текстур использующихся как карты освещения специальные имена.

`some_name.00` или `some_name.0`

Первая цифра после `.` это группа палет (всего 0x11), вторая цифра индекс в этой группе.

```
All lightmaps named whatever.0 → they all end up with no DirectDraw palette attached.
```
