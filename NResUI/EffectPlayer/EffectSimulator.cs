using System.Numerics;
using FxidLib;
using Vec3 = Common.Vector3;

namespace NResUI.EffectPlayer;

/// <summary>
/// Ортографический вид: оси экрана в координатах эффекта (X, Y, Z - локальные оси эффекта, Z - вверх).
/// Экран: U = p·Right, V = p·Up (V вверх).
/// </summary>
public readonly record struct EffectView(Vector3 Right, Vector3 Up)
{
    /// <summary>Вид сбоку: X вправо, Z вверх.</summary>
    public static EffectView Side => FromAngles(0f, 0f);

    /// <summary>Вид спереди: Y вправо, Z вверх.</summary>
    public static EffectView Front => FromAngles(90f, 0f);

    /// <summary>Вид сверху: X вправо, Y вверх.</summary>
    public static EffectView Top => FromAngles(0f, 90f);

    /// <summary>
    /// Поворот вокруг вертикальной оси Z (yaw, 0 = смотрим вдоль -Y, то есть X вправо) и наклон камеры вверх (pitch, 90 = сверху).
    /// </summary>
    public static EffectView FromAngles(float yawDeg, float pitchDeg)
    {
        var yaw = yawDeg * MathF.PI / 180f;
        var pitch = pitchDeg * MathF.PI / 180f;
        var (sy, cy) = MathF.SinCos(yaw);
        var (sp, cp) = MathF.SinCos(pitch);
        return new EffectView(
            new Vector3(cy, sy, 0f),
            new Vector3(-sy * sp, cy * sp, cp));
    }
}

public enum DrawKind
{
    Sprite,
    Light,
    Marker,
}

/// <summary>
/// Одна команда отрисовки, полученная от симулятора. Позиция и размеры в единицах эффекта.
/// <see cref="Size"/> - сторона квадратного спрайта (объекты) или размер света; у частиц дополнительно заданы
/// <see cref="Width"/> и <see cref="Height"/> (вытянутые «штрихи»), тогда они имеют приоритет над <see cref="Size"/>.
/// <see cref="Direction"/> - единичный вектор в плоскости проекции (Y вверх), вдоль которого вытянута ширина спрайта;
/// нулевой вектор означает «без поворота» (спрайт вдоль оси экрана).
/// <see cref="HalfU"/> и <see cref="HalfV"/> - если заданы, спрайт - параллелограмм со сторонами 2·HalfU и 2·HalfV вокруг
/// позиции (проекция 3D плоскости в плоскость вида, Y вверх). <see cref="Phase"/> - фаза 0..1 для выбора кадра материала
/// (меньше нуля - выбирать по времени).
/// </summary>
public readonly record struct DrawItem(
    DrawKind Kind,
    int ComponentIndex,
    Vector2 Position,
    float Size,
    float Alpha,
    Vector3 Color,
    string? Material,
    string Label,
    float Width = 0f,
    float Height = 0f,
    Vector2 Direction = default,
    Vector2 HalfU = default,
    Vector2 HalfV = default,
    float Phase = -1f);

/// <summary>Разобранный компонент FXID вместе с текущим состоянием окна активности.</summary>
public sealed class PreparedComponent
{
    public required int Index { get; init; }
    public required FxidComponentType Type { get; init; }
    public required string Label { get; init; }
    public string? ResourceName { get; init; }

    public EffectType1Data? Light { get; init; }
    public EffectSoundData? Sound { get; init; }
    public EffectType3Data? Object { get; init; }
    public EffectParticleData? Particle { get; init; }
    public EffectTrailData? Trail { get; init; }
    public EffectSpawnerData? Spawner { get; init; }

    /// <summary>
    /// Опорное расстояние компонента типа 4 (+0xC8): прогресс анимации умножается на расстояние до камеры / это значение
    /// (<c>EffectType4::RenderContents</c>); 0 - обычный компонент.
    /// </summary>
    public float ReferenceDistance { get; init; }

    /// <summary>Окно активности на значении силы 0..1; HasWindow = false у пустых компонентов.</summary>
    public bool HasWindow { get; init; }

    public float WindowStart { get; init; }
    public float WindowEnd { get; init; }

    /// <summary>Заполняется <see cref="EffectSimulator.Evaluate"/>.</summary>
    public bool Active { get; set; }

    /// <summary>Нормированное положение внутри окна 0..1 (когда <see cref="Active"/>).</summary>
    public float Life { get; set; }
}

