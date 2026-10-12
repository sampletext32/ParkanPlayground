# VarsetLib: наборы переменных (`.var`)

![VARSET Explorer: переменные и константы `MISSIONS/SCRIPTS/varset.var`](../docs/images/varset.png)
*VARSET Explorer: переменные и константы `MISSIONS/SCRIPTS/varset.var`*


Игра хранит именованные переменные с типами в двух видах:

| Вид | Где в игре | Парсер |
|---|---|---|
| Текст `VAR( тип, имя, значение )` | `MISSIONS/SCRIPTS/varset.var` - переменные и константы скриптов ИИ | `VarsetParser.Parse` → `List<VarsetItem>` |
| Бинарный `VarsetFile` | внутри `behpsp.res` (поведение юнитов, `chas_*.var`) | `BinaryVarsetFileParser.Parse` → `BinaryVarsetFile` |

## Текстовый формат

```
//VAR( Type, Name, DefValue, Minimum, Maximum, Comment)
//STRING( Size, Name, DefValue, Comment)
VAR( float, fDifficulty, 0.5)    // комментарий
VAR( DWORD, PBM_MINE_NEEDED, 1)
```

Строки, начинающиеся с `//`, и пустые пропускаются. `VarsetItem` хранит тип, имя и значение как строки. Индекс переменной в скриптах `.scr`
(инструкции `Compare`, `SetVarsetValue`, аргументы встроенных функций) - порядковый номер объявления; подробности - в [ScrLib/README.md](../ScrLib/README.md).
Файл `varset.var` игры содержит 220 переменных: рабочие (`f0..f9`, `d0..d9`, `fTemp*`, `dTemp*`), управляемые кодом (`dCurrentProblem`, `fDifficulty`, `dMax*`),
настройки характера клана (`fHazard`, `fAgressive`...) и константы (`PBM_*`, `ORDER_*`, `BUILDING_*`, `ROBOT_*`, `RESOURCE_*`...).

## Бинарный формат

Содержимое: число переменных, затем для каждой: длина значения, тип, `Magic3` (почти всегда 0), имя, вторая строка (обычно пуста), `Magic4`, `Magic5`,
`Magic6`, `Magic7` (значения того же типа, что и переменная). Типы (`BinaryVarsetValueType`): `Float0` 0, `Int` 1, `Bool` 2, `Float` 3, `CharPtr` 4, `Dword` 5; парсер
понимает `Bool`, `Dword` и `Float`. Смысл `Magic4..Magic7` не подтверждён: `Magic6` и `Magic7` похожи на минимум и максимум.

## Панели NResUI

`VarsetExplorerPanel` показывает текстовый набор (`VarsetParser`); бинарный парсер в NResUI не подключён.
