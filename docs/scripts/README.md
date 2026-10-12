# Дампы скриптов ИИ (`.scr` + `.fml`)

Текстовые дампы всех 50 скриптов `MISSIONS/SCRIPTS` и списка переменных `varset.var` - чтобы изучать логику ИИ без запуска приложения
и сопоставлять её с декомпиляцией `ai.dll` в Ghidra (`AIActions::execute_scr_ai_script`, `action_N`).

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
