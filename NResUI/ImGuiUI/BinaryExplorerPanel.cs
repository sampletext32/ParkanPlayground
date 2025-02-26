using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Text.Json;
using ImGuiNET;
using NativeFileDialogSharp;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class BinaryExplorerPanel : IImGuiPanel
{
    private readonly BinaryExplorerViewModel _viewModel;

    public BinaryExplorerPanel(BinaryExplorerViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        return;
        if (ImGui.Begin("Binary Explorer"))
        {
            if (ImGui.Button("Open File"))
            {
                OpenFile();
            }
        
            if (_viewModel.HasFile)
            {
                ImGui.SameLine();
                ImGui.Text(_viewModel.Path);
        
                if (ImGui.Button("Сохранить регионы"))
                {
                    File.WriteAllText("preset.json", JsonSerializer.Serialize(_viewModel.Regions));
                }
        
                ImGui.SameLine();
                if (ImGui.Button("Загрузить регионы"))
                {
                    _viewModel.Regions = JsonSerializer.Deserialize<List<Region>>(File.ReadAllText("preset.json"))!;
                }
        
                const int bytesPerRow = 16;
                if (ImGui.BeginTable("HexTable", bytesPerRow + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                {
                    ImGui.TableSetupColumn("Address", ImGuiTableColumnFlags.WidthFixed);
                    for (var i = 0; i < bytesPerRow; i++)
                    {
                        ImGui.TableSetupColumn(i.ToString());
                    }
        
                    ImGui.TableHeadersRow();
                    for (int i = 0; i < _viewModel.Data.Length; i += bytesPerRow)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text($"{i:x8} ");
        
                        for (int j = 0; j < 16; j++)
                        {
                            var index = i + j;
                            ImGui.TableNextColumn();
                            if (index < _viewModel.Data.Length)
                            {
                                uint? regionColor = GetRegionColor(i + j);
        
                                if (regionColor is not null)
                                {
                                    ImGui.PushStyleColor(ImGuiCol.Header, regionColor.Value);
                                }
        
                                if (ImGui.Selectable($"{_viewModel.Data[i + j]}##sel{i + j}", regionColor.HasValue))
                                {
                                    HandleRegionSelect(i + j);
                                }
        
                                if (regionColor is not null)
                                {
                                    ImGui.PopStyleColor();
                                }
                            }
                            else
                            {
                                ImGui.Text(" ");
                            }
                        }
                    }
        
                    ImGui.EndTable();
                }
        
                ImGui.SameLine();
        
                ImGui.SetNextItemWidth(200f);
                if (ImGui.ColorPicker4("NextColor", ref _viewModel.NextColor, ImGuiColorEditFlags.Float))
                {
                }
        
                if (ImGui.BeginTable("Регионы", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                {
                    ImGui.TableSetupColumn("Номер");
                    ImGui.TableSetupColumn("Старт");
                    ImGui.TableSetupColumn("Длина");
                    ImGui.TableSetupColumn("Значение");
                    ImGui.TableSetupColumn("Действия");
        
                    ImGui.TableHeadersRow();

                    for (int k = 0; k < _viewModel.Regions.Count; k++)
                    {
                        var region = _viewModel.Regions[k];
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(k.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(region.Begin.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(region.Length.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(region.Value ?? "unknown");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"float##f{k}"))
                        {
                            region.Value = BinaryPrimitives.ReadSingleLittleEndian(_viewModel.Data.AsSpan()[region.Begin..(region.Begin + region.Length)])
                                .ToString("F2");
                        }

                        ImGui.SameLine();
                        if (ImGui.Button($"int##i{k}"))
                        {
                            region.Value = BinaryPrimitives.ReadInt32LittleEndian(_viewModel.Data.AsSpan()[region.Begin..(region.Begin + region.Length)])
                                .ToString();
                        }

                        ImGui.SameLine();
                        if (ImGui.Button($"ASCII##a{k}"))
                        {
                            region.Value = Encoding.ASCII.GetString(_viewModel.Data.AsSpan()[region.Begin..(region.Begin + region.Length)]);
                        }

                        ImGui.SameLine();
                        if (ImGui.Button($"raw##r{k}"))
                        {
                            region.Value = string.Join(
                                "",
                                _viewModel.Data[region.Begin..(region.Begin + region.Length)]
                                    .Select(x => x.ToString("x2"))
                            );
                        }
                    }

                    ImGui.EndTable();
                }
            }
        
            ImGui.End();
        }
    }

    private uint? GetRegionColor(int index)
    {
        Region? inRegion = _viewModel.Regions.Find(x => x.Begin <= index && x.Begin + x.Length > index);

        return inRegion?.Color;
    }

    private void HandleRegionSelect(int index)
    {
        Region? inRegion = _viewModel.Regions.FirstOrDefault(x => x.Begin <= index && index < x.Begin + x.Length);

        if (inRegion is null)
        {
            // not in region
            Region? prependRegion;
            Region? appendRegion;

            if ((prependRegion = _viewModel.Regions.Find(x => x.Begin + x.Length == index)) is not null)
            {
                if (prependRegion.Color == GetImGuiColor(_viewModel.NextColor))
                {
                    prependRegion.Length += 1;
                    return;
                }
            }

            if ((appendRegion = _viewModel.Regions.Find(x => x.Begin - 1 == index)) is not null)
            {
                if (appendRegion.Color == GetImGuiColor(_viewModel.NextColor))
                {
                    appendRegion.Begin--;
                    appendRegion.Length += 1;
                    return;
                }
            }

            var color = ImGui.ColorConvertFloat4ToU32(_viewModel.NextColor);

            color = unchecked((uint) (0xFF << 24)) | color;
            _viewModel.Regions.Add(
                new Region()
                {
                    Begin = index,
                    Length = 1,
                    Color = color
                }
            );
        }
        else
        {
            if (inRegion.Length == 1)
            {
                _viewModel.Regions.Remove(inRegion);
            }
            else
            {
                if (inRegion.Begin == index)
                {
                    inRegion.Begin++;
                    inRegion.Length--;
                }

                if (inRegion.Begin + inRegion.Length - 1 == index)
                {
                    inRegion.Length--;
                }
            }
        }
    }

    private static uint GetImGuiColor(Vector4 vec)
    {
        var color = ImGui.ColorConvertFloat4ToU32(vec);

        color = unchecked((uint) (0xFF << 24)) | color;

        return color;
    }

    private bool OpenFile()
    {
        var result = Dialog.FileOpen("*");

        if (result.IsOk)
        {
            var path = result.Path;

            var bytes = File.ReadAllBytes(path);
            _viewModel.HasFile = true;
            _viewModel.Data = bytes;
            _viewModel.Path = path;
        }

        return false;
    }
}