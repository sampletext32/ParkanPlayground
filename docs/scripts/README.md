# Дампы скриптов ИИ (`.scr` + `.fml`)

Текстовые дампы всех 50 скриптов `MISSIONS/SCRIPTS` и списка переменных `varset.var` - чтобы изучать логику ИИ без запуска приложения
и сопоставлять её с декомпиляцией `ai.dll` в Ghidra (`AIActions::ExecuteInstruction`, `action_N`).

| Файл | Что внутри |
|---|---|
| `<имя>.txt` | секции скрипта: по строке на инструкцию, вложенность по условиям (`if` ... `endif`), формулы `.fml` подставлены выражением |
| `varset.txt` | `индекс  тип  имя  значение по умолчанию` (220 переменных) |

Строка дампа: `<номер инструкции с 1>: <действие>   ; act=.. res=.. val=.. type=.. cmp=.. args=[..] fml=..`.
После `;` - исходные поля записи (`NativeActionIndex`, `ResultVarIndex`, `ValueOrSourceVarIndex`, тип, `CompareOperator`, аргументы,
`FmlFunctionIndex`), их смысл описан в [ScrLib/README.md](../../ScrLib/README.md). Имена встроенных функций - из `ScrLib.ScrActions`
(для неразобранных - просто `action_N`); имена переменных - по порядку объявления в `varset.var`.

Дампы - производные данные: пересоздаются командой

```
dotnet run --project ParkanPlayground -- scr-dump "<игра>\MISSIONS\SCRIPTS" docs/scripts
```

## Вторая часть игры (`is2/`)

В `docs/scripts/is2` лежат такие же дампы скриптов `IS2\MISSIONS\SCRIPTS` (59 файлов, 231 переменная). Движок второй части новее: у скриптов `magic` = 73
(встроенных функций 0..72, в данных используются 59-71, которых в первой части нет), поэтому номера `action_N` выведены **без имён** - таблица `ScrActions`
описывает только первую часть, и совпадение номеров 0..58 между частями не проверено. Вторая часть не исследуется; дамп сделан командой

```
dotnet run --project ParkanPlayground -- scr-dump "<IS2>\MISSIONS\SCRIPTS" docs/scripts/is2 raw
```
