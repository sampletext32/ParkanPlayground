using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using ScrLib;

namespace NResUI.ImGuiUI;

public class ScrExplorer : IImGuiPanel
{
    private readonly ScrViewModel _viewModel;

    public ScrExplorer(ScrViewModel viewModel)
    {
        _viewModel = viewModel;
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

                if (ImGui.TreeNodeEx("Секции"))
                {
                    for (var i = 0; i < scr.Entries.Count; i++)
                    {
                        var entry = scr.Entries[i];
                        if (ImGui.TreeNodeEx($"Секция {i} - \"{entry.Title}\""))
                        {
                            ImGui.Text("Индекс: ");
                            ImGui.SameLine();
                            ImGui.Text(entry.Index.ToString());

                            ImGui.Text("Кол-во элементов: ");
                            ImGui.SameLine();
                            ImGui.Text(entry.InnerCount.ToString());

                            if (ImGui.BeginTable($"Элементы##{i:0000}", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
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

                                for (int j = 0; j < entry.Inners.Count; j++)
                                {
                                    var inner = entry.Inners[j];
                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.Text(ScrLib.ScrActions.Describe(inner.NativeActionIndex));

                                    if (inner.NativeActionIndex == 2)
                                    {
                                        Utils.ShowHint("Первый аргумент - номер проблемы");
                                    }

                                    if (inner.NativeActionIndex == 4)
                                    {
                                        Utils.ShowHint("Первый аргумент - номер проблемы");
                                    }

                                    if (inner.NativeActionIndex == 8)
                                    {
                                        Utils.ShowHint("Установить dCurrentProblem стейт (VARSET:arg0)");
                                    }

                                    if (inner.NativeActionIndex == 20)
                                    {
                                        Utils.ShowHint("Первый аргумент - номер проблемы");
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.ResultVarIndex.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.ValueOrSourceVarIndex.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text($"{(int) inner.Type}: {inner.Type:G}");
                                    if (inner.Type == ScrEntryInnerType.Compare)
                                    {
                                        Utils.ShowHint("Сравнение двух переменных Varset (аргументы), операция в CompareOperator: 0 <, 1 ==, 2 >, 3 <=, 4 >=, 5 !=");
                                    }

                                    if (inner.Type == ScrEntryInnerType.Return)
                                    {
                                        Utils.ShowHint("Return: если условия выполнены, секция на этом тике завершается");
                                    }

                                    if (inner.Type == ScrEntryInnerType.SetVarsetValue)
                                    {
                                        Utils.ShowHint("Записать в переменную Varset ResultVarIndex значение ValueOrSourceVarIndex");
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.CompareOperator.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.ArgumentsCount.ToString());
                                    ImGui.TableNextColumn();
                                    foreach (var argument in inner.Arguments)
                                    {
                                        if (ImGui.Button(argument.ToString()))
                                        {
                                        }

                                        ImGui.SameLine();
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.FmlFunctionIndex.ToString());
                                }

                                ImGui.EndTable();
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
}