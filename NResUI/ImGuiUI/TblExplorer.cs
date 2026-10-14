using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using TblLib;

namespace NResUI.ImGuiUI;

public class TblExplorer : IImGuiPanel
{
    private readonly TblViewModel _viewModel;

    public TblExplorer(TblViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("Input Table Explorer"))
        {
            ImGui.Text("m1.tbl, m2.tbl, hero.tbl - привязки клавиш и мыши к командам ручного управления");
            ImGui.Separator();

            if (_viewModel.Error is not null)
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0.3f, 0.3f, 1), _viewModel.Error);
            }

            if (_viewModel.HasFile)
            {
                ImGui.Text("Путь к файлу: ");
                ImGui.SameLine();
                ImGui.Text(_viewModel.Path);
                ImGui.Text($"Привязок: {_viewModel.Bindings.Count}");

                const ImGuiTableFlags flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit |
                                              ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
                if (ImGui.BeginTable("Привязки", 10, flags))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn("Устройство");
                    ImGui.TableSetupColumn("Модификатор");
                    ImGui.TableSetupColumn("Клавиша");
                    ImGui.TableSetupColumn("Событие");
                    ImGui.TableSetupColumn("Класс предмета");
                    ImGui.TableSetupColumn("Команда");
                    ImGui.TableSetupColumn("Значение");
                    ImGui.TableSetupColumn("Предмет");
                    ImGui.TableSetupColumn("Режим");
                    ImGui.TableSetupColumn("Комментарий", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableHeadersRow();

                    foreach (var b in _viewModel.Bindings)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Device == TblDevice.Key ? "KEY" : "MOUSE");
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Modifier);
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Scan);
                        ImGui.TableNextColumn();
                        ImGui.Text(b.IsPress ? "нажатие" : "отпускание");
                        ImGui.TableNextColumn();
                        ImGui.Text(b.ItemClass);
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Command);
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Value.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(b.ItemIndex.ToString());
                        ImGui.TableNextColumn();
                        ImGui.Text(b.RampTimeMs == 0 ? b.Mode : $"{b.Mode} (+{b.RampStep} / {b.RampTimeMs} мс)");
                        ImGui.TableNextColumn();
                        ImGui.Text(b.Comment);
                    }

                    ImGui.EndTable();
                }
            }

            ImGui.End();
        }
    }
}
