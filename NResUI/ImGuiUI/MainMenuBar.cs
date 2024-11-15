using System.Numerics;
using ImGuiNET;
using NativeFileDialogSharp;
using NResLib;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI
{
    public class MainMenuBar : IImGuiPanel
    {
        private readonly ExplorerViewModel _explorerViewModel;

        public MainMenuBar(ExplorerViewModel explorerViewModel)
        {
            _explorerViewModel = explorerViewModel;
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

                            _explorerViewModel.SetParseResult(parseResult, path);
                        }
                    }

                    if (_explorerViewModel.HasFile)
                    {
                        if (ImGui.MenuItem("Экспортировать"))
                        {
                            var result = Dialog.FolderPicker();

                            if (result.IsOk)
                            {
                                var path = result.Path;

                                Console.WriteLine(path);
                            }
                        }
                    }

                    if (ImGui.BeginMenu("Open Recent"))
                    {
                        ImGui.EndMenu();
                    }

                    if (ImGui.MenuItem("Exit"))
                    {
                        App.Instance.Exit();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    if (ImGui.MenuItem("Settings"))
                    {
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