using CpDatLib;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class CpDatSchemeExplorer : IImGuiPanel
{
    private readonly CpDatSchemeViewModel _viewModel;

    public CpDatSchemeExplorer(CpDatSchemeViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("cp .dat Scheme Explorer"))
        {
            ImGui.Text("cp .dat - это файл схема здания или робота. Их можно найти в папке UNITS");
            ImGui.Separator();
            
            var cpDat = _viewModel.CpDatScheme;
            if (_viewModel.HasFile && cpDat is not null)
            {
                ImGui.Text("Тип объекта в схеме: ");
                ImGui.SameLine();
                ImGui.Text(cpDat.Type.ToString("G"));

                var root = cpDat.Root;

                DrawEntry(root, 0);

                ImGui.Separator();

                if (ImGui.BeginTable("content", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                {
                    ImGui.TableSetupColumn("Уровень вложенности");
                    ImGui.TableSetupColumn("Архив");
                    ImGui.TableSetupColumn("Элемент");
                    ImGui.TableSetupColumn("Magic1");
                    ImGui.TableSetupColumn("Magic2");
                    ImGui.TableSetupColumn("Описание");
                    ImGui.TableSetupColumn("Magic3");

                    ImGui.TableHeadersRow();

                    for (int i = 0; i < _viewModel.FlatList.Count; i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Level.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.ArchiveFile);
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.ArchiveEntryName);
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.Magic1.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.Magic2.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.Description);
                        ImGui.TableNextColumn();
                        ImGui.Text(_viewModel.FlatList[i].Entry.Magic3.ToString());
                    }

                    ImGui.EndTable();
                }

                void DrawEntry(CpDatEntry entry, int index)
                {
                    if (ImGui.TreeNodeEx($"Элемент: \"{entry.ArchiveFile}/{entry.ArchiveEntryName}\" - {entry.Description}##entry_{index}"))
                    {
                        ImGui.Text("Magic1: ");
                        ImGui.SameLine();
                        ImGui.Text(entry.Magic1.ToString());

                        ImGui.Text("Magic2: ");
                        ImGui.SameLine();
                        ImGui.Text(entry.Magic2.ToString());

                        ImGui.Text("Magic3: ");
                        ImGui.SameLine();
                        ImGui.Text(entry.Magic3.ToString());

                        ImGui.Text("Кол-во дочерних элементов: ");
                        ImGui.SameLine();
                        ImGui.Text(entry.ChildCount.ToString());

                        foreach (var child in entry.Children)
                        {
                            DrawEntry(child, ++index);
                        }

                        ImGui.TreePop();
                    }
                }
            }
            else if (_viewModel.Error is not null)
            {
                ImGui.Text(_viewModel.Error);
            }
            else
            {
                ImGui.Text("cp .dat не открыт");
            }
        }
        
        ImGui.End();
    }
}