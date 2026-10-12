using System.Numerics;
using FxidLib;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.EffectPlayer;
using NResUI.Models;
using Silk.NET.Windowing;

namespace NResUI.ImGuiUI;

/// <summary>
/// Проигрыватель эффектов FXID: выбор эффекта из effects.rlb, таймлайн окон активности компонентов и приближённый предпросмотр
/// (спрайты с анимацией материалов, частицы, свет). Файлы берутся из папки игры (<c>GameBasePath</c> в config.json).
/// </summary>
public class EffectPlayerPanel : IImGuiPanel
{
    private static readonly string[] StrengthModeNames =
    [
        "Life (как StartEffect(handle, 1))",
        "Из заголовка FXID",
        "Вручную",
    ];

    private static readonly string[] BlendModeNames =
    [
        "По материалу",
        "Всегда alpha",
        "Всегда аддитивное",
    ];

    private readonly EffectPlayerViewModel _vm;
    private readonly EffectAssetCache _assets;
    private readonly EffectCanvasRenderer _canvas;
    private readonly IWindow _window;
    private readonly List<DrawItem> _items = [];
    private readonly EffectSoundPlayer _sound = new();

    public EffectPlayerPanel(
        EffectPlayerViewModel vm, EffectAssetCache assets, EffectCanvasRenderer canvas, IWindow window)
    {
        _vm = vm;
        _assets = assets;
        _canvas = canvas;
        _window = window;
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Effect Player"))
        {
            ImGui.End();
            return;
        }

        if (!_vm.NamesLoaded)
            ReloadNames();

        DrawToolbar();
        ImGui.Separator();

        if (ImGui.BeginChild("fx_list", new Vector2(260, 0), ImGuiChildFlags.Border, ImGuiWindowFlags.None))
            DrawEffectList();
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("fx_view", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None))
            DrawPlayer();
        ImGui.EndChild();

        // Окно работает по событиям, поэтому во время воспроизведения принудительно просим непрерывную отрисовку.
        _window.IsEventDriven = !_vm.Playing;

