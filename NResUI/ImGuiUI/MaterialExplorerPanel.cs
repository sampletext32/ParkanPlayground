using System.Numerics;
using ImGuiNET;
using MaterialLib;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

public class MaterialExplorerPanel : IImGuiPanel
{
    private readonly MaterialViewModel _viewModel;

    public MaterialExplorerPanel(MaterialViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (ImGui.Begin("Material Explorer"))
        {
            if (!_viewModel.HasFile)
            {
                ImGui.Text("No Material file opened");
            }
            else if (_viewModel.Error != null)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), $"Error: {_viewModel.Error}");
            }
            else
            {
                var mat = _viewModel.MaterialFile!;
                
                ImGui.Text($"File: {_viewModel.FilePath}");
                ImGui.Separator();

                // === FILE READ SEQUENCE ===
                
                if (ImGui.CollapsingHeader("1. Metadata & Derived Fields", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.TextDisabled("(Not from file content)");
                    ImGui.Text($"File Name: {mat.FileName}");
                    ImGui.Text($"Version (ElementCount): {mat.Version}");
                    ImGui.Text($"Magic1: {mat.Magic1}");
                    
                    ImGui.Separator();
                    ImGui.TextDisabled("(Derived from Version)");
                    ImGui.Text($"Material Rendering Type: {mat.MaterialRenderingType}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("0=Standard, 1=Special, 2=Particle/Effect");
                    }
                    ImGui.Text($"Supports Bump Mapping: {mat.SupportsBumpMapping}");
                    ImGui.Text($"Is Particle Effect: {mat.IsParticleEffect}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("0=Normal material, 8=Particle/Effect (e.g., jet engines)");
                    }
                }

                if (ImGui.CollapsingHeader("2. File Header (Read Order)", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Text($"Stage Count: {mat.Stages.Count}");
                    ImGui.SameLine();
                    ImGui.TextDisabled("(2 bytes, ushort)");
                    
                    ImGui.Text($"Animation Count: {mat.Animations.Count}");
                    ImGui.SameLine();
                    ImGui.TextDisabled("(2 bytes, ushort)");
                    
                    ImGui.Separator();
                    ImGui.TextDisabled($"(Read if Magic1 >= 2)");
                    ImGui.Text($"Source Blend Mode: {mat.SourceBlendMode}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(GetBlendModeDescription(mat.SourceBlendMode));
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("(1 byte)");
                    
                    ImGui.Text($"Dest Blend Mode: {mat.DestBlendMode}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(GetBlendModeDescription(mat.DestBlendMode));
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("(1 byte)");
                    
                    ImGui.Separator();
                    ImGui.TextDisabled($"(Read if Magic1 > 2)");
                    ImGui.Text($"Global Alpha Multiplier: {mat.GlobalAlphaMultiplier:F3}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Always 1.0 in all 628 materials - global alpha multiplier");
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("(4 bytes, float)");
                    
                    ImGui.Separator();
                    ImGui.TextDisabled($"(Read if Magic1 > 3)");
                    ImGui.Text($"Global Emissive Intensity: {mat.GlobalEmissiveIntensity:F3}");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("0=no glow (99.4%), rare values: 1000, 10000");
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("(4 bytes, float)");
                }

                if (ImGui.CollapsingHeader($"3. Stages ({mat.Stages.Count}) - 34 bytes each", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    for (int i = 0; i < mat.Stages.Count; i++)
                    {
                        var stage = mat.Stages[i];
                        if (ImGui.TreeNode($"Stage {i}: {stage.TextureName}"))
                        {
                            ImGui.TextDisabled("=== File Read Order (34 bytes) ===");
                            
                            // 1. Ambient (4 bytes)
                            ImGui.Text("1. Ambient Color:");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(4 bytes, A scaled by 0.01)");
                            var ambient = new Vector4(stage.AmbientR, stage.AmbientG, stage.AmbientB, stage.AmbientA);
                            ImGui.ColorEdit4("##Ambient", ref ambient, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine();
                            ImGui.Text($"RGBA: ({stage.AmbientR:F3}, {stage.AmbientG:F3}, {stage.AmbientB:F3}, {stage.AmbientA:F3})");
                            
                            // 2. Diffuse (4 bytes)
                            ImGui.Text("2. Diffuse Color:");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(4 bytes)");
                            var diffuse = new Vector4(stage.DiffuseR, stage.DiffuseG, stage.DiffuseB, stage.DiffuseA);
                            ImGui.ColorEdit4("##Diffuse", ref diffuse, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine();
                            ImGui.Text($"RGBA: ({stage.DiffuseR:F3}, {stage.DiffuseG:F3}, {stage.DiffuseB:F3}, {stage.DiffuseA:F3})");
                            
                            // 3. Specular (4 bytes)
                            ImGui.Text("3. Specular Color:");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(4 bytes)");
                            var specular = new Vector4(stage.SpecularR, stage.SpecularG, stage.SpecularB, stage.SpecularA);
                            ImGui.ColorEdit4("##Specular", ref specular, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine();
                            ImGui.Text($"RGBA: ({stage.SpecularR:F3}, {stage.SpecularG:F3}, {stage.SpecularB:F3}, {stage.SpecularA:F3})");
                            
                            // 4. Emissive (4 bytes)
                            ImGui.Text("4. Emissive Color:");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(4 bytes)");
                            var emissive = new Vector4(stage.EmissiveR, stage.EmissiveG, stage.EmissiveB, stage.EmissiveA);
                            ImGui.ColorEdit4("##Emissive", ref emissive, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine();
                            ImGui.Text($"RGBA: ({stage.EmissiveR:F3}, {stage.EmissiveG:F3}, {stage.EmissiveB:F3}, {stage.EmissiveA:F3})");
                            
                            // 5. Power (1 byte)
                            ImGui.Text($"5. Power: {stage.Power:F3}");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(1 byte -> float)");
                            
                            // 6. Texture Stage Index (1 byte)
                            ImGui.Text($"6. Texture Stage Index: {stage.TextureStageIndex}");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("255 = not set/default, 0-47 = reference to specific texture stage");
                            }
                            ImGui.SameLine();
                            ImGui.TextDisabled("(1 byte)");
                            
                            // 7. Texture Name (16 bytes)
                            ImGui.Text($"7. Texture Name: {stage.TextureName}");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(16 bytes, ASCII)");

                            ImGui.TreePop();
                        }
                    }
                }

                if (ImGui.CollapsingHeader($"4. Animations ({mat.Animations.Count})"))
                {
                    for (int i = 0; i < mat.Animations.Count; i++)
                    {
                        var anim = mat.Animations[i];
                        if (ImGui.TreeNode($"Anim {i}: {anim.Target} ({anim.LoopMode})"))
                        {
                            ImGui.TextDisabled("=== File Read Order ===");
                            
                            // Combined field (4 bytes)
                            ImGui.Text("1. Target & Loop Mode:");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(4 bytes combined)");
                            
                            ImGui.Indent();
                            ImGui.Text($"Target (bits 3-31): {anim.Target}");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(anim.TargetDescription);
                            }
                            
                            ImGui.Text($"Loop Mode (bits 0-2): {anim.LoopMode}");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(GetAnimationLoopModeDescription(anim.LoopMode));
                            }
                            ImGui.Unindent();
                            
                            // Key count (2 bytes)
                            ImGui.Text($"2. Key Count: {anim.Keys.Count}");
                            ImGui.SameLine();
                            ImGui.TextDisabled("(2 bytes, ushort)");
                            
                            // Keys (6 bytes each)
                            ImGui.Text($"3. Keys ({anim.Keys.Count} × 6 bytes):");
                            if (ImGui.BeginTable($"keys_{i}", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                            {
                                ImGui.TableSetupColumn("#");
                                ImGui.TableSetupColumn("Stage Index (2 bytes)");
                                ImGui.TableSetupColumn("Duration ms (2 bytes)");
                                ImGui.TableSetupColumn("Interpolation (2 bytes)");
                                ImGui.TableHeadersRow();

                                int keyIdx = 0;
                                foreach (var key in anim.Keys)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    ImGui.Text(keyIdx.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(key.StageIndex.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(key.DurationMs.ToString());
                                    ImGui.TableNextColumn();
                                    ImGui.Text(key.InterpolationCurve.ToString());
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("Always 0 = linear interpolation");
                                    }
                                    keyIdx++;
                                }
                                ImGui.EndTable();
                            }
                            ImGui.TreePop();
                        }
                    }
                }
            }
        }
        ImGui.End();
    }

    private static string GetBlendModeDescription(BlendMode mode)
    {
        return mode switch
        {
            BlendMode.Zero => "D3DBLEND_ZERO: Blend factor is (0, 0, 0, 0)",
            BlendMode.One => "D3DBLEND_ONE: Blend factor is (1, 1, 1, 1)",
            BlendMode.SrcColor => "D3DBLEND_SRCCOLOR: Blend factor is (Rs, Gs, Bs, As)",
            BlendMode.InvSrcColor => "D3DBLEND_INVSRCCOLOR: Blend factor is (1-Rs, 1-Gs, 1-Bs, 1-As)",
            BlendMode.SrcAlpha => "D3DBLEND_SRCALPHA: Blend factor is (As, As, As, As)",
            BlendMode.InvSrcAlpha => "D3DBLEND_INVSRCALPHA: Blend factor is (1-As, 1-As, 1-As, 1-As)",
            BlendMode.DestAlpha => "D3DBLEND_DESTALPHA: Blend factor is (Ad, Ad, Ad, Ad)",
            BlendMode.InvDestAlpha => "D3DBLEND_INVDESTALPHA: Blend factor is (1-Ad, 1-Ad, 1-Ad, 1-Ad)",
            BlendMode.DestColor => "D3DBLEND_DESTCOLOR: Blend factor is (Rd, Gd, Bd, Ad)",
            BlendMode.InvDestColor => "D3DBLEND_INVDESTCOLOR: Blend factor is (1-Rd, 1-Gd, 1-Bd, 1-Ad)",
            BlendMode.SrcAlphaSat => "D3DBLEND_SRCALPHASAT: Blend factor is (f, f, f, 1); f = min(As, 1-Ad)",
            BlendMode.BothSrcAlpha => "D3DBLEND_BOTHSRCALPHA: Source blend factor is (As, As, As, As) and destination blend factor is (1-As, 1-As, 1-As, 1-As)",
            BlendMode.BothInvSrcAlpha => "D3DBLEND_BOTHINVSRCALPHA: Source blend factor is (1-As, 1-As, 1-As, 1-As) and destination blend factor is (As, As, As, As)",
            BlendMode.Unknown => "Unknown/Default (0xFF): No blending or opaque",
            _ => "Unknown blend mode"
        };
    }

    private static string GetAnimationTargetDescription(AnimationTarget target)
    {
        // NOTE: This method is now only used for fallback/debugging
        // The actual UI uses MaterialAnimation.TargetDescription which is precomputed during parsing
        return $"Target flags: 0x{(int)target:X}";
    }

    private static string GetAnimationLoopModeDescription(AnimationLoopMode mode)
    {
        return mode switch
        {
            AnimationLoopMode.Loop => "Loop: Animation repeats continuously from start to end",
            AnimationLoopMode.PingPong => "PingPong: Animation plays forward then backward repeatedly",
            AnimationLoopMode.Clamp => "Clamp: Animation plays once and holds at the final value",
            AnimationLoopMode.Random => "Random: Animation selects random keyframes",
            _ => "Unknown loop mode"
        };
    }
}
