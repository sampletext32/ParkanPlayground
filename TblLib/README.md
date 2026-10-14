# TblLib: таблицы привязок ввода (`.tbl`)

Текстовые файлы `m1.tbl`, `m2.tbl`, `hero.tbl` в папке игры: привязки клавиш и осей мыши к командам ручного управления (`IManManager` из
`World3D.dll`, см. [docs/WORLD3D.md](../docs/WORLD3D.md)). Одна строка - одна привязка, `//` начинает комментарий:

```
KEY   SCAN_NULL SCAN_W 1 CICLS_UNKNOWN MCMD_WALK_F  1.0 0 0 0.0 0 // OBJ_MOVE_FORWARD
```

Поля описаны в `TblBinding`. Формат игра читает строкой `"%s %s %d %s %s %f %d %s %f %d"` после слова устройства; проверено на трёх файлах игры (112
строк).

```csharp
var bindings = TblParser.ReadFile("m1.tbl");
```
