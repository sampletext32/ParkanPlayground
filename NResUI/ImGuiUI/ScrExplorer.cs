using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using ScrLib;

namespace NResUI.ImGuiUI;

public class ScrExplorer : IImGuiPanel
{
    private static readonly string[] CompareOperators = ["<", "==", ">", "<=", ">=", "!="];

    private static readonly Vector4 ColorLineNumber = new(0.35f, 0.35f, 0.35f, 1f);
    private static readonly Vector4 ColorKeyword = new(0.05f, 0.2f, 0.85f, 1f);
    private static readonly Vector4 ColorVariable = new(0.0f, 0.5f, 0.05f, 1f);
    private static readonly Vector4 ColorFunction = new(0.75f, 0.3f, 0.0f, 1f);
    private static readonly Vector4 ColorNumber = new(0.7f, 0.0f, 0.5f, 1f);
    private static readonly Vector4 ColorPlain = new(0.0f, 0.0f, 0.0f, 1f);

    private readonly ScrViewModel _viewModel;
    private readonly VarsetViewModel _varsetViewModel;

    public ScrExplorer(ScrViewModel viewModel, VarsetViewModel varsetViewModel)
    {
        _viewModel = viewModel;
        _varsetViewModel = varsetViewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("SCR Explorer"))
        {
            ImGui.Text("scr - это файл AI скриптов. Их можно найти в папке MISSIONS/SCRIPTS");
            ImGui.Separator();

            var scr = _viewModel.Scr;
            if (_viewModel.HasFile && scr is not null)
            {
                ImGui.Text("Магия: ");
                Utils.ShowHint("тут всегда число 59 (0x3b) - это число известных игре скриптов");
                ImGui.SameLine();
                ImGui.Text(scr.Magic.ToString());

                ImGui.Text("Кол-во секций: ");
                ImGui.SameLine();
                ImGui.Text(scr.EntryCount.ToString());

                if (_varsetViewModel.Items.Count == 0)
                {
                    ImGui.TextDisabled("Имена переменных не показаны: откройте MISSIONS/SCRIPTS/varset.var (File -> Open Varset File)");
                }

                if (ImGui.TreeNodeEx("Секции"))
                {
                    for (var i = 0; i < scr.Entries.Count; i++)
                    {
                        var entry = scr.Entries[i];
                        if (ImGui.TreeNodeEx($"Секция {i} - \"{entry.Title}\" ({entry.InstructionCount})###section{i}"))
                        {
                            if (ImGui.BeginTabBar($"Вид##{i:0000}"))
                            {
                                if (ImGui.BeginTabItem("Текст"))
                                {
                                    DrawListing(scr, entry, i);
                                    ImGui.EndTabItem();
                                }

                                if (ImGui.BeginTabItem("Таблица"))
                                {
                                    DrawTable(entry, i);
                                    ImGui.EndTabItem();
                                }

                                ImGui.EndTabBar();
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text("SCR не открыт");
            }

            ImGui.End();
        }
    }

    /// <summary>Читаемый вид секции: по строке на инструкцию, вложенность по условиям, при наведении - подробности.</summary>
    private void DrawListing(ScrFile scr, ScrEntry entry, int entryIndex)
    {
        ImGui.PushID($"listing{entryIndex}");
        var depth = 0;

        for (var j = 0; j < entry.Instructions.Count; j++)
        {
            var instruction = entry.Instructions[j];
            ImGui.PushID(j);

            if (instruction.Type == ScrInstructionType.EndIf && depth > 0)
            {
                depth--;
            }

            var first = true;
            Token($"{j + 1,3}", ColorLineNumber, "Номер инструкции в секции (с 1): на него ссылаются переходы", ref first);
            Token(new string(' ', depth * 4), ColorPlain, null, ref first, noSpace: true);

            switch (instruction.Type)
            {
                case ScrInstructionType.Execute:
                    DrawExecute(instruction, ref first);
                    break;

                case ScrInstructionType.Compare:
                    Token("если", ColorKeyword, "Условие: следующие инструкции до «конец если» выполняются, только если оно истинно. Вложенные условия объединяются по И.", ref first);
                    if (instruction.Arguments.Count >= 2)
                    {
                        Variable(instruction.Arguments[0], ref first);
                        var op = instruction.CompareOperator is >= 0 and < 6 ? CompareOperators[instruction.CompareOperator] : $"op{instruction.CompareOperator}";
                        Token(op, ColorKeyword, $"Операция сравнения {instruction.CompareOperator}: 0 <, 1 ==, 2 >, 3 <=, 4 >=, 5 !=\nDWORD сравнивается как целое, остальное - как float", ref first);
                        Variable(instruction.Arguments[1], ref first);
                    }

                    depth++;
                    break;

                case ScrInstructionType.EndIf:
                    Token("конец если", ColorKeyword, "Закрывает ближайшее условие", ref first);
                    break;

                case ScrInstructionType.Nop:
                    Token("пусто", ColorKeyword, "Ничего не делает (в данных игры встречается 9 раз)", ref first);
                    break;

                case ScrInstructionType.GotoInstruction:
                    Token("перейти на инструкцию", ColorKeyword, "Продолжить выполнение секции с инструкции с указанным номером", ref first);
                    var target = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : -1;
                    Token(target.ToString(), ColorNumber, "Номер инструкции в этой секции (с 1)", ref first);
                    break;

                case ScrInstructionType.GotoEntry:
                    Token("перейти в секцию", ColorKeyword, "Начать выполнение другой секции с первой инструкции", ref first);
                    var section = instruction.Arguments.Count > 0 ? instruction.Arguments[0] : -1;
                    var sectionTitle = section >= 0 && section < scr.Entries.Count ? scr.Entries[section].Title : null;
                    Token(
                        sectionTitle is null ? section.ToString() : $"{section} \"{sectionTitle}\"",
                        ColorNumber,
                        sectionTitle is null ? "Номер секции (вне диапазона файла)" : $"Секция {section}: \"{sectionTitle}\", инструкций: {scr.Entries[section].InstructionCount}",
                        ref first);
                    break;

                case ScrInstructionType.Return:
                    Token("вернуться", ColorKeyword, "Завершить секцию на этом тике. При следующем вызове она снова начнётся с первой инструкции.", ref first);
                    break;

                case ScrInstructionType.SetVarsetValue:
                    Variable(instruction.ResultVarIndex, ref first);
                    Token("=", ColorPlain, null, ref first);
                    Token(instruction.ValueOrSourceVarIndex.ToString(), ColorNumber,
                        "Записываемое значение: для переменной типа DWORD - целое число, для остальных типов - как float", ref first);
                    break;

                default:
                    Token($"тип {(int) instruction.Type}", ColorPlain, "Неизвестный тип инструкции", ref first);
                    break;
            }

            ImGui.PopID();
        }

        ImGui.PopID();
    }

    private void DrawExecute(ScrInstruction instruction, ref bool first)
    {
        ScrActionSignature? signature = null;
        if (instruction.NativeActionIndex >= 0)
        {
            ScrActions.Signatures.TryGetValue(instruction.NativeActionIndex, out signature);
        }

        if (instruction.ResultVarIndex >= 0)
        {
            Variable(instruction.ResultVarIndex, ref first);
            Token("=", ColorPlain, "Результат записывается в эту переменную Varset", ref first);
        }

        if (instruction.NativeActionIndex >= 0)
        {
            var known = ScrActions.Names.TryGetValue(instruction.NativeActionIndex, out var n);
            var name = known ? n! : $"action_{instruction.NativeActionIndex}";
            var hint = $"Встроенная функция ai.dll action_{instruction.NativeActionIndex}" + (known ? "" : " (пока не разобрана)");
            if (ScrActions.Signatures.TryGetValue(instruction.NativeActionIndex, out var sig))
            {
                hint += sig.Arguments.Length == 0
                    ? "\nАргументов нет"
                    : "\nАргументы:" + string.Concat(sig.Arguments.Select((a, k) => $"\n  {k}: {a}"));
                hint += "\nРезультат: " + (sig.Result ?? "нет");
                if (sig.Arguments.Length != instruction.ArgumentsCount)
                {
                    hint += $"\n(в этом вызове аргументов: {instruction.ArgumentsCount})";
                }
            }
            else
            {
                hint += $"\nАргументов в этом вызове: {instruction.ArgumentsCount}";
            }

            Token(name, ColorFunction, hint, ref first);
        }
        else if (instruction.ValueOrSourceVarIndex >= 0)
        {
            // копирование переменной
            Variable(instruction.ValueOrSourceVarIndex, ref first);
            return;
        }
        else
        {
            DrawFormula(instruction, ref first);
            return;
        }

        Token("(", ColorPlain, null, ref first, noSpace: true);
        for (var k = 0; k < instruction.Arguments.Count; k++)
        {
            if (k > 0)
            {
                Token(",", ColorPlain, null, ref first, noSpace: true);
            }

            var role = signature is not null && k < signature.Arguments.Length ? signature.Arguments[k] : null;
            Variable(instruction.Arguments[k], ref first, argumentNumber: k, noSpace: k == 0, argumentRole: role);
        }

        Token(")", ColorPlain, null, ref first, noSpace: true);
    }

    /// <summary>Формула из .fml: выражение с подсвеченными переменными и числами; без .fml - заглушка <c>fml[N]</c>.</summary>
    private void DrawFormula(ScrInstruction instruction, ref bool first)
    {
        var formulas = _viewModel.Fml?.Formulas;
        if (formulas is null || instruction.FmlFunctionIndex < 0 || instruction.FmlFunctionIndex >= formulas.Count)
        {
            Token($"fml[{instruction.FmlFunctionIndex}]", ColorFunction,
                _viewModel.Fml is null
                    ? $"Формула №{instruction.FmlFunctionIndex} из одноимённого .fml, файл рядом с .scr не найден"
                    : $"Формула №{instruction.FmlFunctionIndex}: в .fml всего {formulas!.Count} формул", ref first);
            return;
        }

        var expression = formulas[instruction.FmlFunctionIndex].Expression;
        var hint = $"Формула №{instruction.FmlFunctionIndex} из .fml: {expression}";

        var i = 0;
        while (i < expression.Length)
        {
            var c = expression[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            int start = i;
            if (char.IsLetter(c) || c == '_')
            {
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                {
                    i++;
                }

                var name = expression[start..i];
                var index = _varsetViewModel.Items.FindIndex(v => v.Name == name);
                var details = index >= 0
                    ? $"Переменная Varset #{index}\nТип: {_varsetViewModel.Items[index].Type}\nЗначение по умолчанию: {_varsetViewModel.Items[index].Value}"
                    : $"Переменная {name}" + (_varsetViewModel.Items.Count == 0 ? " (откройте varset.var, чтобы увидеть описание)" : " (в открытом varset.var не найдена)");
                Token(name, ColorVariable, hint + "\n\n" + details,
                    ref first);
            }
            else if (char.IsDigit(c) || c == '.')
            {
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    i++;
                }

                Token(expression[start..i], ColorNumber, hint, ref first);
            }
            else
            {
                i++;
                Token(c.ToString(), ColorFunction, hint, ref first);
            }
        }
    }

    /// <summary>Переменная Varset: имя, если varset.var открыт, иначе <c>var[N]</c>; подробности - в подсказке.</summary>
    private void Variable(int index, ref bool first, int argumentNumber = -1, bool noSpace = false, string? argumentRole = null)
    {
        var items = _varsetViewModel.Items;
        string text;
        string hint;
        if (index >= 0 && index < items.Count)
        {
            var item = items[index];
            text = item.Name;
            hint = $"Переменная Varset #{index}\nИмя: {item.Name}\nТип: {item.Type}\nЗначение по умолчанию: {item.Value}";
        }
        else if (index >= 0)
        {
            text = $"var[{index}]";
            hint = items.Count == 0
                ? $"Переменная Varset #{index}\nОткройте varset.var, чтобы увидеть имя"
                : $"Переменная Varset #{index}: индекса нет в открытом varset.var ({items.Count} переменных)";
        }
        else
        {
            text = index.ToString();
            hint = "Нет переменной (-1)";
        }

        if (argumentNumber >= 0)
        {
            hint = $"Аргумент {argumentNumber}" + (argumentRole is null ? "" : $": {argumentRole}") + "\n" + hint;
        }

        Token(text, ColorVariable, hint, ref first, noSpace);
    }

    private static void Token(string text, Vector4 color, string? hint, ref bool first, bool noSpace = false)
    {
        if (!first)
        {
            ImGui.SameLine(0, noSpace ? 0 : 6);
        }

        first = false;
        ImGui.TextColored(color, text);
        if (hint is not null)
        {
            Utils.ShowHint(hint);
        }
    }

    private static void DrawTable(ScrEntry entry, int entryIndex)
    {
        ImGui.Text($"Индекс: {entry.Index}, кол-во элементов: {entry.InstructionCount}");

        if (ImGui.BeginTable($"Элементы##{entryIndex:0000}", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
        {
            ImGui.TableSetupColumn("Встроенная функция");
            ImGui.TableSetupColumn("Результат в переменную");
            ImGui.TableSetupColumn("Значение / источник");
            ImGui.TableSetupColumn("Тип действия");
            ImGui.TableSetupColumn("Операция сравнения");
            ImGui.TableSetupColumn("Кол-во аргументов");
            ImGui.TableSetupColumn("Аргументы");
            ImGui.TableSetupColumn("Формула .fml");
            ImGui.TableHeadersRow();

            for (var j = 0; j < entry.Instructions.Count; j++)
            {
                var instruction = entry.Instructions[j];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ScrActions.Describe(instruction.NativeActionIndex));

                ImGui.TableNextColumn();
                ImGui.Text(instruction.ResultVarIndex.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(instruction.ValueOrSourceVarIndex.ToString());
                ImGui.TableNextColumn();
                ImGui.Text($"{(int) instruction.Type}: {instruction.Type:G}");
                if (instruction.Type == ScrInstructionType.Compare)
                {
                    Utils.ShowHint("Сравнение двух переменных Varset (аргументы), операция в CompareOperator: 0 <, 1 ==, 2 >, 3 <=, 4 >=, 5 !=");
                }

                if (instruction.Type == ScrInstructionType.Return)
                {
                    Utils.ShowHint("Return: если условия выполнены, секция на этом тике завершается");
                }

                if (instruction.Type == ScrInstructionType.SetVarsetValue)
                {
                    Utils.ShowHint("Записать в переменную Varset ResultVarIndex значение ValueOrSourceVarIndex");
                }

                ImGui.TableNextColumn();
                ImGui.Text(instruction.CompareOperator.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(instruction.ArgumentsCount.ToString());
                ImGui.TableNextColumn();
                foreach (var argument in instruction.Arguments)
                {
                    ImGui.Button($"{argument}##{j}_{argument}");
                    ImGui.SameLine();
                }

                ImGui.TableNextColumn();
                ImGui.Text(instruction.FmlFunctionIndex.ToString());
            }

            ImGui.EndTable();
        }
    }
}
