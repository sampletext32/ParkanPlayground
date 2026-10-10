using System.Numerics;
using FxidLib;
using ImGuiNET;
using MaterialLib;
using NResUI.Abstractions;
using NResUI.EffectPlayer;
using Silk.NET.Windowing;

namespace NResUI.ImGuiUI;

/// <summary>
/// Просмотр орудий: выбирается CTL орудия из guns.rlb, и один выстрел проигрывается целиком: вспышка и звук у дула (эффекты CTL
/// орудия), снаряд с эффектами полёта (шлейф, свет, факел - эффекты CTL снаряда) и взрыв при попадании (EXPL снаряда, слот по
/// материалу поверхности). Цепочка орудие -> BULL -> CTL снаряда -> EXPL собирается в <see cref="WeaponCatalog"/>.
/// </summary>
public class WeaponViewerPanel : IImGuiPanel
{
    private enum Role
    {
        Muzzle,
        Bullet,
        Impact,
    }

    private sealed class Instance
    {
        public required string Name { get; init; }
        public required Role Role { get; init; }
        public required PreparedEffect Effect { get; init; }

        /// <summary>Размещение по CPT (система координат объекта) или null.</summary>
        public EffectPlacement? Placement { get; init; }

        /// <summary>Эффект снаряда запускается START_EFFECT при попадании (лазерный шлейф), иначе выключается.</summary>
        public bool StartsAtEnd { get; init; }

        /// <summary>Дополнительный масштаб (ScaleMultiplier EXPL для взрыва).</summary>
        public float ExtraScale { get; init; } = 1f;

        public EffectSoundPlayer Sound { get; } = new();
        public float PrevStrength = -1f;
    }

    private readonly WeaponCatalog _catalog;
    private readonly EffectAssetCache _assets;
    private readonly EffectCanvasRenderer _canvas;
    private readonly IWindow _window;

    private readonly List<Instance> _instances = [];
    private readonly List<DrawItem> _temp = [];
    private readonly List<DrawItem> _scene = [];
    private readonly List<string> _loadErrors = [];

    private IReadOnlyList<string> _names = [];
    private string? _listError;
    private bool _namesLoaded;
    private string _filter = string.Empty;
    private string? _selected;
    private WeaponInfo? _weapon;
    private int _builtSurface = -2;

    private float _time;
    private bool _playing = true;
    private bool _loop = true;
    private bool _mute;
    private float _volume = 0.8f;
    private float _speedFactor = 1f;
    private float _distance = 100f;
    private float _zoom = 1f;
    private float _impactScale = 1f;
    private int _focus;
    private float _bulletX;
    private float _end;
    private float _shotDelay;
    private int _surface = -1;
    private float _yaw;
    private float _pitch;
    private bool _showGrid = true;
    private bool _showLights = true;
    private int _blendMode;

    public WeaponViewerPanel(WeaponCatalog catalog, EffectAssetCache assets, EffectCanvasRenderer canvas, IWindow window)
    {
        _catalog = catalog;
        _assets = assets;
        _canvas = canvas;
        _window = window;
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Weapon Viewer"))
        {
            ImGui.End();
            return;
        }

        if (!_namesLoaded)
        {
            _namesLoaded = true;
            _names = _catalog.GetWeaponNames(out _listError);
        }

        ImGui.Text($"Папка игры: {_assets.GameBasePath}");
        ImGui.SameLine();
        if (ImGui.Button("Перечитать"))
        {
            _assets.Reset();
            _catalog.Reset();
            _namesLoaded = false;
            _selected = null;
            _weapon = null;
            _instances.Clear();
        }

        if (_listError != null)
            ImGui.TextColored(new Vector4(1, 0.4f, 0.3f, 1), _listError);

        ImGui.Separator();

        if (ImGui.BeginChild("wp_list", new Vector2(220, 0), ImGuiChildFlags.Border, ImGuiWindowFlags.None))
            DrawList();
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("wp_view", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.None))
            DrawView();
        ImGui.EndChild();

