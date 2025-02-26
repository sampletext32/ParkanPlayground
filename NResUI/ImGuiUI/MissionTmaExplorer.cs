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
                ImGui.Columns(2);

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
                                        ImGui.Text(
                                            arealInfo.Coords[k]
                                                .X.ToString("F2")
                                        );
                                        ImGui.TableNextColumn();
                                        ImGui.Text(
                                            arealInfo.Coords[k]
                                                .Y.ToString("F2")
                                        );
                                        ImGui.TableNextColumn();
                                        ImGui.Text(
                                            arealInfo.Coords[k]
                                                .Z.ToString("F2")
                                        );
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

                                ImGui.Text("Скрипты поведения: ");
                                Utils.ShowHint("Пути к файлам .scr и .fml описывающих настройку объектов и поведение AI");
                                ImGui.SameLine();
                                ImGui.Text(clanInfo.ScriptsString);

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
                                        ImGui.Text(
                                            clanInfo.AlliesMap[alliesMapKey]
                                                .ToString()
                                        );
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
                    var gameObjectsData = mission.GameObjectsData;

                    ImGui.Text("Фиче-сет: ");
                    Utils.ShowHint("Магическое число из файла, на основе которого игра читает разные секции об объекте");
                    ImGui.SameLine();
                    ImGui.Text(gameObjectsData.GameObjectsFeatureSet.ToString());

                    ImGui.Text("Кол-во объектов: ");
                    ImGui.SameLine();
                    ImGui.Text(gameObjectsData.GameObjectsCount.ToString());

                    for (var i = 0; i < gameObjectsData.GameObjectInfos.Count; i++)
                    {
                        var gameObjectInfo = gameObjectsData.GameObjectInfos[i];
                        if (ImGui.TreeNodeEx($"Объект {i} - {gameObjectInfo.DatString}"))
                        {
                            ImGui.Text("Тип объекта: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.Type.ToReadableString());

                            ImGui.Text("Неизвестные флаги: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.UnknownFlags.ToString("X8"));

                            ImGui.Text("Путь к файлу .dat: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.DatString);

                            ImGui.Text("Индекс владеющего клана: ");
                            Utils.ShowHint("-1 если объект никому не принадлежит");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.OwningClanIndex.ToString());

                            ImGui.Text("Порядковый номер: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.Order.ToString());

                            ImGui.Text("Вектор позиции: ");
                            ImGui.SameLine();
                            ImGui.Text($"{gameObjectInfo.Position.X} : {gameObjectInfo.Position.Y} : {gameObjectInfo.Position.Z}");

                            ImGui.Text("Вектор поворота: ");
                            ImGui.SameLine();
                            ImGui.Text($"{gameObjectInfo.Rotation.X} : {gameObjectInfo.Rotation.Y} : {gameObjectInfo.Rotation.Z}");

                            ImGui.Text("Вектор масштаба: ");
                            ImGui.SameLine();
                            ImGui.Text($"{gameObjectInfo.Scale.X} : {gameObjectInfo.Scale.Y} : {gameObjectInfo.Scale.Z}");

                            ImGui.Text("Неизвестная строка 2: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.UnknownString2);

                            ImGui.Text("Неизвестное число 4: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.UnknownInt4.ToString());

                            ImGui.Text("Неизвестное число 5: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.UnknownInt5.ToString());

                            ImGui.Text("Неизвестное число 6: ");
                            ImGui.SameLine();
                            ImGui.Text(gameObjectInfo.UnknownInt6.ToString());

                            if (ImGui.TreeNodeEx("Настройки"))
                            {
                                ImGui.Text("Неиспользуемый заголовок: ");
                                ImGui.SameLine();
                                ImGui.Text(gameObjectInfo.Settings.Unused.ToString());
                                ImGui.Text("Кол-во настроек: ");
                                ImGui.SameLine();
                                ImGui.Text(gameObjectInfo.Settings.SettingsCount.ToString());
                                ImGui.Text("0 - дробное число, 1 - целое число");
                                if (ImGui.BeginTable("Настройки", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                                {
                                    ImGui.TableSetupColumn("Тип");
                                    ImGui.TableSetupColumn("Число 1");
                                    ImGui.TableSetupColumn("Число 2");
                                    ImGui.TableSetupColumn("Число 3");
                                    ImGui.TableSetupColumn("Название");
                                    ImGui.TableHeadersRow();

                                    for (var i1 = 0; i1 < gameObjectInfo.Settings.Settings.Count; i1++)
                                    {
                                        var setting = gameObjectInfo.Settings.Settings[i1];
                                        
                                        ImGui.TableNextRow();
                                        ImGui.TableNextColumn();
                                        ImGui.Text(setting.SettingType.ToString());
                                        ImGui.TableNextColumn();
                                        ImGui.Text(setting.SettingType == 0 ? setting.Unk1.AsFloat.ToString() : setting.Unk1.AsInt.ToString());
                                        ImGui.TableNextColumn();
                                        ImGui.Text(setting.SettingType == 0 ? setting.Unk1.AsFloat.ToString() : setting.Unk1.AsInt.ToString());
                                        ImGui.TableNextColumn();
                                        ImGui.Text(setting.SettingType == 0 ? setting.Unk1.AsFloat.ToString() : setting.Unk1.AsInt.ToString());
                                        ImGui.TableNextColumn();
                                        ImGui.Text(setting.Name);
                                    }

                                    ImGui.EndTable();
                                }

                                ImGui.TreePop();
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.Text("LAND строка: ");
                    Utils.ShowHint("Видимо это путь к настройкам поверхности");
                    ImGui.SameLine();
                    ImGui.Text(gameObjectsData.LandString);

                    ImGui.Text("Неизвестное число: ");
                    ImGui.SameLine();
                    ImGui.Text(gameObjectsData.UnknownInt.ToString());

                    ImGui.Text("Техническое описание: ");
                    ImGui.SameLine();
                    ImGui.Text(gameObjectsData.MissionTechDescription?.Replace((char)0xcd, '.') ?? "Отсутствует");

                    var lodeData = gameObjectsData.LodeData;
                    if (lodeData is not null)
                    {
                        ImGui.Text("Информация о LOD-ах");

                        ImGui.Text("Неиспользуемый заголовок: ");
                        ImGui.SameLine();
                        ImGui.Text(lodeData.Unused.ToString());

                        ImGui.Text("Кол-во LOD-ов: ");
                        ImGui.SameLine();
                        ImGui.Text(lodeData.LodeCount.ToString());
                        
                        if (ImGui.BeginTable("Информация о лодах", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX))
                        {
                            ImGui.TableSetupColumn("X");
                            ImGui.TableSetupColumn("Y");
                            ImGui.TableSetupColumn("Z");
                            ImGui.TableSetupColumn("Число 1");
                            ImGui.TableSetupColumn("Флаги 2");
                            ImGui.TableSetupColumn("Число 3");
                            ImGui.TableSetupColumn("Число 4");
                            ImGui.TableHeadersRow();

                            for (var i1 = 0; i1 < lodeData.Lodes.Count; i1++)
                            {
                                var lode = lodeData.Lodes[i1];
                                        
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownVector.X.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownVector.Y.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownVector.Z.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownInt1.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownFlags2.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownFloat.ToString());
                                ImGui.TableNextColumn();
                                ImGui.Text(lode.UnknownInt3.ToString());
                            }

                            ImGui.EndTable();
                        }
                    }
                    else
                    {
                        ImGui.Text("Информаия о LOD-ах отсутствует");
                    }
                    

                    ImGui.TreePop();
                }
                
                ImGui.NextColumn();
                
                ImGui.Text("Тут хочу сделать карту");
            }
            else
            {
                ImGui.Text("Миссия не открыта");
            }

            ImGui.End();
        }
    }
}