﻿using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class ExplorerPanel : IImGuiPanel
{
    private readonly ExplorerViewModel _viewModel;

    public ExplorerPanel(ExplorerViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("Explorer"))
        {
            if (!_viewModel.HasFile)
            {
                ImGui.Text("No NRes is opened");
            }
            else
            {
                if (_viewModel.Error != null)
                {
                    ImGui.Text(_viewModel.Error);
                }

                if (_viewModel.Archive is not null)
                {
                    ImGui.Text(_viewModel.Path);
                    
                    ImGui.Text("Header: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.Archive.Header.NRes);
                    ImGui.Text("Version: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.Archive.Header.Version.ToString());
                    ImGui.Text("File Count: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.Archive.Header.FileCount.ToString());
                    ImGui.Text("Total File Length: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.Archive.Header.TotalFileLengthBytes.ToString());


                    if (ImGui.BeginTable("content", 11))
                    {
                        ImGui.TableSetupColumn("Тип файла");
                        ImGui.TableSetupColumn("Magic1");
                        ImGui.TableSetupColumn("Длина файла в байтах");
                        ImGui.TableSetupColumn("Magic2");
                        ImGui.TableSetupColumn("Имя файла");
                        ImGui.TableSetupColumn("Magic3");
                        ImGui.TableSetupColumn("Magic4");
                        ImGui.TableSetupColumn("Magic5");
                        ImGui.TableSetupColumn("Magic6");
                        ImGui.TableSetupColumn("Смещение в байтах");
                        ImGui.TableSetupColumn("Индекс в файле");

                        ImGui.TableHeadersRow();

                        for (int i = 0; i < _viewModel.Archive.Files.Count; i++)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text(_viewModel.Archive.Files[i].FileType);
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic1.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .FileLength.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic2.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(_viewModel.Archive.Files[i].FileName);
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic3.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic4.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic5.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Magic6.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .OffsetInFile.ToString()
                            );
                            ImGui.TableNextColumn();
                            ImGui.Text(
                                _viewModel.Archive.Files[i]
                                    .Index.ToString()
                            );
                        }

                        ImGui.EndTable();
                    }
                }
            }

            ImGui.End();
        }
    }
}