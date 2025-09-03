using System.Numerics;
using CpDatLib;
using ImGuiNET;
using MissionTmaLib;
using MissionTmaLib.Parsing;
using NativeFileDialogSharp;
using NResLib;
using NResUI.Abstractions;
using NResUI.Models;
using ScrLib;
using TexmLib;
using VarsetLib;

namespace NResUI.ImGuiUI
{
    public class MainMenuBar(
        NResExplorerViewModel nResExplorerViewModel,
        TexmExplorerViewModel texmExplorerViewModel,
        ScrViewModel scrViewModel,
        MissionTmaViewModel missionTmaViewModel,
        VarsetViewModel varsetViewModel,
        CpDatSchemeViewModel cpDatSchemeViewModel,
        MessageBoxModalPanel messageBox)
        : IImGuiPanel
    {
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

                            nResExplorerViewModel.SetParseResult(parseResult, path);
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

                            texmExplorerViewModel.SetParseResult(parseResult, path);
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

                            texmExplorerViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read TFNT TEXM");
                        }
                    }

                    if (ImGui.MenuItem("Open Mission TMA"))
                    {
                        var result = Dialog.FileOpen("tma");

                        if (result.IsOk)
                        {
                            var path = result.Path;
                            var parseResult = MissionTmaParser.ReadFile(path);

                            missionTmaViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read TMA");
                        }
                    }

                    if (ImGui.MenuItem("Open SCR Scripts File"))
                    {
                        var result = Dialog.FileOpen("scr");

                        if (result.IsOk)
                        {
                            var path = result.Path;
                            var parseResult = ScrParser.ReadFile(path);

                            scrViewModel.SetParseResult(parseResult, path);
                            Console.WriteLine("Read SCR");
                        }
                    }

                    if (ImGui.MenuItem("Open Varset File"))
                    {
                        var result = Dialog.FileOpen("var");

                        if (result.IsOk)
                        {
                            var path = result.Path;
                            var parseResult = VarsetParser.Parse(path);

                            varsetViewModel.Items = parseResult;

                            Console.WriteLine("Read VARSET");
                        }
                    }

                    if (ImGui.MenuItem("Open cp .dat Scheme File"))
                    {
                        var result = Dialog.FileOpen("dat");

                        if (result.IsOk)
                        {
                            var path = result.Path;
                            var parseResult = CpDatParser.Parse(path);

                            cpDatSchemeViewModel.SetParseResult(parseResult, path);

                            Console.WriteLine("Read cp .dat");
                        }
                    }

                    if (nResExplorerViewModel.HasFile)
                    {
                        if (ImGui.MenuItem("Экспортировать NRes"))
                        {
                            var result = Dialog.FolderPicker();

                            if (result.IsOk)
                            {
                                var path = result.Path;

                                NResExporter.Export(nResExplorerViewModel.Archive!, path, nResExplorerViewModel.Path!);

                                messageBox.Show("Успешно экспортировано");
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