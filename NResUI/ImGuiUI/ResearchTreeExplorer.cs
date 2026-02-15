using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class ResearchTreeExplorer : IImGuiPanel
{
    private readonly ResearchTreeViewModel _viewModel;

    public ResearchTreeExplorer(ResearchTreeViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("Research Tree Explorer (trf)"))
        {
            ImGui.Text("trf - это файл дерева исследований. Их можно найти в папке MISSIONS/SCRIPTS");
            ImGui.Separator();
            
            var nodes = _viewModel.ResearchNodeDatas;

            if (_viewModel.HasFile && nodes is not null)
            {
                if (ImGui.TreeNodeEx("Узлы"))
                {
                    for (var i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (ImGui.TreeNodeEx($"{i} - \"{node.LongName}\" (\"{node.ShortName}\")"))
                        {
                            ImGui.Text("Состояние: ");
                            ImGui.SameLine();
                            ImGui.Text(node.State.ToString("G"));

                            ImGui.Text("ShortName: ");
                            ImGui.SameLine();
                            ImGui.Text(node.ShortName);

                            ImGui.Text("LongName: ");
                            ImGui.SameLine();
                            ImGui.Text(node.LongName);

                            ImGui.Text("HelpText: ");
                            ImGui.SameLine();
                            ImGui.Text(node.HelpText);

                            ImGui.Text("Description: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Description);

                            ImGui.Text("Тип турели: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.TurretType.ToString());

                            ImGui.Text("Основной тип: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.MainType.ToString());

                            ImGui.Text("Подтип: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.SubType.ToString());

                            ImGui.Text("Подтип строения: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.BuildSubSystem.ToString());

                            ImGui.Text("Размер типа: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.SizeOfType.ToString());

                            ImGui.Text("Уровень апгрейда: ");
                            ImGui.SameLine();
                            ImGui.Text(node.Node.UpgradeLevel.ToString());
                            
                            ImGui.Text("Условия: ");
                            ImGui.SameLine();
                            ImGui.Text(string.Join(", ", node.PrerequisiteIds));

                            ImGui.Text("Открывает: ");
                            ImGui.SameLine();
                            ImGui.Text(string.Join(", ", node.UnlockIds));

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text("trf не открыт");
            }

            ImGui.End();
        }
    }
}
