using ImGuiNET;
using MissionTmaLib;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class MissionTmaExplorer : IImGuiPanel
{
    private readonly MissionTmaViewModel _viewModel;

    public MissionTmaExplorer(MissionTmaViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("Mission TMA Explorer"))
        {
            var mission = _viewModel.Mission;
            if (_viewModel.HasFile && mission is not null)
            {
                ImGui.Text("Путь к файлу: ");
                ImGui.SameLine();
                ImGui.Text(_viewModel.Path);

                if (ImGui.TreeNodeEx("Ареалы"))
                {
                    var (unusedHeader, arealCount, arealInfos) = mission.ArealData;

                    ImGui.Text("Неиспользуемый заголовок: ");
                    ImGui.SameLine();
                    ImGui.Text(unusedHeader.ToString());

                    ImGui.Text("Количество ареалов: ");
                    ImGui.SameLine();
                    ImGui.Text(arealCount.ToString());

                    if (ImGui.TreeNodeEx("Информация об ареалах"))
                    {
                        for (var i = 0; i < arealInfos.Count; i++)
                        {
                            var arealInfo = arealInfos[i];
                            if (ImGui.TreeNodeEx($"Ареал {i}"))
                            {
                                Utils.ShowHint("Кажется, что ареал это просто некая зона на карте");
                                ImGui.Text("Индекс: ");
                                ImGui.SameLine();
                                ImGui.Text(arealInfo.Index.ToString());

                                ImGui.Text("Количество координат: ");
                                ImGui.SameLine();
                                ImGui.Text(arealInfo.CoordsCount.ToString());

                                if (ImGui.BeginTable("Координаты", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                                {
                                    ImGui.TableSetupColumn("X");
                                    ImGui.TableSetupColumn("Y");
                                    ImGui.TableSetupColumn("Z");

                                    ImGui.TableHeadersRow();

                                    for (int k = 0; k < arealInfo.Coords.Count; k++)
                                    {
                                        ImGui.TableNextRow();
                                        ImGui.TableNextColumn();
                                        ImGui.Text(arealInfo.Coords[k].X.ToString("F2"));
                                        ImGui.TableNextColumn();
                                        ImGui.Text(arealInfo.Coords[k].Y.ToString("F2"));
                                        ImGui.TableNextColumn();
                                        ImGui.Text(arealInfo.Coords[k].Z.ToString("F2"));
                                    }

                                    ImGui.EndTable();
                                }
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Кланы"))
                {
                    var (clanFeatureSet, clanCount, clanInfos) = mission.ClansData;
                    
                    ImGui.Text("Фиче-сет: ");
                    Utils.ShowHint("Магическое число из файла, на основе которого игра читает разные секции о клане");
                    ImGui.SameLine();
                    ImGui.Text(clanFeatureSet.ToString());
                    
                    ImGui.Text("Количество кланов: ");
                    ImGui.SameLine();
                    ImGui.Text(clanCount.ToString());

                    if (ImGui.TreeNodeEx("Информация о кланах"))
                    {
                        for (var i = 0; i < clanInfos.Count; i++)
                        {
                            var clanInfo = clanInfos[i];
                            if (ImGui.TreeNodeEx($"Клан {i} - \"{clanInfo.ClanName}\""))
                            {
                                ImGui.Text("Неизвестное число 1: ");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.UnkInt1.ToString());

                                ImGui.Text("X: ");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.X.ToString());
                                ImGui.SameLine();
                                ImGui.Text(" Y: ");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.Y.ToString());
                                
                                ImGui.Text("Тип клана: ");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.ClanType.ToReadableString());
                                
                                ImGui.Text("Неизвестная строка 1: ");
                                Utils.ShowHint("Кажется это путь к файлу поведения (Behavior), но пока не понятно. Обычно пути соответствуют 2 файла.");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.UnkString2);

                                if (clanInfo.UnknownParts.Count > 0)
                                {
                                    ImGui.Text("Неизвестная часть");
                                    if (ImGui.BeginTable("Неизвестная часть##unk1", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                                    {
                                        ImGui.TableSetupColumn("Число 1");
                                        ImGui.TableSetupColumn("X");
                                        ImGui.TableSetupColumn("Y");
                                        ImGui.TableSetupColumn("Z");
                                        ImGui.TableSetupColumn("Число 2");
                                        ImGui.TableSetupColumn("Число 3");
                                        ImGui.TableHeadersRow();

                                        for (var i1 = 0; i1 < clanInfo.UnknownParts.Count; i1++)
                                        {
                                            var unkPart = clanInfo.UnknownParts[i1];
                                            ImGui.TableNextRow();
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkInt1.ToString());
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkVector.X.ToString());
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkVector.Y.ToString());
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkVector.Z.ToString());
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkInt2.ToString());
                                            ImGui.TableNextColumn();
                                            ImGui.Text(unkPart.UnkInt3.ToString());
                                        }

                                        ImGui.EndTable();
                                    }
                                }
                                else
                                {
                                    ImGui.Text("Отсутствует неизвестная часть");
                                }

                                ImGui.Text("Путь к файлу .trf: ");
                                Utils.ShowHint("Не до конца понятно, что означает, вероятно это NRes с деревом исследований");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.ResearchNResPath);
                                
                                ImGui.Text("Неизвестное число 3: ");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.UnkInt3.ToString());

                                ImGui.Text("Матрица союзников");
                                Utils.ShowHint("Если 1, то кланы - союзники, и не нападают друг на друга");
                                if (ImGui.BeginTable("Матрица союзников", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                                {
                                    ImGui.TableSetupColumn("Клан");
                                    ImGui.TableSetupColumn("Союзник?");
                                    ImGui.TableHeadersRow();
                                    
                                    foreach (var alliesMapKey in clanInfo.AlliesMap.Keys)
                                    {
                                        ImGui.TableNextRow();
                                        ImGui.TableNextColumn();
                                        ImGui.Text(alliesMapKey);
                                        ImGui.TableNextColumn();
                                        ImGui.Text(clanInfo.AlliesMap[alliesMapKey].ToString());
                                    }
                                    
                                    ImGui.EndTable();
                                }
                                
                                
                                ImGui.TreePop();
                            }
                        }

                        ImGui.TreePop();
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Объекты"))
                {
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text("Миссия не открыта");
            }

            ImGui.End();
        }
    }
}