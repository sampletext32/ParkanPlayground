using System.Numerics;
using ImGuiNET;
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

        private readonly MessageBoxModalPanel _messageBox;
        public MainMenuBar(NResExplorerViewModel nResExplorerViewModel, TexmExplorerViewModel texmExplorerViewModel, MessageBoxModalPanel messageBox)
        {
            _nResExplorerViewModel = nResExplorerViewModel;
            _texmExplorerViewModel = texmExplorerViewModel;
            _messageBox = messageBox;
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

        // This is a direct port of imgui_demo.cpp HelpMarker function

        // https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp#L190

        private void ShowHint(string message)
        {
            // ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                // Change background transparency
                ImGui.PushStyleColor(
                    ImGuiCol.PopupBg,
                    new Vector4(
                        1,
                        1,
                        1,
                        0.8f
                    )
                );
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(message);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
                ImGui.PopStyleColor();
            }
        }
    }
}