/// <summary>FXID, разобранный в типизированные компоненты. Создаётся один раз при выборе эффекта.</summary>
public sealed class PreparedEffect
{
    public required string Name { get; init; }
    public required EffectHeader Header { get; init; }
    public required IReadOnlyList<PreparedComponent> Components { get; init; }

    public static PreparedEffect From(string name, FxidFile file)
    {
        var list = new List<PreparedComponent>();
        foreach (var c in file.Components)
        {
            var res = c.Resource?.ResourceName;
            var label = $"{c.Index}: {TypeName(c.Type)}" + (string.IsNullOrEmpty(res) ? string.Empty : $" {res}");
            switch (c.Type)
            {
                case FxidComponentType.Billboard:
                {
                    var d = c.AsType1();
                    list.Add(Make(c, label, res, light: d, start: d.ActiveTimeStart, end: d.ActiveTimeEnd));
                    break;
                }
                case FxidComponentType.Sound:
                {
                    var d = c.AsSound();
                    list.Add(Make(c, label, res, sound: d, start: d.WindowStart, end: d.WindowEnd));
                    break;
                }
                case FxidComponentType.AnimParticle:
                {
                    var d = c.AsType3();
                    list.Add(Make(c, label, res, obj: d, start: d.ActiveTimeStart, end: d.ActiveTimeEnd));
                    break;
                }
                case FxidComponentType.AnimBillboard:
                {
                    var t4 = c.AsType4();
                    var d = t4.Base;
                    list.Add(Make(
                        c, label, res, obj: d, start: d.ActiveTimeStart, end: d.ActiveTimeEnd,
                        referenceDistance: t4.ReferenceDistance));
                    break;
                }
                case FxidComponentType.AnimModel:
                {
                    var d = c.AsType9().Base;
                    list.Add(Make(c, label, res, obj: d, start: d.ActiveTimeStart, end: d.ActiveTimeEnd));
                    break;
                }
                case FxidComponentType.Plane:
                case FxidComponentType.Cube:
                {
                    var d = c.AsParticle();
                    list.Add(Make(c, label, res, particle: d, start: d.ActiveWindowStart, end: d.ActiveWindowEnd));
                    break;
                }
                case FxidComponentType.Trail:
                {
                    var d = c.AsTrail();
                    list.Add(Make(c, label, res, trail: d, start: d.ActiveWindowStart, end: d.ActiveWindowEnd));
                    break;
                }
                case FxidComponentType.Model:
                {
                    var d = c.AsSpawner();
                    list.Add(Make(c, label, res, spawner: d, start: d.ActiveWindowStart, end: d.ActiveWindowEnd));
                    break;
                }
                default:
                    list.Add(New(c, label, res));
                    break;
            }
        }

        return new PreparedEffect { Name = name, Header = file.Header, Components = list };
    }

    private static PreparedComponent New(FxidComponent c, string label, string? res) => new()
    {
        Index = c.Index,
        Type = c.Type,
        Label = label,
        ResourceName = res,
    };

    private static PreparedComponent Make(
        FxidComponent c,
        string label,
        string? res,
        float start,
        float end,
        EffectType1Data? light = null,
        EffectSoundData? sound = null,
        EffectType3Data? obj = null,
        EffectParticleData? particle = null,
        EffectTrailData? trail = null,
        EffectSpawnerData? spawner = null,
        float referenceDistance = 0f) => new()
    {
        ReferenceDistance = referenceDistance,
        Index = c.Index,
        Type = c.Type,
        Label = label,
        ResourceName = res,
        Light = light,
        Sound = sound,
        Object = obj,
        Particle = particle,
        Trail = trail,
        Spawner = spawner,
        HasWindow = true,
        WindowStart = start,
        WindowEnd = end,
    };

    public static string TypeName(FxidComponentType type) => type switch
    {
        FxidComponentType.Billboard => "свет",
        FxidComponentType.Sound => "звук",
        FxidComponentType.AnimParticle => "объект",
        FxidComponentType.AnimBillboard => "объект+дист",
        FxidComponentType.Trail => "шлейф",
        FxidComponentType.Point => "точка",
        FxidComponentType.Plane => "частицы (плоск.)",
        FxidComponentType.Model => "спавнер",
        FxidComponentType.AnimModel => "объект (9)",
        FxidComponentType.Cube => "частицы (3D)",
        _ => type.ToString(),
    };
}

