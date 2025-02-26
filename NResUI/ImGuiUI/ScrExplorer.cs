using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

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
                                ImGui.TableSetupColumn("Индекс скрипта");
                                ImGui.TableSetupColumn("UnkInner2");
                                ImGui.TableSetupColumn("UnkInner3");
                                ImGui.TableSetupColumn("UnkInner4");
                                ImGui.TableSetupColumn("UnkInner5");
                                ImGui.TableSetupColumn("Кол-во аргументов");
                                ImGui.TableSetupColumn("Аргументы");
                                ImGui.TableSetupColumn("UnkInner7");
                                ImGui.TableHeadersRow();

                                for (int j = 0; j < entry.Inners.Count; j++)
                                {
                                    var inner = entry.Inners[j];
                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.ScriptIndex.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.UnkInner2.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.UnkInner3.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.UnkInner4.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.UnkInner5.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.ArgumentsCount.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(string.Join(", ", inner.Arguments));
                                    ImGui.TableNextColumn();
                                    ImGui.Text(inner.UnkInner7.ToString());
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