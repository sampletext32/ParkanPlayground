using CpDatLib;
using ImGuiNET;
using MissionTmaLib.Parsing;
using NResLib;
using NResUI.Abstractions;
using NResUI.Models;
using ScrLib;
using TexmLib;
using VarsetLib;

namespace NResUI.ImGuiUI;

public class NResExplorerPanel : IImGuiPanel
{
    private readonly NResExplorerViewModel _viewModel;
    private readonly TexmExplorerViewModel _texmExplorerViewModel;
    private readonly VarsetViewModel _varsetViewModel;
    private readonly CpDatSchemeViewModel _cpDatSchemeViewModel;
    private readonly MissionTmaViewModel _missionTmaViewModel;
    private readonly ScrViewModel _scrViewModel;

    public NResExplorerPanel(NResExplorerViewModel viewModel, TexmExplorerViewModel texmExplorerViewModel,
        VarsetViewModel varsetViewModel, CpDatSchemeViewModel cpDatSchemeViewModel, MissionTmaViewModel missionTmaViewModel, ScrViewModel scrViewModel)
    {
        _viewModel = viewModel;
        _texmExplorerViewModel = texmExplorerViewModel;
        _varsetViewModel = varsetViewModel;
        _cpDatSchemeViewModel = cpDatSchemeViewModel;
        _missionTmaViewModel = missionTmaViewModel;
        _scrViewModel = scrViewModel;
    }

    int contextMenuRow = -1;

    public void OnImGuiRender()
    {
        if (ImGui.Begin("NRes Explorer"))
        {
            ImGui.Text(
                "NRes - это файл-архив. Они имеют разные расширения. Примеры - Textures.lib, weapon.rlb, object.dlb, behpsp.res");
            ImGui.Separator();

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

                    if (ImGui.BeginTable("content", 12,
                            ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                    {
                        ImGui.TableSetupColumn("Тип файла");
                        ImGui.TableSetupColumn("Кол-во элементов");
                        ImGui.TableSetupColumn("Magic1");
                        ImGui.TableSetupColumn("Длина файла в байтах");
                        ImGui.TableSetupColumn("Размер элемента");
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

                            ImGui.Selectable("##row_select" + i, false, ImGuiSelectableFlags.SpanAllColumns);
                            if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                            {
                                Console.WriteLine("Context menu for row " + i);
                                contextMenuRow = i;
                                ImGui.OpenPopup("row_context_menu");
                            }

                            ImGui.SameLine();

                            ImGui.Text(_viewModel.Archive.Files[i].FileType);
                            ImGui.TableNextColumn();
                            ImGui.Text(_viewModel.Archive.Files[i].ElementCount.ToString());
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
                                    .ElementSize.ToString()
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

                        if (ImGui.BeginPopup("row_context_menu"))
                        {
                            if (contextMenuRow == -1 || contextMenuRow > _viewModel.Archive.Files.Count)
                            {
                                ImGui.Text("Broken context menu :(. Reopen");
                            }
                            else
                            {
                                var file = _viewModel.Archive.Files[contextMenuRow];
                                ImGui.Text("Actions for file " + file.FileName);
                                ImGui.TextDisabled("Program has no understading of file format(");
                                ImGui.Separator();
                                if (ImGui.MenuItem("Open as Texture TEXM"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = TexmParser.ReadFromStream(ms, file.FileName);

                                    _texmExplorerViewModel.SetParseResult(parseResult, Path.Combine(_viewModel.Path!, file.FileName));
                                    Console.WriteLine("Read TEXM from context menu");
                                }

                                if (ImGui.MenuItem("Open as Archive NRes"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = NResParser.ReadFile(ms);

                                    _viewModel.SetParseResult(parseResult, Path.Combine(_viewModel.Path!, file.FileName));
                                    Console.WriteLine("Read NRes from context menu");
                                }

                                if (ImGui.MenuItem("Open as Varset .var"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = VarsetParser.Parse(ms);

                                    _varsetViewModel.Items = parseResult;
                                    Console.WriteLine("Read Varset from context menu");
                                }

                                if (ImGui.MenuItem("Open as Scheme cp.dat"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = CpDatParser.Parse(ms);

                                    _cpDatSchemeViewModel.SetParseResult(parseResult, file.FileName);
                                    Console.WriteLine("Read cp.dat from context menu");
                                }

                                if (ImGui.MenuItem("Open as Mission .tma"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = MissionTmaParser.ReadFile(ms);

                                    _missionTmaViewModel.SetParseResult(parseResult, Path.Combine(_viewModel.Path!, file.FileName));
                                    Console.WriteLine("Read .tma from context menu");
                                }

                                if (ImGui.MenuItem("Open as Scripts .scr"))
                                {
                                    using var fs = new FileStream(_viewModel.Path!, FileMode.Open, FileAccess.Read,
                                        FileShare.Read);
                                    fs.Seek(file.OffsetInFile, SeekOrigin.Begin);

                                    var buffer = new byte[file.FileLength];

                                    fs.ReadExactly(buffer, 0, file.FileLength);

                                    using var ms = new MemoryStream(buffer);

                                    var parseResult = ScrParser.ReadFile(ms);

                                    _scrViewModel.SetParseResult(parseResult, Path.Combine(_viewModel.Path!, file.FileName));
                                    Console.WriteLine("Read .scr from context menu");
                                }
                            }

                            ImGui.EndPopup();
                        }

                        ImGui.EndTable();
                    }
                }
            }

            ImGui.End();
        }
    }
}