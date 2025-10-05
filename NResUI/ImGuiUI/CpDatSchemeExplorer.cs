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

                if (ImGui.BeginTable("content", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.Sortable))
                {
                    ImGui.TableSetupColumn("Индекс");
                    ImGui.TableSetupColumn("Уровень вложенности");
                    ImGui.TableSetupColumn("Архив");
                    ImGui.TableSetupColumn("Элемент");
                    ImGui.TableSetupColumn("Magic1");
                    ImGui.TableSetupColumn("Magic2");
                    ImGui.TableSetupColumn("Описание");
                    ImGui.TableSetupColumn("Тип");

                    ImGui.TableHeadersRow();

                    // Handle sorting
                    ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
                    if (sortSpecs.SpecsDirty)
                    {
                        // Only handle the first sort spec for simplicity
                        var sortSpec = sortSpecs.Specs;

                        if (sortSpec.ColumnIndex == 0)
                        {
                            _viewModel.RebuildFlatList();
                        }
                        else
                        {

                            _viewModel.FlatList.Sort((a, b) =>
                            {
                                int result = 0;
                                switch (sortSpec.ColumnIndex)
                                {
                                    case 1: result = a.Level.CompareTo(b.Level); break;
                                    case 2: result = string.Compare(a.Entry.ArchiveFile, b.Entry.ArchiveFile, StringComparison.Ordinal); break;
                                    case 3: result = string.Compare(a.Entry.ArchiveEntryName, b.Entry.ArchiveEntryName, StringComparison.Ordinal); break;
                                    case 4: result = a.Entry.Magic1.CompareTo(b.Entry.Magic1); break;
                                    case 5: result = a.Entry.Magic2.CompareTo(b.Entry.Magic2); break;
                                    case 6: result = string.Compare(a.Entry.Description, b.Entry.Description, StringComparison.Ordinal); break;
                                    case 7: result = a.Entry.Type.CompareTo(b.Entry.Type); break;
                                }

                                return sortSpec.SortDirection == ImGuiSortDirection.Descending ? -result : result;
                            });
                        }

                        sortSpecs.SpecsDirty = false;
                    }

                    for (int i = 0; i < _viewModel.FlatList.Count; i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(i.ToString());
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
                        ImGui.Text(_viewModel.FlatList[i].Entry.Type.ToString("G"));
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

                        ImGui.Text("Тип: ");
                        ImGui.SameLine();
                        ImGui.Text(entry.Type.ToString());

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