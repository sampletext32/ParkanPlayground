using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using Silk.NET.OpenGL;

namespace NResUI.ImGuiUI;

public class TexmExplorer : IImGuiPanel
{
    private readonly TexmExplorerViewModel _viewModel;
    private readonly GL _openGl;

    public TexmExplorer(TexmExplorerViewModel viewModel, GL openGl)
    {
        _viewModel = viewModel;
        _openGl = openGl;
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

                    ImGui.Text("Magic1 (possibly ddsCaps): ");
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

                    ImGui.Checkbox("Включить чёрный фон", ref _viewModel.IsBlackBgEnabled);
                    ImGui.Checkbox("Включить белый фон", ref _viewModel.IsWhiteBgEnabled);

                    if (_viewModel.IsWhiteBgEnabled && _viewModel.IsBlackBgEnabled)
                    {
                        _viewModel.IsBlackBgEnabled = false;
                        _viewModel.IsWhiteBgEnabled = false;
                    }
                    
                    _viewModel.GenerateGlTextures(_openGl);

                    var drawList = ImGui.GetWindowDrawList();
                    for (var index = 0; index < _viewModel.GlTextures.Count; index++)
                    {
                        var glTexture = _viewModel.GlTextures[index];
                        var screenPos = ImGui.GetCursorScreenPos();
                        if (_viewModel.IsBlackBgEnabled)
                        {
                            drawList.AddRectFilled(screenPos, screenPos + new Vector2(glTexture.Width, glTexture.Height), 0xFF000000);
                        }
                        else if (_viewModel.IsWhiteBgEnabled)
                        {
                            drawList.AddRectFilled(screenPos, screenPos + new Vector2(glTexture.Width, glTexture.Height), 0xFFFFFFFF);
                        }

                        ImGui.Image((IntPtr) glTexture.GlTexture, new Vector2(glTexture.Width, glTexture.Height));
                        ImGui.SameLine();

                        if (ImGui.IsItemHovered())
                        {
                            var mousePos = ImGui.GetMousePos();
                            var relativePos = mousePos - screenPos;
                            
                            ImGui.Text("Hovering over: ");
                            ImGui.SameLine();
                            ImGui.Text(relativePos.ToString());
                        }
                    }
                }
            }

            ImGui.End();
        }
    }
}