/// <summary>
/// Приближённая симуляция компонентов FXID в одной плоскости проекции. Окна активности, интерполяции позиции/масштаба/цвета
/// и жизнь частиц следуют разобранному формату (см. FxidLib/README.md); траектории частиц, спавнер, шлейф и 3D ориентация
/// объектов упрощены. Звук только отмечается в таймлайне.
/// </summary>
/// <summary>
/// Положение локальной системы эффекта в сцене: оси эффекта X, Y, Z в координатах вида (единичные) и общий масштаб.
/// Движок строит её для эффектов, созданных командами CTL (SPAWN_* из трёх CPT): колонки матрицы - векторы направлений
/// CPT, поэтому длина этих векторов (0.02..1) масштабирует весь эффект, а их направление задаёт оси (у пуль +X эффекта
/// смотрит назад).
/// </summary>
public readonly record struct EffectFrame(Vector3 Ex, Vector3 Ey, Vector3 Ez, float Scale);

internal static class ViewExtensions
{
    public static Vec3 AsVec(this Vector3 v) => new(v.X, v.Y, v.Z);
}

public static class EffectSimulator
{

    private const float ParticleGravity = 9.8f;
    private const int MaxParticlesPerComponent = 256;

    public static bool InWindow(float t, float start, float end) =>
        end >= start ? t >= start && t <= end : t >= start || t <= end;

    /// <summary>Нормированное положение t внутри окна (учитывает «заворачивающиеся» окна, где конец меньше начала).</summary>
    public static float WindowLife(float t, float start, float end)
    {
        if (end >= start)
            return end > start ? (t - start) / (end - start) : 0f;

        var length = end - start + 1f;
        var pos = t >= start ? t - start : t + 1f - start;
        return length > 0f ? pos / length : 0f;
    }

    public static Vector2 Project(Vec3 p, EffectView view) => new(
        p.X * view.Right.X + p.Y * view.Right.Y + p.Z * view.Right.Z,
        p.X * view.Up.X + p.Y * view.Up.Y + p.Z * view.Up.Z);

    private static Vec3 Lerp(Vec3 a, Vec3 b, float t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);

    private static Vec3 LerpPow(Vec3 a, Vec3 b, float t, Vec3 exponent) => new(
        a.X + (b.X - a.X) * MathF.Pow(t, Safe(exponent.X)),
        a.Y + (b.Y - a.Y) * MathF.Pow(t, Safe(exponent.Y)),
        a.Z + (b.Z - a.Z) * MathF.Pow(t, Safe(exponent.Z)));

    private static float Safe(float exponent) => exponent > 0.0001f ? exponent : 1f;

    /// <summary>
    /// Скалярный размер спрайта по вектору масштаба: геометрическое среднее двух наибольших компонент
    /// (плоскость со сжатой осью X даёт размер её видимых сторон).
    /// </summary>
    public static float SpriteSize(Vec3 scale)
    {
        Span<float> v = [MathF.Abs(scale.X), MathF.Abs(scale.Y), MathF.Abs(scale.Z)];
        v.Sort();
        var a = v[2];
        var b = v[1];
        return b > 1e-4f ? MathF.Sqrt(a * b) : a;
    }