        ImGui.End();
    }

    /// <summary>Элемент в ячейке таблицы на всю ширину ячейки (подпись справа учитывается).</summary>
    private static void Cell(Action draw, string label)
    {
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-ImGui.CalcTextSize(label).X - 12f);
        draw();
    }

    private void ReloadNames()
    {
        _vm.NamesLoaded = true;
        _vm.EffectNames = _assets.GetEffectNames(out var error);
        _vm.ListError = error;
    }

    private void DrawToolbar()
    {
        ImGui.Text($"Папка игры: {_assets.GameBasePath}");
        ImGui.SameLine();
        if (ImGui.Button("Перечитать"))
        {
            _assets.Reset();
            _vm.NamesLoaded = false;
            _vm.Effect = null;
            _vm.SelectedName = null;
        }

        if (_vm.ListError != null)
            ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), _vm.ListError);
        else
            ImGui.TextDisabled("effects.rlb, Material.lib и Textures.lib должны лежать в папке игры (config.json: GameBasePath).");
    }

    private void DrawEffectList()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##fx_filter", ref _vm.Filter, 64);
        ImGui.Text($"FXID: {_vm.EffectNames.Count}");

        var filter = _vm.Filter.Trim();
        foreach (var name in _vm.EffectNames)
        {
            if (filter.Length > 0 && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (ImGui.Selectable(name, name == _vm.SelectedName))
                Select(name);
        }
    }

    private void Select(string name)
    {
        _vm.SelectedName = name;
        var file = _assets.LoadFxid(name, out var error);
        _vm.Error = error;
        _vm.Effect = file == null ? null : PreparedEffect.From(name, file);
        _vm.TimeSec = 0;
        _vm.PrevStrength = -1f;
        _sound.StopAll();
        _vm.Playing = _vm.Effect != null;
    }

    private float EffectiveDuration(PreparedEffect effect) =>
        effect.Header.Duration > 0.001f ? effect.Header.Duration : 1f;

    private float ComputeStrength(PreparedEffect effect, float duration)
    {
        var life = Math.Clamp(_vm.TimeSec / duration, 0f, 1f);
        var strength = _vm.StrengthMode switch
        {
            EffectStrengthMode.Manual => _vm.ManualStrength,
            EffectStrengthMode.FromFile => FxidStrength.FromTimeSource(
                effect.Header.StrengthSource, _vm.TimeSec * 1000f, 0f, duration * 1000f) ?? life,
            _ => life,
        };

        // Флаги заголовка (MultiplyByLife 0x200, TriangularShape 0x20) движок применяет после выбора источника.
        return FxidStrength.ApplyFlags(effect.Header.Flags, strength, life);
    }

    private void DrawPlayer()
    {
        var effect = _vm.Effect;
        if (effect == null)
        {
            ImGui.Text(_vm.Error ?? "Выберите эффект в списке слева.");
            return;
        }

        var header = effect.Header;
        var duration = EffectiveDuration(effect);

        ImGui.Text($"{effect.Name}   компонентов: {header.ComponentCount}   длительность: {header.Duration:F2} с" +
                   (header.Duration > 0.001f ? string.Empty : " (в файле 0, показано 1 с)"));
        ImGui.TextDisabled($"источник силы: {header.StrengthSource}   флаги: {header.Flags}   SettingsId: 0x{header.SettingsId:X}");

        // Транспорт
        if (ImGui.Button(_vm.Playing ? "Пауза" : "Играть"))
        {
            if (!_vm.Playing && _vm.TimeSec >= duration)
            {
                _vm.TimeSec = 0;
                _vm.PrevStrength = -1f;
            }

            _vm.Playing = !_vm.Playing;
        }

        ImGui.SameLine();
        if (ImGui.Button("В начало"))
        {
            _vm.TimeSec = 0;
            _vm.PrevStrength = -1f;
            _sound.StopAll();
        }
        ImGui.SameLine();
        ImGui.Checkbox("Повтор", ref _vm.Loop);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120);
        ImGui.SliderFloat("Скорость", ref _vm.Speed, 0.05f, 2f, "%.2fx");

        if (_vm.Playing)
        {
            _vm.TimeSec += ImGui.GetIO().DeltaTime * _vm.Speed;
            if (_vm.TimeSec >= duration)
            {
                if (_vm.Loop)
                {
                    _vm.TimeSec %= duration;
                    _vm.PrevStrength = -1f;
                    _sound.StopLooped();
                }
                else
                {
                    _vm.TimeSec = duration;
                    _vm.Playing = false;
                }
            }
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##time", ref _vm.TimeSec, 0f, duration, "%.3f с");

        ImGui.SetNextItemWidth(260);
        ImGui.Combo("Сила", ref _vm.StrengthModeIndex, StrengthModeNames, StrengthModeNames.Length);
        if (_vm.StrengthMode == EffectStrengthMode.Manual)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.SliderFloat("##manual", ref _vm.ManualStrength, 0f, 1f);
        }

        // Остальные настройки вида сворачиваются, чтобы не занимать экран.
        if (ImGui.CollapsingHeader("Вид, камера и отладка") && ImGui.BeginTable("##fx_opts", 3, ImGuiTableFlags.SizingStretchSame))
        {
            Cell(() => ImGui.SliderFloat("Масштаб", ref _vm.PixelsPerUnit, 20f, 600f, "%.0f px/ед."), "Масштаб");
            Cell(() => ImGui.SliderFloat("Размер спрайтов", ref _vm.SpriteScale, 0.1f, 4f, "%.2fx"), "Размер спрайтов");
            Cell(() => ImGui.SliderFloat("Скорость шлейфа", ref _vm.AttachSpeed, 0f, 300f, "%.0f ед./с"), "Скорость шлейфа");

            Cell(() => ImGui.SliderFloat("Поворот", ref _vm.YawDeg, -180f, 180f, "%.0f°"), "Поворот");
            Cell(() => ImGui.SliderFloat("Наклон", ref _vm.PitchDeg, -90f, 90f, "%.0f°"), "Наклон");
            ImGui.TableNextColumn();
            if (ImGui.Button("Сбоку"))
                (_vm.YawDeg, _vm.PitchDeg) = (0f, 0f);
            ImGui.SameLine();
            if (ImGui.Button("Спереди"))
                (_vm.YawDeg, _vm.PitchDeg) = (90f, 0f);
            ImGui.SameLine();
            if (ImGui.Button("Сверху"))
                (_vm.YawDeg, _vm.PitchDeg) = (0f, 90f);

            Cell(() => ImGui.SliderFloat("Дист. камеры", ref _vm.CameraDistance, 1f, 1000f, "%.0f ед.", ImGuiSliderFlags.Logarithmic), "Дист. камеры");
            Cell(() => ImGui.Combo("Смешивание", ref _vm.BlendModeIndex, BlendModeNames, BlendModeNames.Length), "Смешивание");
            ImGui.TableNextColumn();
            ImGui.Checkbox("Сетка", ref _vm.ShowGrid);
            ImGui.SameLine();
            ImGui.Checkbox("Маркеры", ref _vm.ShowMarkers);
            ImGui.SameLine();
            ImGui.Checkbox("Тёмный фон", ref _vm.DarkBackground);

            ImGui.TableNextColumn();
            ImGui.Checkbox("Без звука", ref _vm.MuteSound);
            Cell(() => ImGui.SliderFloat("Громкость", ref _vm.Volume, 0f, 1f, "%.2f"), "Громкость");
            ImGui.EndTable();
        }

        var strength = ComputeStrength(effect, duration);
        EffectSimulator.Evaluate(
            effect, strength, _vm.TimeSec, duration, _vm.View, _items, _vm.AttachSpeed, cameraDistance: _vm.CameraDistance);

        if (_vm.Playing && !_vm.MuteSound)
            _sound.Update(effect, _assets, _vm.PrevStrength, strength, _vm.Volume);
        else
            _sound.StopLooped();

        _vm.PrevStrength = strength;

        ImGui.Text($"t = {_vm.TimeSec:F3} с   сила = {strength:F3}   активных компонентов: {effect.Components.Count(c => c.Active)}" +
                   $"   звуков: {_sound.ActiveVoices}");
        if (!EffectSoundPlayer.IsSupported)
            ImGui.TextDisabled("Звук поддерживается только в Windows (winmm).");
        else if (_sound.LastWarning != null)
            ImGui.TextColored(new Vector4(1, 0.6f, 0.2f, 1), _sound.LastWarning);

        var avail = ImGui.GetContentRegionAvail();
        var timelineHeight = 28f + effect.Components.Count * 20f;
        var canvasSize = new Vector2(avail.X, MathF.Max(160f, avail.Y - timelineHeight - 8f));
        DrawCanvas(canvasSize);
        DrawTimeline(effect, strength, new Vector2(avail.X, timelineHeight));

        if (_assets.Warnings.Count > 0 && ImGui.TreeNode($"Предупреждения ({_assets.Warnings.Count})"))
        {
            foreach (var warning in _assets.Warnings.Distinct())
                ImGui.TextWrapped(warning);
            ImGui.TreePop();
        }
    }

    private void DrawCanvas(Vector2 size)
    {
        ImGui.InvisibleButton("fx_canvas", size);
        var dl = ImGui.GetWindowDrawList();
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var local = size * 0.5f;
        var ppu = _vm.PixelsPerUnit;

        // Холст собирается в offscreen-текстуре (ImGui не умеет аддитивное смешивание), здесь только координаты в пикселях.
        var background = _vm.DarkBackground ? new Vector4(0.094f, 0.102f, 0.141f, 1f) : new Vector4(0.9f, 0.9f, 0.9f, 1f);
        _canvas.Begin();

        if (_vm.ShowGrid)
        {
            var gridColor = _vm.DarkBackground ? new Vector4(0.17f, 0.18f, 0.25f, 1f) : new Vector4(0.78f, 0.78f, 0.78f, 1f);
            var axisColor = _vm.DarkBackground ? new Vector4(0.33f, 0.35f, 0.48f, 1f) : new Vector4(0.56f, 0.56f, 0.56f, 1f);
            for (var x = local.X % ppu; x < size.X; x += ppu)
                _canvas.AddLine(new Vector2(x, 0), new Vector2(x, size.Y), gridColor);
            for (var y = local.Y % ppu; y < size.Y; y += ppu)
                _canvas.AddLine(new Vector2(0, y), new Vector2(size.X, y), gridColor);
            _canvas.AddLine(new Vector2(0, local.Y), new Vector2(size.X, local.Y), axisColor);
            _canvas.AddLine(new Vector2(local.X, 0), new Vector2(local.X, size.Y), axisColor);
        }

        var overlay = new List<(DrawItem Item, Vector2 Pos)>();
        foreach (var item in _items)
        {
            var pos = new Vector2(local.X + item.Position.X * ppu, local.Y - item.Position.Y * ppu);
            switch (item.Kind)
            {
                case DrawKind.Sprite:
                    if (!EffectCanvasDraw.Sprite(
                            _canvas, _assets, item, pos, ppu, _vm.SpriteScale, _vm.BlendModeIndex, _vm.TimeSec * 1000f))
                        overlay.Add((item, pos));
                    break;
                case DrawKind.Light:
                    EffectCanvasDraw.Light(_canvas, _assets, item, pos, ppu);
                    break;
                case DrawKind.Marker:
                    overlay.Add((item, pos));
                    break;
            }
        }

        _canvas.Render((int)size.X, (int)size.Y, background);
        dl.AddImage(_canvas.TextureId, min, max);

        // Маркеры и спрайты без материала рисует ImGui поверх готовой картинки.
        dl.PushClipRect(min, max, true);
        foreach (var (item, pos) in overlay)
        {
            if (!_vm.ShowMarkers)
                continue;

            var screen = min + pos;
            if (item.Kind == DrawKind.Marker)
            {
                dl.AddCircle(screen, 6f, 0xFF40C0FFu);
                dl.AddText(screen + new Vector2(8, -8), 0xFF40C0FFu, item.Label);
            }
            else
            {
                dl.AddCircle(screen, 5f, 0xFF4040FFu);
                if (!string.IsNullOrEmpty(item.Material))
                    dl.AddText(screen + new Vector2(8, -8), 0xFF4040FFu, item.Material);
            }
        }

        dl.PopClipRect();
        dl.AddRect(min, max, 0xFF808080u);
    }

    private void DrawTimeline(PreparedEffect effect, float strength, Vector2 size)
    {
        ImGui.InvisibleButton("fx_timeline", size);
        var dl = ImGui.GetWindowDrawList();
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        const float labelWidth = 230f;
        var trackMin = new Vector2(min.X + labelWidth, min.Y + 20f);
        var trackWidth = MathF.Max(10f, max.X - trackMin.X - 6f);

        dl.AddRectFilled(min, max, 0xFF20222Cu);
        dl.AddText(new Vector2(min.X + 4, min.Y + 2), 0xFFB0B0B0u, "окна активности (по силе 0..1)");
        for (var i = 0; i <= 10; i++)
        {
            var x = trackMin.X + trackWidth * i / 10f;
            dl.AddLine(new Vector2(x, min.Y + 16f), new Vector2(x, max.Y), 0xFF3A3D4Eu);
            dl.AddText(new Vector2(x + 2, min.Y + 2), 0xFF808080u, (i / 10f).ToString("0.0"));
        }

        for (var row = 0; row < effect.Components.Count; row++)
        {
            var c = effect.Components[row];
            var y = trackMin.Y + row * 20f;
            var labelColor = c.Active ? 0xFFFFFFFFu : 0xFF909090u;
            dl.AddText(new Vector2(min.X + 4, y + 2), labelColor, Truncate(c.Label, 30));

            if (!c.HasWindow)
                continue;

            var color = TypeColor(c.Type, c.Active);
            if (c.WindowEnd >= c.WindowStart)
            {
                DrawBar(dl, trackMin.X, trackWidth, y, c.WindowStart, c.WindowEnd, color);
            }
            else
            {
                DrawBar(dl, trackMin.X, trackWidth, y, c.WindowStart, 1f, color);
                DrawBar(dl, trackMin.X, trackWidth, y, 0f, c.WindowEnd, color);
            }
        }

        var px = trackMin.X + trackWidth * Math.Clamp(strength, 0f, 1f);
        dl.AddLine(new Vector2(px, min.Y + 16f), new Vector2(px, max.Y), 0xFF40FFFFu, 2f);

        // Перемотка мышью: по таймлайну сила = время / длительность.
        if (ImGui.IsItemActive() && ImGui.IsMouseDown(ImGuiMouseButton.Left) &&
            _vm.StrengthMode != EffectStrengthMode.Manual)
        {
            var t = Math.Clamp((ImGui.GetMousePos().X - trackMin.X) / trackWidth, 0f, 1f);
            _vm.TimeSec = t * EffectiveDuration(effect);
            _vm.Playing = false;
        }
        else if (ImGui.IsItemActive() && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _vm.ManualStrength = Math.Clamp((ImGui.GetMousePos().X - trackMin.X) / trackWidth, 0f, 1f);
        }
    }

    private static void DrawBar(ImDrawListPtr dl, float trackX, float trackWidth, float y, float start, float end, uint color)
    {
        var x0 = trackX + trackWidth * Math.Clamp(start, 0f, 1f);
        var x1 = trackX + trackWidth * Math.Clamp(end, 0f, 1f);
        dl.AddRectFilled(new Vector2(x0, y + 3f), new Vector2(MathF.Max(x1, x0 + 2f), y + 17f), color, 2f);
    }

    private static uint TypeColor(FxidComponentType type, bool active)
    {
        var rgb = type switch
        {
            FxidComponentType.Sound => 0x4FA8E0u,
            FxidComponentType.Light => 0xE0C040u,
            FxidComponentType.PlaneParticles or FxidComponentType.PolyhedronParticles => 0x60D070u,
            FxidComponentType.Trail or FxidComponentType.ParticleSpawner => 0xD07040u,
            _ => 0xC060C0u,
        };

        // ImGui использует ABGR: 0xAABBGGRR.
        var r = (rgb >> 16) & 0xFF;
        var g = (rgb >> 8) & 0xFF;
        var b = rgb & 0xFF;
        var alpha = active ? 0xFFu : 0x70u;
        return (alpha << 24) | (b << 16) | (g << 8) | r;
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..(length - 1)] + "…";
}
