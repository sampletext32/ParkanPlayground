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
            ImGui.Text("TEXM - это файл текстуры. Их можно найти внутри NRes архивов, например Textures.lib");
            ImGui.Separator();
            
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
                    ImGui.Text(_viewModel.TexmFile.Header.FormatOptionFlags.ToString());

                    ImGui.Text("Format: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.Header.Format.ToString());

                    ImGui.Text("IsIndexed: ");
                    ImGui.SameLine();
                    ImGui.Text(_viewModel.TexmFile.IsIndexed.ToString());

                    if (_viewModel.TexmFile.Pages is not null)
                    {
                        ImGui.Text("Page Section: ");
                        ImGui.SameLine();
                        ImGui.Text(_viewModel.TexmFile.Pages.Page);

                        ImGui.Text("Page Count: ");
                        ImGui.SameLine();
                        ImGui.Text(_viewModel.TexmFile.Pages.Count.ToString());

                        if (ImGui.TreeNodeEx("Показать координаты атласа"))
                        {
                            if (ImGui.BeginTable("pages-table", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                            {
                                ImGui.TableSetupColumn("X");
                                ImGui.TableSetupColumn("Ширина");
                                ImGui.TableSetupColumn("Y");
                                ImGui.TableSetupColumn("Height");
                                ImGui.TableHeadersRow();

                                for (int i = 0; i < _viewModel.TexmFile.Pages.Count; i++)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.Text(
                                        _viewModel.TexmFile.Pages.Items[i]
                                            .X.ToString()
                                    );
                                    ImGui.TableNextColumn();
                                    ImGui.Text(
                                        _viewModel.TexmFile.Pages.Items[i]
                                            .Width.ToString()
                                    );
                                    ImGui.TableNextColumn();
                                    ImGui.Text(
                                        _viewModel.TexmFile.Pages.Items[i]
                                            .Y.ToString()
                                    );
                                    ImGui.TableNextColumn();
                                    ImGui.Text(
                                        _viewModel.TexmFile.Pages.Items[i]
                                            .Height.ToString()
                                    );
                                }

                                ImGui.EndTable();
                            }
                            
                            ImGui.TreePop();
                        }
                    }

                    ImGui.Checkbox("Включить чёрный фон", ref _viewModel.IsBlackBgEnabled);
                    ImGui.SameLine();
                    ImGui.Checkbox("Включить белый фон", ref _viewModel.IsWhiteBgEnabled);
                    ImGui.SameLine();
                    ImGui.Checkbox("Увеличить в 2 раза", ref _viewModel.DoubleSize);


                    if (_viewModel.TexmFile.Pages is not null)
                    {
                        ImGui.SameLine();
                        ImGui.Checkbox("Отображать атлас", ref _viewModel.ViewPages);
                    }

                    if (_viewModel is {IsWhiteBgEnabled: true, IsBlackBgEnabled: true})
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
                        Vector2 imageSize = new Vector2(glTexture.Width, glTexture.Height);
                        if (_viewModel.DoubleSize)
                        {
                            imageSize *= 2;
                        }

                        if (_viewModel.IsBlackBgEnabled)
                        {
                            drawList.AddRectFilled(screenPos, screenPos + imageSize, 0xFF000000);
                        }
                        else if (_viewModel.IsWhiteBgEnabled)
                        {
                            drawList.AddRectFilled(screenPos, screenPos + imageSize, 0xFFFFFFFF);
                        }

                        ImGui.Image((IntPtr) glTexture.GlTexture, imageSize);
                        ImGui.SameLine();

                        if (_viewModel.ViewPages && _viewModel.TexmFile.Pages is not null)
                        {
                            for (int i = 0; i < _viewModel.TexmFile.Pages.Items.Count; i++)
                            {
                                var page = _viewModel.TexmFile.Pages.Items[i];
                                drawList.AddRect(
                                    screenPos + (new Vector2(page.X, page.Y) * (_viewModel.DoubleSize ? 2 : 1) / (int)Math.Pow(2, index)),
                                    screenPos + (new Vector2(page.X + page.Width, page.Y + page.Height) * (_viewModel.DoubleSize ? 2 : 1) / (int)Math.Pow(2, index)),
                                    0xFF0000FF
                                );
                            }
                        }

                        if (ImGui.IsItemHovered())
                        {
                            var mousePos = ImGui.GetMousePos();
                            var relativePos = (mousePos - screenPos) / (_viewModel.DoubleSize
                                ? 2
                                : 1);

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