    private static float Hash01(int a, int b, int c)
    {
        unchecked
        {
            var h = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ (uint)(c * 83492791);
            h ^= h >> 16;
            h *= 0x7feb352d;
            h ^= h >> 15;
            h *= 0x846ca68b;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }

    /// <summary>
    /// Считает состояние всех компонентов и список спрайтов/источников света на заданный момент.
    /// </summary>
    /// <param name="effect">Разобранный FXID; в компонентах обновляются <see cref="PreparedComponent.Active"/> и <see cref="PreparedComponent.Life"/>.</param>
    /// <param name="strength">Значение силы эффекта 0..1.</param>
    /// <param name="timeSec">Секунды с начала эффекта (для анимации материалов и гравитации).</param>
    /// <param name="durationSec">Длительность эффекта в секундах (для пересчёта жизни частиц в секунды).</param>
    /// <param name="view">Плоскость проекции.</param>
    /// <param name="output">Список команд отрисовки (очищается и заполняется заново).</param>
    /// <param name="attachSpeed">
    /// Скорость точки привязки вдоль +X (ед./с): нужна шлейфам (тип 5), которые рисуются между предыдущим и текущим
    /// положением точки. В превью точка просто летит с постоянной скоростью от начала координат.
    /// </param>
    /// <param name="origin">
    /// Положение точки привязки эффекта в сцене (ед.). Все компоненты, кроме шлейфов, сдвигаются на него; шлейф всегда
    /// начинается в начале координат (позиция привязки при старте) и тянется до <paramref name="attachSpeed"/> * время.
    /// </param>
    /// <param name="maxTrailLength">Ограничение длины шлейфа (например, расстояние до точки попадания).</param>
    /// <param name="originAt">
    /// Положение точки привязки как функция локального времени эффекта (для спавнера, тип 8: частицы остаются в мире
    /// там, где были созданы). Если не задана, точка неподвижна в <paramref name="origin"/>.
    /// </param>
    /// <param name="cameraDistance">
    /// Расстояние от камеры до эффекта (ед.). Компоненты типа 4 («объект+дист», вспышки <c>glow_eng</c> во взрывах) растут с
    /// расстоянием: прогресс анимации = жизнь * расстояние / ReferenceDistance. 0 - не учитывать (полный размер).
    /// </param>
    /// <param name="frameDt">Длительность кадра игры; нужна только при <paramref name="resetTrailEachFrame"/>.</param>
    /// <param name="resetTrailEachFrame">
    /// Эмулировать <c>NotifyAttachPointMoved</c> каждый кадр (шлейф = путь точки за один кадр: скорость * dt). Движок
    /// рассылает его только эффектам, привязанным через UpdateCachedLocalPoint (флаг 0x1000); у эффектов, созданных
    /// командами CTL, этого нет, и шлейф тянется от точки старта (проверено по наблюдению в игре: лазер рисуется целиком
    /// от орудия до цели и затухает).
    /// </param>
    public static void Evaluate(
        PreparedEffect effect,
        float strength,
        float timeSec,
        float durationSec,
        EffectView view,
        List<DrawItem> output,
        float attachSpeed = 80f,
        Vector3 origin = default,
        float maxTrailLength = float.MaxValue,
        float frameDt = 1f / 60f,
        EffectFrame? frame = null,
        bool resetTrailEachFrame = false,
        Func<float, Vector3>? originAt = null,
        float cameraDistance = 0f,
        Vector3 trailOffset = default)
    {
        output.Clear();
        var originOffset = Project(new Vec3(origin.X, origin.Y, origin.Z), view);

        // Компоненты считаются в локальных осях эффекта: вид переписывается в эти оси; шлейф лежит в мировых координатах.
        var frameView = view;
        var frameScale = 1f;
        if (frame is { } f)
        {
            frameView = new EffectView(
                new Vector3(Vector3.Dot(f.Ex, view.Right), Vector3.Dot(f.Ey, view.Right), Vector3.Dot(f.Ez, view.Right)),
                new Vector3(Vector3.Dot(f.Ex, view.Up), Vector3.Dot(f.Ey, view.Up), Vector3.Dot(f.Ez, view.Up)));
            frameScale = f.Scale;
        }

        // Масштаб из заголовка FXID (+0x30): InitFromFxidData кладёт его в scale хэндла, SetEffectScale умножает на него.
        var h = effect.Header;
        var headerScale = (MathF.Abs(h.ScaleX) + MathF.Abs(h.ScaleY) + MathF.Abs(h.ScaleZ)) / 3f;
        if (headerScale > 1e-6f)
            frameScale *= headerScale;

        foreach (var c in effect.Components)
        {
            var firstItem = output.Count;
            if (!c.HasWindow)
            {
                c.Active = false;
                continue;
            }

            c.Active = InWindow(strength, c.WindowStart, c.WindowEnd);
            c.Life = c.Active ? Math.Clamp(WindowLife(strength, c.WindowStart, c.WindowEnd), 0f, 1f) : 0f;
            if (!c.Active)
                continue;

            switch (c.Type)
            {
                case FxidComponentType.Billboard when c.Light is { } light:
                {
                    var p = Lerp(light.PositionStart, light.PositionEnd, c.Life);
                    var range = light.RangeStart_maybe + (light.RangeEnd_maybe - light.RangeStart_maybe) * c.Life;
                    var r = light.ColorStart_maybe[0] + (light.ColorEnd_maybe[0] - light.ColorStart_maybe[0]) * c.Life;
                    var g = light.ColorStart_maybe[1] + (light.ColorEnd_maybe[1] - light.ColorStart_maybe[1]) * c.Life;
                    var b = light.ColorStart_maybe[2] + (light.ColorEnd_maybe[2] - light.ColorStart_maybe[2]) * c.Life;
                    output.Add(new DrawItem(
                        DrawKind.Light, c.Index, Project(p, frameView), MathF.Max(range, 0.1f), 1f,
                        new Vector3(r, g, b), null, c.Label));
                    break;
                }
                case FxidComponentType.AnimParticle:
                case FxidComponentType.AnimBillboard:
                case FxidComponentType.AnimModel:
                    if (c.Object is { } obj)
                        // Вектор до камеры берётся в локальной системе эффекта (TransformPointWorldToLocal по матрице с масштабом эффекта),
                        // поэтому расстояние делится на общий масштаб (EffectType4::RenderContents, Effect.dll 0x10011750).
                        EvaluateObject(c, obj, strength, frameView, output, cameraDistance / MathF.Max(frameScale, 1e-4f));
                    break;
                case FxidComponentType.Plane:
                case FxidComponentType.Cube:
                    if (c.Particle is { } particle)
                        EvaluateParticles(c, particle, strength, timeSec, durationSec, frameView, output);
                    break;
                case FxidComponentType.Trail when c.Trail is { } trail:
                    var tracksAttach = resetTrailEachFrame && (effect.Header.Flags & FxidEffectFlags.TracksAttachPoint_maybe) != 0;
                    EvaluateTrail(c, trail, timeSec, attachSpeed, maxTrailLength, tracksAttach ? frameDt : -1f, view, output);
                    if (trailOffset != Vector3.Zero)
                    {
                        // начало шлейфа - точка создания снаряда (дуло)
                        var shift = Project(new Vec3(trailOffset.X, trailOffset.Y, trailOffset.Z), view);
                        for (var i = firstItem; i < output.Count; i++)
                            output[i] = output[i] with { Position = output[i].Position + shift };
                    }

                    firstItem = output.Count;
                    break;
                case FxidComponentType.Model when c.Spawner is { } spawner:
                    EvaluateSpawner(
                        c, spawner, timeSec, originAt ?? (t => origin + new Vector3(attachSpeed * t, 0f, 0f)), view, output);
                    firstItem = output.Count;
                    break;
                case FxidComponentType.Trail:
                case FxidComponentType.Model:
                    output.Add(new DrawItem(
                        DrawKind.Marker, c.Index, Vector2.Zero, 0.15f, 1f, new Vector3(1f, 0.8f, 0.3f), null, c.Label));
                    break;
            }

            if (originOffset != Vector2.Zero || frameScale != 1f)
            {
                for (var i = firstItem; i < output.Count; i++)
                {
                    var item = output[i];
                    output[i] = item with
                    {
                        Position = item.Position * frameScale + originOffset,
                        Size = item.Size * frameScale,
                        Width = item.Width * frameScale,
                        Height = item.Height * frameScale,
                        HalfU = item.HalfU * frameScale,
                        HalfV = item.HalfV * frameScale,
                    };
                }
            }
        }
    }

    private static Vec3 Cross(Vec3 a, Vec3 b) => new(
        a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    private static Vec3 Normalize(Vec3 v, Vec3 fallback)
    {
        var len = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return len > 1e-5f ? new Vec3(v.X / len, v.Y / len, v.Z / len) : fallback;
    }

    private static Vec3 Scaled(Vec3 v, float k) => new(v.X * k, v.Y * k, v.Z * k);

    private static void EvaluateObject(
        PreparedComponent c,
        EffectType3Data obj,
        float strength,
        EffectView view,
        List<DrawItem> output,
        float cameraDistance = 0f)
    {
        var life = c.Life;
        if (c.ReferenceDistance > 0f && cameraDistance > 0f)
            life = Math.Clamp(life * cameraDistance / c.ReferenceDistance, 0f, 1f);

        var position = LerpPow(obj.PositionStart, obj.PositionEnd, life, obj.PositionExponent);
        var scale = LerpPow(obj.ScaleStart, obj.ScaleEnd, life, obj.ScaleExponent);
        var direction = Normalize(Lerp(obj.DirectionStart, obj.DirectionEnd, life), new Vec3(1f, 0f, 0f));
        var value = obj.IntensityStart +
                    (obj.IntensityEnd - obj.IntensityStart) * MathF.Pow(life, Safe(obj.IntensityExponent));

        // Фаза выбирает кадр материала (EffectContents::Draw): v = t^e * (End - Start) + Start, где t - сила эффекта при
        // PhaseStart < 0, иначе жизнь в окне; в рендер уходит дробная часть v.
        var phaseStart = MathF.Max(obj.PhaseStart, 0f);
        var t = obj.PhaseUsesEffectStrength ? strength : life;
        var v = MathF.Pow(Math.Clamp(t, 0f, 1f), Safe(obj.PhaseExponent)) * (obj.PhaseEnd - phaseStart) + phaseStart;
        var phase = v - MathF.Floor(v);

        // Форма спрайта по ESpace: 0 - свободный билборд к камере, 1 - осевой билборд (вытянут вдоль direction,
        // разворачивается вокруг неё к камере), 2/3 - плоскость с нормалью direction.
        var cameraForward = Cross(view.Right.AsVec(), view.Up.AsVec());
        Vec3 halfU;
        Vec3 halfV;
        switch (obj.Space)
        {
            case 1:
            {
                var side = Normalize(Cross(direction, cameraForward), view.Up.AsVec());
                halfU = Scaled(direction, MathF.Abs(scale.X) * 0.5f);
                halfV = Scaled(side, MathF.Abs(scale.Y) * 0.5f);
                break;
            }
            case 0:
            {
                var size = (MathF.Abs(scale.Y) + MathF.Abs(scale.Z)) * 0.5f;
                halfU = Scaled(view.Right.AsVec(), size * 0.5f);
                halfV = Scaled(view.Up.AsVec(), size * 0.5f);
                break;
            }
            default:
            {
                var u = Normalize(Cross(direction, new Vec3(0f, 0f, 1f)), new Vec3(0f, 1f, 0f));
                var w = Cross(direction, u);
                halfU = Scaled(u, MathF.Abs(scale.Y) * 0.5f);
                halfV = Scaled(w, MathF.Abs(scale.Z) * 0.5f);
                break;
            }
        }

        output.Add(new DrawItem(
            DrawKind.Sprite, c.Index, Project(position, view), SpriteSize(scale), Math.Clamp(value, 0f, 1f),
            Vector3.One, obj.Resource.ResourceName, c.Label,
            HalfU: Project(halfU, view), HalfV: Project(halfV, view), Phase: phase));
    }

    /// <summary>
    /// Шлейф (тип 5): отрезок от начальной точки (позиция привязки при старте эффекта; движок сбрасывает её сообщением
    /// NotifyAttachPointMoved) до текущей позиции привязки. В превью точка летит вдоль +X со скоростью
    /// <paramref name="attachSpeed"/>. Хвост не укорачивается: число кусков = floor(длина / MaxPieceLength) в пределах
    /// 1..MaxPieces, при превышении куски растягиваются. Текстура повторяется каждые <c>TextureTileLength</c>
    /// (отрицательная прокрутка = по секундам с начала эффекта), ширина идёт от WidthStart к WidthEnd, прозрачность
    /// от ValueStart к ValueEnd по окну. Проверено по EffectType5::Render (Effect.dll).
    /// </summary>
    private static void EvaluateTrail(
        PreparedComponent c,
        EffectTrailData trail,
        float timeSec,
        float attachSpeed,
        float maxLength,
        float trackedFrameDt,
        EffectView view,
        List<DrawItem> output)
    {
        const int maxTiles = 400;
        var head = MathF.Min(MathF.Max(attachSpeed * timeSec, 0f), maxLength);
        var start = trackedFrameDt >= 0f ? MathF.Max(0f, attachSpeed * (timeSec - trackedFrameDt)) : 0f;
        start = MathF.Min(start, head);
        var length = head - start;
        if (length < 0.01f)
            return;

        var tile = MathF.Max(trail.TextureTileLength, 0.1f);
        var scroll = trail.TextureScrollStart < 0f
            ? timeSec
            : trail.TextureScrollStart + (trail.TextureScrollEnd - trail.TextureScrollStart) * c.Life;
        var shift = (scroll - MathF.Floor(scroll)) * tile;
        var width = MathF.Max(trail.WidthStart + (trail.WidthEnd - trail.WidthStart) * c.Life, 0.01f);
        var value = Math.Clamp(trail.ValueStart + (trail.ValueEnd - trail.ValueStart) * c.Life, 0f, 1f);

        var axis = new Vec3(1f, 0f, 0f);
        var cameraForward = Cross(view.Right.AsVec(), view.Up.AsVec());
        var side = Normalize(Cross(axis, cameraForward), view.Up.AsVec());
        var halfV = Project(Scaled(side, width * 0.5f), view);

        var count = 0;
        for (var x = start - shift; x < head && count < maxTiles; x += tile, count++)
        {
            var x0 = MathF.Max(x, start);
            var x1 = MathF.Min(x + tile, head);
            if (x1 <= x0)
                continue;

            var center = Project(new Vec3((x0 + x1) * 0.5f, 0f, 0f), view);
            var halfU = Project(Scaled(axis, (x1 - x0) * 0.5f), view);
            output.Add(new DrawItem(
                DrawKind.Sprite, c.Index, center, width, value, Vector3.One, trail.Resource.ResourceName, c.Label,
                HalfU: halfU, HalfV: halfV));
        }
    }

    /// <summary>
    /// Спавнер (тип 8, например <c>smoke_rk_01</c> - дымовой след ракеты): каждые <c>interval</c> секунд (между
    /// SpawnIntervalStart и End по значению окна) в точке привязки создаётся частица, которая остаётся в мире. Частицы живут в
    /// кольцевом буфере из MaxParticles слотов, поэтому возраст частицы = (номер слота от головы + доля интервала) /
    /// MaxParticles, то есть живёт MaxParticles * interval секунд. Траектория и размер идут от Start к End по возрасту, значение
    /// (прозрачность) - от ValueLerpStart к End. Проверено по EffectType8::EvaluateAtTime / Render (Effect.dll).
    /// </summary>
    private static void EvaluateSpawner(
        PreparedComponent c,
        EffectSpawnerData sp,
        float timeSec,
        Func<float, Vector3> originAt,
        EffectView view,
        List<DrawItem> output)
    {
        var interval = MathF.Max(sp.SpawnIntervalStart + (sp.SpawnIntervalEnd - sp.SpawnIntervalStart) * c.Life, 0.005f);
        var ring = (int)Math.Clamp(sp.MaxParticles, 1u, 200u);
        var lifeSeconds = interval * ring;
        var last = (int)MathF.Floor(timeSec / interval);
        var first = Math.Max(1, last - ring);

        var movement = Project(new Vec3(1f, 0f, 0f), view);
        var direction = movement.LengthSquared() > 1e-6f ? Vector2.Normalize(movement) : new Vector2(1f, 0f);

        for (var k = first; k <= last; k++)
        {
            var age = timeSec - k * interval;
            var f = Math.Clamp(age / lifeSeconds, 0f, 1f);
            if (age < 0f || age > lifeSeconds)
                continue;

            var o = originAt(k * interval);
            var path = Lerp(sp.TrajectoryStart, sp.TrajectoryEnd, f);
            var jitterY = (Hash01(c.Index, k, 11) - 0.5f) * sp.SpawnBox.Y;
            var jitterZ = (Hash01(c.Index, k, 12) - 0.5f) * sp.SpawnBox.Z;
            // Система координат частицы: X - направление движения точки привязки (вид: вперёд), Y - вверх, Z - к зрителю.
            var position = new Vec3(o.X + path.X, o.Y + path.Y + jitterY, o.Z + path.Z + jitterZ);

            var size = Lerp(sp.SizeStart, sp.SizeEnd, f);
            var sx = MathF.Abs(size.X + (Hash01(c.Index, k, 13) - 0.5f) * sp.SizeJitterBox.X);
            var sy = MathF.Abs(size.Y + (Hash01(c.Index, k, 14) - 0.5f) * sp.SizeJitterBox.Y);
            var sz = MathF.Abs(size.Z + (Hash01(c.Index, k, 15) - 0.5f) * sp.SizeJitterBox.Z);
            var width = sx;
            var height = (sy + sz) * 0.5f;

            var alpha = sp.ValueLerpStart + (sp.ValueLerpEnd - sp.ValueLerpStart) * MathF.Pow(f, Safe(sp.ValueLerpExponent));
            var halfU = direction * (width * 0.5f);
            var halfV = new Vector2(-direction.Y, direction.X) * (height * 0.5f);
            output.Add(new DrawItem(
                DrawKind.Sprite, c.Index, Project(position, view), MathF.Max(width, height),
                Math.Clamp(alpha, 0f, 1f), Vector3.One, sp.Resource.ResourceName, c.Label, width, height, direction,
                halfU, halfV, Math.Clamp(f, 0f, 0.99f)));
        }
    }

    private static void EvaluateParticles(
        PreparedComponent c,
        EffectParticleData p,
        float strength,
        float timeSec,
        float durationSec,
        EffectView view,
        List<DrawItem> output)
    {
        var v = c.Life;
        var rate = p.RepeatRate;
        if (rate > 1f)
        {
            var x = v * rate;
            v = x - MathF.Floor(x);
        }
        else if (rate > 0f)
        {
            v /= rate;
        }

        var groups = (int)Math.Clamp(p.Groups, 1u, 64u);
        var perGroup = (int)Math.Clamp(p.PerGroup, 1u, 64u);
        var lifeFraction = Math.Clamp(p.ParticleLifeFraction, 0.001f, 1f);
        var count = 0;

        for (var g = 0; g < groups && count < MaxParticlesPerComponent; g++)
        {
            var offset = (1f - lifeFraction) * g / groups;
            var u = (v - offset) / lifeFraction;
            if (u < 0f || u > 1f)
                continue;

            for (var k = 0; k < perGroup && count < MaxParticlesPerComponent; k++, count++)
            {
                var id = g * perGroup + k;
                var spawn = new Vec3(
                    (Hash01(c.Index, id, 1) - 0.5f) * p.SpawnBox.X,
                    (Hash01(c.Index, id, 2) - 0.5f) * p.SpawnBox.Y,
                    (Hash01(c.Index, id, 3) - 0.5f) * p.SpawnBox.Z);
                var path = Lerp(p.TrajectoryStart, p.TrajectoryEnd, u);
                var t = u * lifeFraction * MathF.Max(durationSec, 0.001f);
                var drop = p.GravityFactor * ParticleGravity * 0.5f * t * t;
                var position = new Vec3(path.X + spawn.X, path.Y + spawn.Y, path.Z + spawn.Z - drop);

                var sizeVec = c.Type == FxidComponentType.Plane ? Lerp(p.SizeStart, p.SizeEnd, u) : p.SizeStart;
                // Спрайт частицы - прямоугольник: X - длина (у «штрихов» splash/spark она растёт от SizeStart к SizeEnd),
                // высота - среднее Y и Z (в файлах они равны).
                var sx = MathF.Abs(sizeVec.X + (Hash01(c.Index, id, 4) - 0.5f) * p.SizeJitterBox.X);
                var sy = MathF.Abs(sizeVec.Y + (Hash01(c.Index, id, 5) - 0.5f) * p.SizeJitterBox.Y);
                var sz = MathF.Abs(sizeVec.Z + (Hash01(c.Index, id, 6) - 0.5f) * p.SizeJitterBox.Z);
                var width = sx;
                var height = (sy + sz) * 0.5f;

                // Штрих вытянут вдоль скорости частицы: проецируем 3D скорость на плоскость вида. Если частица летит почти
                // вдоль луча взгляда, штрих укорачивается до высоты (виден «точкой»), как у плоского предмета в 3D.
                var lifeSeconds = MathF.Max(lifeFraction * MathF.Max(durationSec, 0.001f), 0.001f);
                var velocity = new Vec3(
                    (p.TrajectoryEnd.X - p.TrajectoryStart.X) / lifeSeconds,
                    (p.TrajectoryEnd.Y - p.TrajectoryStart.Y) / lifeSeconds,
                    (p.TrajectoryEnd.Z - p.TrajectoryStart.Z) / lifeSeconds - p.GravityFactor * ParticleGravity * t);
                var speed = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
                var direction = Vector2.Zero;
                if (speed > 1e-4f)
                {
                    var projected = Project(new Vec3(velocity.X / speed, velocity.Y / speed, velocity.Z / speed), view);
                    var projectedLength = projected.Length();
                    if (projectedLength > 1e-3f)
                    {
                        direction = projected / projectedLength;
                        width = MathF.Max(width * projectedLength, height);
                    }
                    else
                    {
                        width = height;
                    }
                }

                var alpha = p.ValueLerpStart_maybe +
                            (p.ValueLerpEnd_maybe - p.ValueLerpStart_maybe) * MathF.Pow(u, Safe(p.ValueExponent_maybe));

                // Фаза кадра материала частицы (EffectType7::Render): (окно - сдвиг группы), затем деление или дробная часть
                // по RepeatRate.
                var age = c.Life - offset;
                var phase = p.RepeatRate <= 1f
                    ? age / p.RepeatRate
                    : age * p.RepeatRate - MathF.Floor(age * p.RepeatRate);
                phase = phase >= 1f ? 0.99f : Math.Clamp(phase, 0f, 0.99f);

                var halfU = direction * (width * 0.5f);
                var halfV = new Vector2(-direction.Y, direction.X) * (height * 0.5f);
                output.Add(new DrawItem(
                    DrawKind.Sprite, c.Index, Project(position, view), MathF.Max(width, height),
                    Math.Clamp(alpha, 0f, 1f), Vector3.One, p.Resource.ResourceName, c.Label, width, height, direction,
                    halfU, halfV, phase));
            }
        }
    }
}
