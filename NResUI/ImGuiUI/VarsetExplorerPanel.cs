using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class VarsetExplorerPanel : IImGuiPanel
{
    private readonly VarsetViewModel _viewModel;

    public VarsetExplorerPanel(VarsetViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("VARSET Explorer"))
        {
            ImGui.Text(".var - это файл динамических настроек. Можно найти в MISSIONS/SCRIPTS/varset.var, а также внутри behpsp.res");
            ImGui.Separator();

            if (_viewModel.Items.Count == 0)
            {
                ImGui.Text("VARSET не загружен");
            }
            else
            {
                if (ImGui.BeginTable($"varset", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                {
                    ImGui.TableSetupColumn("Индекс");
                    ImGui.TableSetupColumn("Тип");
                    ImGui.TableSetupColumn("Имя");
                    ImGui.TableSetupColumn("Значение");
                    ImGui.TableHeadersRow();

                    for (int j = 0; j < _viewModel.Items.Count; j++)
                    {
                        var item = _viewModel.Items[j];
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(j.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Type);
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Name);
                        ImGui.TableNextColumn();
                        ImGui.Text(item.Value);
                    }

                    ImGui.EndTable();
                }

                ImGui.TreePop();
            }

            ImGui.End();
        }
    }
}