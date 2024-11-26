using System.Numerics;
using ImGuiNET;
using MissionTmaLib.Parsing;
using NativeFileDialogSharp;
using NResLib;
using NResUI.Abstractions;
using NResUI.Models;
using TexmLib;

namespace NResUI.ImGuiUI
{
    public class MainMenuBar : IImGuiPanel
    {
        private readonly NResExplorerViewModel _nResExplorerViewModel;
        private readonly TexmExplorerViewModel _texmExplorerViewModel;
        private readonly MissionTmaViewModel _missionTmaViewModel;

        private readonly MessageBoxModalPanel _messageBox;
        public MainMenuBar(NResExplorerViewModel nResExplorerViewModel, TexmExplorerViewModel texmExplorerViewModel, MessageBoxModalPanel messageBox, MissionTmaViewModel missionTmaViewModel)
        {
            _nResExplorerViewModel = nResExplorerViewModel;
            _texmExplorerViewModel = texmExplorerViewModel;
            _messageBox = messageBox;
            _missionTmaViewModel = missionTmaViewModel;
        }

        public void OnImGuiRender()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open NRes"))
                    {
                        var result = Dialog.FileOpen();

                        if (result.IsOk)
                        {
                            var path = result.Path;

                            var parseResult = NResParser.ReadFile(path);

                            _nResExplorerViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read NRES");
                        }
                    }

                    if (ImGui.MenuItem("Open TEXM"))
                    {
                        var result = Dialog.FileOpen();

                        if (result.IsOk)
                        {
                            var path = result.Path;

                            using var fs = new FileStream(path, FileMode.Open);
                            
                            var parseResult = TexmParser.ReadFromStream(fs, path);

                            _texmExplorerViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read TEXM");
                        }
                    }

                    if (ImGui.MenuItem("Open TFNT TEXM"))
                    {
                        var result = Dialog.FileOpen();

                        if (result.IsOk)
                        {
                            var path = result.Path;

                            using var fs = new FileStream(path, FileMode.Open);

                            fs.Seek(4116, SeekOrigin.Begin);
                            
                            var parseResult = TexmParser.ReadFromStream(fs, path);

                            _texmExplorerViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read TEXM");
                        }
                    }

                    if (ImGui.MenuItem("Open Mission TMA"))
                    {
                        var result = Dialog.FileOpen("tma");

                        if (result.IsOk)
                        {
                            var path = result.Path;
                            var parseResult = MissionTmaParser.ReadFile(path);
                            
                            _missionTmaViewModel.SetParseResult(parseResult, path);
                        }
                    }

                    if (_nResExplorerViewModel.HasFile)
                    {
                        if (ImGui.MenuItem("Экспортировать NRes"))
                        {
                            var result = Dialog.FolderPicker();

                            if (result.IsOk)
                            {
                                var path = result.Path;
                                
                                NResExporter.Export(_nResExplorerViewModel.Archive!, path, _nResExplorerViewModel.Path!);
                                
                                _messageBox.Show("Успешно экспортировано");
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

    }
}