        _window.IsEventDriven = !(_playing && _weapon != null);
        ImGui.End();
    }

    private void DrawList()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##wp_filter", "фильтр", ref _filter, 64);
        foreach (var name in _names)
        {
            if (_filter.Length > 0 && !name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (ImGui.Selectable(name, name == _selected))
                Select(name);
        }
    }

    private void Select(string name)
    {
        _selected = name;
        _weapon = _catalog.GetWeapon(name);
        _builtSurface = -2;
        Restart();
    }

    private void Restart()
    {
        _time = 0f;
        foreach (var inst in _instances)
        {
            inst.Sound.StopAll();
            inst.PrevStrength = -1f;
        }
    }

    private void Rebuild()
    {
        foreach (var inst in _instances)
            inst.Sound.StopAll();

        _instances.Clear();
        _loadErrors.Clear();
        _builtSurface = _surface;
        if (_weapon == null)
            return;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fx in _weapon.MuzzleEffects)
            Add(fx.Effect, Role.Muzzle, seen, fx.Placement);

        if (_weapon.Bullet is { } bullet)
        {
            foreach (var fx in bullet.FlightEffects)
                Add(fx.Effect, Role.Bullet, seen, fx.Placement, 1f, bullet.StartOnEndLocalIds.Contains(fx.LocalId));

            if (bullet.Expl != null)
            {
                var slot = _surface >= 0 ? bullet.Expl.GetEffectForMaterial(_surface) : bullet.Expl.Slots[0];
                if (slot is { } s)
                {
                    var scale = bullet.Expl.ScaleMultiplier > 0.0001f ? bullet.Expl.ScaleMultiplier : 1f;
                    Add(s.ItemName, Role.Impact, seen, null, scale);
                }
            }
        }
    }

    private void Add(
        string effectName, Role role, HashSet<string> seen, EffectPlacement? placement, float extraScale = 1f,
        bool startsAtEnd = false)
    {
        if (!seen.Add(role + ":" + effectName))
            return;

        var file = _assets.LoadFxid(effectName, out var error);
        if (file == null)
        {
            _loadErrors.Add($"{effectName}: {error}");
            return;
        }

        _instances.Add(new Instance
        {
            Name = effectName,
            Role = role,
            Effect = PreparedEffect.From(effectName, file),
            Placement = placement,
            ExtraScale = extraScale,
            StartsAtEnd = startsAtEnd,
        });
    }

    private void DrawView()
    {
        if (_weapon == null)
        {
            ImGui.TextDisabled("Выберите орудие слева.");
            return;
        }

        if (_builtSurface != _surface)
            Rebuild();

        DrawInfo(_weapon);
        DrawControls();

        var speed = _weapon.Bullet?.Speed ?? 0f;
        // Автомат выстрела (AdvanceShotStateMachine): канал анимации идёт 0 -> 0.5 за d = 0.5 / скорость; в этот момент снаряд
        // создаётся, затем пауза CP2, затем 0.5 -> 1.0 ещё за d, после чего значение сбрасывается в 0.
        _shotDelay = speed > 0.01f || _weapon.Bullet != null ? 0.5f / MathF.Max(_weapon.ShotAnimSpeed, 0.01f) : 0f;
        var flight = speed > 0.01f ? _shotDelay + _distance / speed : 0f;
        var impactTail = _instances.Where(i => i.Role == Role.Impact).Select(i => Duration(i.Effect)).DefaultIfEmpty(0f).Max();
        var muzzleTail = _instances.Where(i => i.Role == Role.Muzzle).Select(i => Duration(i.Effect)).DefaultIfEmpty(0f).Max();
        var end = MathF.Max(flight + impactTail, muzzleTail) + 0.3f;

        if (_playing)
        {
            _time += ImGui.GetIO().DeltaTime * _speedFactor;
            if (_time > end)
            {
                if (_loop)
                    Restart();
                else
                    _playing = false;
            }
        }

        _end = end;
        _time = Math.Clamp(_time, 0f, end);
        DrawTimeline(end, flight);
        Evaluate(flight, speed);

        ImGui.Text($"t = {_time:F3} с / {end:F2} с   полёт до цели: {flight:F3} с   звуков: {_instances.Sum(i => i.Sound.ActiveVoices)}");
        foreach (var error in _loadErrors)
            ImGui.TextColored(new Vector4(1, 0.5f, 0.3f, 1), error);

        var size = ImGui.GetContentRegionAvail();
        size.Y = MathF.Max(160f, size.Y - 4f);
        DrawCanvas(size);
    }

    /// <summary>
    /// Перемотка: ползунок времени с отметкой момента попадания и шаг по кадрам (1/60 с). Любое ручное изменение времени
    /// ставит воспроизведение на паузу (как у проигрывателя эффектов).
    /// </summary>
    private void DrawTimeline(float end, float flight)
    {
        const float frame = 1f / 60f;
        var changed = false;
        var t = _time;

        if (ImGui.Button("|<"))
        {
            t = 0f;
            changed = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("<"))
        {
            t = MathF.Max(0f, _time - frame);
            changed = true;
        }

        ImGui.SameLine();
        if (ImGui.Button(">"))
        {
            t = MathF.Min(end, _time + frame);
            changed = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("Попадание"))
        {
            t = Math.Clamp(flight, 0f, end);
            changed = true;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##wp_time", ref t, 0f, end, $"{_time:F3} с (попадание на {flight:F3} с)"))
            changed = true;

        if (!changed)
            return;

        _time = Math.Clamp(t, 0f, end);
        _playing = false;
        foreach (var inst in _instances)
            inst.Sound.StopAll();
    }

    /// <summary>Система координат объекта (X вправо, Y вперёд, Z вверх) в систему вида (X вперёд, Y вверх, Z вправо).</summary>
    private static Vector3 ToView(Vector3 v) => new(v.Y, v.Z, v.X);

    /// <summary>
    /// Значение для источника силы Constant: 0, если какое-нибудь окно активности содержит 0 (лазерный шлейф), иначе 1
    /// (иначе у эффектов с окнами 0.01..1 вообще ничего не рисовалось бы, а в игре рисуется).
    /// </summary>
    private static float ConstantStrength(PreparedEffect effect) =>
        effect.Components.Any(c => c.HasWindow && c.WindowStart <= 0f && c.WindowEnd >= 0f) ? 0f : 1f;

    /// <summary>
    /// Значение канала анимации орудия во время выстрела: 0 -> 0.5 за d, удержание CP2 мс, 0.5 -> 1.0 за d, затем 0
    /// (<c>AdvanceShotStateMachine</c>: такты 3, 2, 1, 0). Определяет длительность вспышек с источником силы AnimParam.
    /// </summary>
    private float ShotAnimValue(float t)
    {
        var d = _shotDelay > 0f ? _shotDelay : 0.125f;
        var pause = (_weapon?.ShotPauseMs ?? 0f) * 0.001f;
        if (t < 0f)
            return 0f;

        if (t < d)
            return 0.5f * t / d;

        if (t < d + pause)
            return 0.5f;

        if (t < 2f * d + pause)
            return 0.5f + 0.5f * (t - d - pause) / d;

        return 0f;
    }

    private static float Duration(PreparedEffect effect) => effect.Header.Duration > 0.001f ? effect.Header.Duration : 1f;

    private void DrawInfo(WeaponInfo w)
    {
        ImGui.Text($"{w.Name}");
        if (w.Bullet is { } b)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(
                $"снаряд {b.Name}  скорость {b.Speed:F0} ед./с  пауза {w.FireDelayMs:F0} мс  боезапас {w.Ammo:F0}" +
                (b.ExplName != null ? $"  взрыв {b.ExplName}" : string.Empty));
        }
        else if (w.BulletName != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled($"снаряд {w.BulletName} не найден");
        }

        if (ImGui.TreeNode("Эффекты (откуда берутся)"))
        {
            foreach (var fx in w.MuzzleEffects)
                ImGui.BulletText($"орудие, список {fx.HookList}, команда {fx.Opcode}, local_id {fx.LocalId}: {fx.Effect}");

            if (w.Bullet is { } bullet)
            {
                foreach (var fx in bullet.FlightEffects)
                    ImGui.BulletText($"снаряд, список {fx.HookList}, команда {fx.Opcode}, local_id {fx.LocalId}: {fx.Effect}");

                if (bullet.Expl != null)
                {
                    for (var i = 0; i < bullet.Expl.Slots.Count; i++)
                    {
                        if (bullet.Expl.Slots[i] is { } slot)
                            ImGui.BulletText($"взрыв, слот {i} ({(i == 0 ? "по умолчанию" : SurfaceName(i - 1))}): {slot.ItemName}");
                    }
                }
            }

            ImGui.TreePop();
        }
    }

    private static string SurfaceName(int id) => Enum.IsDefined(typeof(SurfaceMaterial), (byte)id)
        ? ((SurfaceMaterial)(byte)id).ToString()
        : id.ToString();

    private void DrawControls()
    {
        if (ImGui.Button(_playing ? "Пауза" : "Играть"))
        {
            if (!_playing && _time >= _end - 0.0001f)
                Restart();

            _playing = !_playing;
        }

        ImGui.SameLine();
        if (ImGui.Button("Выстрел"))
        {
            Restart();
            _playing = true;
        }

        ImGui.SameLine();
        ImGui.Checkbox("Повтор", ref _loop);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Скорость", ref _speedFactor, 0.05f, 2f, "%.2fx");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(140);
        ImGui.SliderFloat("Дистанция", ref _distance, 5f, 400f, "%.0f ед.");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Масштаб", ref _zoom, 0.25f, 40f, "%.2fx", ImGuiSliderFlags.Logarithmic);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Взрыв", ref _impactScale, 0.01f, 2f, "%.2fx", ImGuiSliderFlags.Logarithmic);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var axial = EffectSimulator.AxialLengthScale;
        if (ImGui.SliderFloat("Длина полос", ref axial, 0.01f, 1f, "%.2fx", ImGuiSliderFlags.Logarithmic))
            EffectSimulator.AxialLengthScale = axial;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(130);
        ImGui.Combo("Камера", ref _focus, ["Вся трасса", "Дуло", "Снаряд", "Цель"], 4);

        var items = new[] { "по умолчанию (слот 0)" }
            .Concat(Enumerable.Range(0, 11).Select(i => $"{i}: {SurfaceName(i)}")).ToArray();
        var index = _surface + 1;
        ImGui.SetNextItemWidth(170);
        if (ImGui.Combo("Поверхность", ref index, items, items.Length))
            _surface = index - 1;

        ImGui.SameLine();
        if (ImGui.Button("Сбоку"))
        {
            _yaw = 0f;
            _pitch = 0f;
        }

        ImGui.SameLine();
        if (ImGui.Button("Сверху"))
        {
            _yaw = 0f;
            _pitch = 90f;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Поворот", ref _yaw, -180f, 180f, "%.0f°");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("Наклон", ref _pitch, -90f, 90f, "%.0f°");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150);
        ImGui.Combo("Смешивание", ref _blendMode, ["По материалу", "Всегда alpha", "Всегда аддитивное"], 3);
        ImGui.SameLine();
        ImGui.Checkbox("Сетка", ref _showGrid);
        ImGui.SameLine();
        ImGui.Checkbox("Свет", ref _showLights);
        ImGui.SameLine();
        ImGui.Checkbox("Без звука", ref _mute);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(90);
        ImGui.SliderFloat("Громкость", ref _volume, 0f, 1f, "%.2f");
    }

    /// <summary>Считает все эффекты выстрела на момент <c>_time</c> и собирает команды рисования в <c>_scene</c>.</summary>
    private void Evaluate(float flight, float speed)
    {
        _scene.Clear();
        var view = EffectView.FromAngles(_yaw, _pitch);
        var hit = _time >= flight;
        var bulletX = speed > 0f ? MathF.Min(speed * MathF.Max(_time - _shotDelay, 0f), _distance) : 0f;
        _bulletX = bulletX;

        foreach (var inst in _instances)
        {
            float local;
            Vector3 origin;
            var attachSpeed = 0f;
            var maxTrail = float.MaxValue;
            var removed = false;
            var ended = flight > 0f && _time >= flight;
            float? forcedStrength = null;

            switch (inst.Role)
            {
                case Role.Muzzle:
                    local = _time;
                    origin = Vector3.Zero;
                    if (inst.Effect.Header.StrengthSource is EffectStrengthSource.AnimParam or
                        EffectStrengthSource.AnimMinClamp or EffectStrengthSource.AnimMaxClamp)
                    {
                        // Источник «анимация детали»: значение канала привязки орудия по автомату выстрела.
                        forcedStrength = ShotAnimValue(_time);
                    }

                    break;
                case Role.Bullet:
                    local = _time - _shotDelay;
                    origin = new Vector3(bulletX, 0f, 0f);
                    attachSpeed = speed;
                    maxTrail = _distance;
                    if (local < 0f)
                    {
                        // снаряд ещё не создан (первый такт автомата выстрела)
                        removed = true;
                    }
                    else if (ended)
                    {
                        // Состояния конца полёта: лазерный шлейф запускается заново с источником Life (START_EFFECT)
                        // и затухает на месте, остальные эффекты выключаются (DEACTIVATE_EFFECT).
                        removed = !inst.StartsAtEnd;
                        local = _time - flight;
                        attachSpeed = 1e9f;
                        var d = Duration(inst.Effect);
                        forcedStrength = Math.Clamp(local / d, 0f, 1f);
                    }
                    else if (inst.Effect.Header.StrengthSource == EffectStrengthSource.Constant)
                    {
                        // Постоянная сила: значение задаётся SetConstantStrength, по умолчанию 0 - как у las_tail (окно 0..1).
                        forcedStrength = ConstantStrength(inst.Effect);
                    }

                    break;
                default:
                    local = _time - flight;
                    origin = new Vector3(_distance, 0f, 0f);
                    removed = local < 0f;
                    break;
            }

            if (removed)
            {
                inst.Sound.StopLooped();
                inst.PrevStrength = -1f;
                continue;
            }

            var duration = Duration(inst.Effect);
            var life = Math.Clamp(local / duration, 0f, 1f);

            // Источники силы по скорости: снаряд летит с максимальной скоростью вдоль Y объекта (вид: X).
            forcedStrength ??= inst.Effect.Header.StrengthSource switch
            {
                EffectStrengthSource.TangSpeedMag or EffectStrengthSource.TangSpeedY => 1f,
                EffectStrengthSource.TangSpeedX or EffectStrengthSource.TangSpeedZ => 0f,
                _ => null,
            };

            var strength = forcedStrength
                           ?? FxidStrength.FromTimeSource(inst.Effect.Header.StrengthSource, local * 1000f, 0f, duration * 1000f)
                           ?? life;
            strength = FxidStrength.ApplyFlags(inst.Effect.Header.Flags, strength, life);

            // Размещение по CPT: оси и масштаб эффекта (X вправо, Y вперёд, Z вверх -> вид: X вперёд, Y вверх, Z вправо).
            EffectFrame? frame = null;
            var placementOffset = Vector3.Zero;
            if (inst.Placement is { } pl)
            {
                var ex = ToView(pl.Ex);
                var ey = ToView(pl.Ey);
                var ez = ToView(pl.Ez);
                var scale = (ex.Length() + ey.Length() + ez.Length()) / 3f;
                if (scale > 1e-6f)
                {
                    frame = new EffectFrame(
                        ex / MathF.Max(ex.Length(), 1e-6f), ey / MathF.Max(ey.Length(), 1e-6f),
                        ez / MathF.Max(ez.Length(), 1e-6f), scale * inst.ExtraScale);
                    placementOffset = ToView(pl.Position);
                    origin += placementOffset;
                }
            }
            else if (inst.Role == Role.Impact)
            {
                // Взрыв ориентирован по нормали поверхности (EXPL режим 7): +X эффекта смотрит назад, к стрелку.
                frame = new EffectFrame(
                    new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), inst.ExtraScale * _impactScale);
            }

            // DetachAfterEmit (флаг 0x4): после первого обновления эффект отцепляется от детали и остаётся там, где был создан
            // (облако дыма у дула ракеты).
            var detach = (inst.Effect.Header.Flags & FxidEffectFlags.DetachAfterEmit) != 0;
            Func<float, Vector3>? originAt = null;
            if (inst.Role == Role.Bullet)
            {
                if (detach)
                {
                    origin = placementOffset;
                    originAt = _ => placementOffset;
                }
                else
                {
                    var maxX = _distance;
                    var v = speed;
                    originAt = t => new Vector3(MathF.Min(MathF.Max(v * t, 0f), maxX), 0f, 0f) + placementOffset;
                }
            }

            EffectSimulator.Evaluate(inst.Effect, strength, local, duration, view, _temp, attachSpeed, origin, maxTrail,
                frame: frame, originAt: originAt);
            _scene.AddRange(_temp);

            if (_playing && !_mute)
                inst.Sound.Update(inst.Effect, _assets, inst.PrevStrength, strength, _volume);
            else
                inst.Sound.StopLooped();

            inst.PrevStrength = strength;
        }
    }

    private void DrawCanvas(Vector2 size)
    {
        ImGui.InvisibleButton("wp_canvas", size);
        var dl = ImGui.GetWindowDrawList();
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var local = size * 0.5f;

        // Камера: вся трасса от дула до цели (не меньше 40 ед. по ширине) или окно шириной ~10 ед. вокруг дула, снаряда
        // или цели (для рассмотрения вспышек и взрывов); дополнительно ползунок «Масштаб».
        float ppu;
        float shiftX;
        if (_focus == 0)
        {
            ppu = MathF.Max(0.5f, size.X * 0.82f / MathF.Max(_distance, 40f) * _zoom);
            shiftX = _distance * 0.5f;
        }
        else
        {
            ppu = MathF.Max(0.5f, size.X * 0.8f / 10f * _zoom);
            shiftX = _focus == 1 ? 0f : _focus == 2 ? _bulletX : _distance;
        }

        var background = new Vector4(0.094f, 0.102f, 0.141f, 1f);

        _canvas.Begin();
        Vector2 ToCanvas(float x, float y) => new(local.X + (x - shiftX) * ppu, local.Y - y * ppu);

        if (_showGrid)
        {
            var grid = new Vector4(0.17f, 0.18f, 0.25f, 1f);
            var axis = new Vector4(0.33f, 0.35f, 0.48f, 1f);
            var step = _distance > 150f ? 50f : _distance > 40f ? 10f : 5f;
            for (var x = 0f; x <= _distance + 0.01f; x += step)
                _canvas.AddLine(ToCanvas(x, -1000f), ToCanvas(x, 1000f), grid);

            _canvas.AddLine(ToCanvas(-1000f, 0f), ToCanvas(1000f, 0f), axis);
            _canvas.AddLine(ToCanvas(0f, -1000f), ToCanvas(0f, 1000f), axis);
            _canvas.AddLine(ToCanvas(_distance, -1000f), ToCanvas(_distance, 1000f), axis);
        }

        var overlay = new List<(DrawItem Item, Vector2 Pos)>();
        foreach (var item in _scene)
        {
            var pos = ToCanvas(item.Position.X, item.Position.Y);
            switch (item.Kind)
            {
                case DrawKind.Sprite:
                    if (!EffectCanvasDraw.Sprite(_canvas, _assets, item, pos, ppu, 1f, _blendMode, _time * 1000f))
                        overlay.Add((item, pos));
                    break;
                case DrawKind.Light:
                    if (_showLights)
                        EffectCanvasDraw.Light(_canvas, _assets, item, pos, MathF.Max(ppu, 40f));
                    break;
                default:
                    overlay.Add((item, pos));
                    break;
            }
        }

        _canvas.Render((int)size.X, (int)size.Y, background);
        dl.AddImage(_canvas.TextureId, min, max);

        dl.PushClipRect(min, max, true);
        foreach (var (item, pos) in overlay)
        {
            var screen = min + pos;
            dl.AddCircle(screen, 5f, item.Kind == DrawKind.Marker ? 0xFF40C0FFu : 0xFF4040FFu);
            var label = item.Kind == DrawKind.Marker ? item.Label : item.Material;
            if (!string.IsNullOrEmpty(label))
                dl.AddText(screen + new Vector2(8, -8), 0xFF40C0FFu, label);
        }

        // Подписи: дуло и цель.
        dl.AddText(min + ToCanvas(0f, 0f) + new Vector2(4, 4), 0xFF909090u, "дуло");
        dl.AddText(min + ToCanvas(_distance, 0f) + new Vector2(4, 4), 0xFF909090u, $"цель ({_distance:F0} ед.)");
        dl.PopClipRect();
        dl.AddRect(min, max, 0xFF808080u);
    }
}
