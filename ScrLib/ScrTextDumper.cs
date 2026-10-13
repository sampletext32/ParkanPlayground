using System.Text;

namespace ScrLib;

/// <summary>
/// Текстовый дамп скрипта <c>.scr</c> (с формулами <c>.fml</c> и именами переменных <c>varset.var</c>): по строке на инструкцию, вложенность по условиям,
/// после <c>;</c> - исходные числа записи, чтобы дамп можно было искать (grep) и сверять с декомпиляцией <c>ai.dll</c>.
/// </summary>
public static class ScrTextDumper
{
    private static readonly string[] CompareOperators = ["<", "==", ">", "<=", ">=", "!="];

    /// <summary>Строит текст дампа.</summary>
    /// <param name="fileName">Имя файла для заголовка (например <c>default.scr</c>).</param>
    /// <param name="scr">Разобранный скрипт.</param>
    /// <param name="fml">Формулы одноимённого <c>.fml</c>; null - формулы выводятся как <c>fml[N]</c>.</param>
    /// <param name="variableNames">Имена переменных Varset по индексу (порядок объявления в <c>varset.var</c>); null - выводятся <c>var[N]</c>.</param>
    /// <param name="useActionNames">false - не подставлять имена встроенных функций (<see cref="ScrActions"/> описывает только первую часть игры), выводить <c>action_N</c>.</param>
    public static string Dump(string fileName, ScrFile scr, FmlFile? fml, IReadOnlyList<string>? variableNames, bool useActionNames = true)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// {fileName}: секций {scr.EntryCount}, magic {scr.Magic}");
        sb.AppendLine("// Формат строки: <номер инструкции с 1>: <действие>   ; <исходные поля: act res val type cmp args fml>");
        sb.AppendLine("// act - NativeActionIndex (action_N в ai.dll), res - ResultVarIndex, val - ValueOrSourceVarIndex, type - ScrInstructionType, cmp - CompareOperator, fml - FmlFunctionIndex");
        sb.AppendLine();

        for (var i = 0; i < scr.Entries.Count; i++)
        {
            var entry = scr.Entries[i];
            sb.AppendLine($"== Секция {i} \"{entry.Title}\" (Index={entry.Index}, инструкций {entry.InstructionCount})");

            var depth = 0;
            for (var j = 0; j < entry.Instructions.Count; j++)
            {
                var instruction = entry.Instructions[j];
                if (instruction.Type == ScrInstructionType.EndIf && depth > 0)
                {
                    depth--;
                }

                var text = Describe(scr, instruction, fml, variableNames, useActionNames);
                var raw = $"act={instruction.NativeActionIndex} res={instruction.ResultVarIndex} val={instruction.ValueOrSourceVarIndex} type={(int) instruction.Type} cmp={instruction.CompareOperator} args=[{string.Join(",", instruction.Arguments)}] fml={instruction.FmlFunctionIndex}";
                sb.AppendLine($"{j + 1,4}: {new string(' ', depth * 4)}{text}   ; {raw}");

                if (instruction.Type == ScrInstructionType.Compare)
                {
                    depth++;
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Describe(ScrFile scr, ScrInstruction instruction, FmlFile? fml, IReadOnlyList<string>? names, bool useActionNames)
    {
        string Var(int index) =>
            index >= 0 && names is not null && index < names.Count ? names[index] : index >= 0 ? $"var[{index}]" : index.ToString();

        switch (instruction.Type)
        {
            case ScrInstructionType.Compare:
                var op = instruction.CompareOperator is >= 0 and < 6 ? CompareOperators[instruction.CompareOperator] : $"op{instruction.CompareOperator}";
                return instruction.Arguments.Count >= 2
                    ? $"if {Var(instruction.Arguments[0])} {op} {Var(instruction.Arguments[1])}"
                    : $"if <некорректное условие: аргументов {instruction.Arguments.Count}>";

            case ScrInstructionType.EndIf:
                return "endif";

            case ScrInstructionType.Nop:
                return "nop";

            case ScrInstructionType.GotoInstruction:
                return $"goto инструкция {(instruction.Arguments.Count > 0 ? instruction.Arguments[0] : -1)}";

            case ScrInstructionType.GotoEntry:
                var target = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : -1;
                var title = target >= 0 && target < scr.Entries.Count ? $" \"{scr.Entries[target].Title}\"" : "";
                return $"goto секция {target}{title}";

            case ScrInstructionType.Return:
                return "return";

            case ScrInstructionType.SetVarsetValue:
                return $"{Var(instruction.ResultVarIndex)} = {instruction.ValueOrSourceVarIndex}";

            case ScrInstructionType.Execute:
                var left = instruction.ResultVarIndex >= 0 ? $"{Var(instruction.ResultVarIndex)} = " : "";
                if (instruction.NativeActionIndex >= 0)
                {
                    var name = useActionNames && ScrActions.Names.TryGetValue(instruction.NativeActionIndex, out var n) ? $"action_{instruction.NativeActionIndex} {n}" : $"action_{instruction.NativeActionIndex}";
                    return $"{left}{name}({string.Join(", ", instruction.Arguments.Select(Var))})";
                }

                if (instruction.ValueOrSourceVarIndex >= 0)
                {
                    return $"{left}{Var(instruction.ValueOrSourceVarIndex)}";
                }

                var formulas = fml?.Formulas;
                var expression = formulas is not null && instruction.FmlFunctionIndex >= 0 && instruction.FmlFunctionIndex < formulas.Count
                    ? formulas[instruction.FmlFunctionIndex].Expression
                    : $"fml[{instruction.FmlFunctionIndex}]";
                return $"{left}{expression}";

            default:
                return $"тип {(int) instruction.Type}";
        }
    }
}
