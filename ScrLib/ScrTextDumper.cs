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
    public static string Dump(string fileName, ScrFile scr, FmlFile? fml, IReadOnlyList<string>? variableNames)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// {fileName}: секций {scr.EntryCount}, magic {scr.Magic}");
        sb.AppendLine("// Формат строки: <номер инструкции с 1>: <действие>   ; <исходные поля: act res val type cmp args fml>");
        sb.AppendLine("// act - NativeActionIndex (action_N в ai.dll), res - ResultVarIndex, val - ValueOrSourceVarIndex, type - ScrEntryInnerType, cmp - CompareOperator, fml - FmlFunctionIndex");
        sb.AppendLine();

        for (var i = 0; i < scr.Entries.Count; i++)
        {
            var entry = scr.Entries[i];
            sb.AppendLine($"== Секция {i} \"{entry.Title}\" (Index={entry.Index}, инструкций {entry.InnerCount})");

            var depth = 0;
            for (var j = 0; j < entry.Inners.Count; j++)
            {
                var inner = entry.Inners[j];
                if (inner.Type == ScrEntryInnerType.EndIf && depth > 0)
                {
                    depth--;
                }

                var text = Describe(scr, inner, fml, variableNames);
                var raw = $"act={inner.NativeActionIndex} res={inner.ResultVarIndex} val={inner.ValueOrSourceVarIndex} type={(int) inner.Type} cmp={inner.CompareOperator} args=[{string.Join(",", inner.Arguments)}] fml={inner.FmlFunctionIndex}";
                sb.AppendLine($"{j + 1,4}: {new string(' ', depth * 4)}{text}   ; {raw}");

                if (inner.Type == ScrEntryInnerType.Compare)
                {
                    depth++;
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Describe(ScrFile scr, ScrEntryInner inner, FmlFile? fml, IReadOnlyList<string>? names)
    {
        string Var(int index) =>
            index >= 0 && names is not null && index < names.Count ? names[index] : index >= 0 ? $"var[{index}]" : index.ToString();

        switch (inner.Type)
        {
            case ScrEntryInnerType.Compare:
                var op = inner.CompareOperator is >= 0 and < 6 ? CompareOperators[inner.CompareOperator] : $"op{inner.CompareOperator}";
                return inner.Arguments.Count >= 2
                    ? $"if {Var(inner.Arguments[0])} {op} {Var(inner.Arguments[1])}"
                    : $"if <некорректное условие: аргументов {inner.Arguments.Count}>";

            case ScrEntryInnerType.EndIf:
                return "endif";

            case ScrEntryInnerType.Nop:
                return "nop";

            case ScrEntryInnerType.GotoInstruction:
                return $"goto инструкция {(inner.Arguments.Count > 0 ? inner.Arguments[0] : -1)}";

            case ScrEntryInnerType.GotoEntry:
                var target = inner.Arguments.Count > 0 ? inner.Arguments[0] : -1;
                var title = target >= 0 && target < scr.Entries.Count ? $" \"{scr.Entries[target].Title}\"" : "";
                return $"goto секция {target}{title}";

            case ScrEntryInnerType.Return:
                return "return";

            case ScrEntryInnerType.SetVarsetValue:
                return $"{Var(inner.ResultVarIndex)} = {inner.ValueOrSourceVarIndex}";

            case ScrEntryInnerType.Execute:
                var left = inner.ResultVarIndex >= 0 ? $"{Var(inner.ResultVarIndex)} = " : "";
                if (inner.NativeActionIndex >= 0)
                {
                    var name = ScrActions.Names.TryGetValue(inner.NativeActionIndex, out var n) ? $"action_{inner.NativeActionIndex} {n}" : $"action_{inner.NativeActionIndex}";
                    return $"{left}{name}({string.Join(", ", inner.Arguments.Select(Var))})";
                }

                if (inner.ValueOrSourceVarIndex >= 0)
                {
                    return $"{left}{Var(inner.ValueOrSourceVarIndex)}";
                }

                var formulas = fml?.Formulas;
                var expression = formulas is not null && inner.FmlFunctionIndex >= 0 && inner.FmlFunctionIndex < formulas.Count
                    ? formulas[inner.FmlFunctionIndex].Expression
                    : $"fml[{inner.FmlFunctionIndex}]";
                return $"{left}{expression}";

            default:
                return $"тип {(int) inner.Type}";
        }
    }
}
