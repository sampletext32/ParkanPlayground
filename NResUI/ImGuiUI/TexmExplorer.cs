using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class TexmExplorer : IImGuiPanel
{
    private readonly TexmExplorerViewModel _viewModel;

    public TexmExplorer(TexmExplorerViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("TEXM Explorer"))
        {
            if (!_viewModel.HasFile)
            {
                ImGui.Text("No TEXM opened");
            }
            else
            {
                if (_viewModel.TexmFile is not null)
                {
                    ImGui.Text("File: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.Path);

                    ImGui.Text("Header: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.TexmAscii);

                    ImGui.Text("Width: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Width.ToString());

                    ImGui.Text("Height: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Height.ToString());

                    ImGui.Text("MipMap Count: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.MipmapCount.ToString());

                    ImGui.Text("Stride: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Stride.ToString());

                    ImGui.Text("Magic1: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Magic1.ToString());

                    ImGui.Text("Magic2: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Magic2.ToString());

                    ImGui.Text("Format: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Format.ToString());

                    ImGui.Text("IsIndexed: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.IsIndexed.ToString());
                }
            }

            ImGui.End();
        }
    }
}