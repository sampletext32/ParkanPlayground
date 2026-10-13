using MshLib;
using NResUI.Rendering.Inspection;

namespace NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Длительность проигрывания состояния по смещению корня анимации и скорости - так её считает игра
/// (<c>CControlBase::AdvanceControlStateSimulation</c> и <c>BuildControlStateDerivedPlacementData</c>, Control.dll), если в состоянии
/// длительность не задана (поле <c>duration</c> равно 0).
/// </summary>
public static class MshRootMotion
{
    /// <summary>Нижний и верхний пределы длительности одного состояния, с (зажим в игре).</summary>
    private const float MinDurationSeconds = 0.01f;

    private const float MaxDurationSeconds = 5.0f;

    /// <summary>Длина смещения корневой детали (узел 0) между двумя временами шкалы.</summary>
    public static float RootMotionLength(MshModelGeometry geometry, float start, float end)
    {
        if (start < 0.0f || end < 0.0f || geometry.Nodes.Nodes.Count == 0)
            return 0.0f;

        var root = geometry.Nodes.Nodes[0];
        var from = MshAnimationSampler.SamplePieceAnimationAtTime(
            root, geometry.TransformKeyframes, geometry.AnimationMap, start, geometry.MaxAnimationTime).Position;
        var to = MshAnimationSampler.SamplePieceAnimationAtTime(
            root, geometry.TransformKeyframes, geometry.AnimationMap, end, geometry.MaxAnimationTime).Position;

        return (to - from).Length();
    }

    /// <summary>
    /// Вес диапазона 2 при скорости <paramref name="speed"/>: растёт от веса при минимальной скорости до 1 по доле скорости внутри полосы состояния
    /// (<c>anim_blend_end_weight</c>).
    /// </summary>
    public static float Weight2AtSpeed(MshClip clip, float speed)
    {
        var low = MathF.Min(clip.MinSpeed, clip.MaxSpeed);
        var width = MathF.Abs(clip.MaxSpeed - clip.MinSpeed);
        if (width <= 0.0f)
            return clip.Weight2AtMinSpeed;

        var x = Math.Clamp((speed - low) / width, 0.0f, 1.0f);
        return (1.0f - x) * clip.Weight2AtMinSpeed + x;
    }

    /// <summary>
    /// Длительность одного состояния, с: заданная в состоянии, если она не нулевая; иначе
    /// <c>(w * длина2 + (1 - w) * длина1) / скорость</c> в пределах 0.01..5 с (при скорости 0 - 0.01 с, как в игре).
    /// </summary>
    public static float DurationSeconds(MshModelGeometry geometry, MshClip clip, float speed)
    {
        if (clip.DurationMs > 0.0f)
            return clip.DurationMs / 1000.0f;

        if (speed <= 1e-9f)
            return MinDurationSeconds;

        var length1 = clip.Anim1Start >= 0.0f ? RootMotionLength(geometry, clip.Anim1Start, clip.Anim1End) : 0.0f;
        var length2 = clip.Anim2Start >= 0.0f ? RootMotionLength(geometry, clip.Anim2Start, clip.Anim2End) : 0.0f;
        var w = clip.Anim2Start >= 0.0f && clip.Anim1Start >= 0.0f ? Weight2AtSpeed(clip, speed) : 1.0f;

        var length = w * length2 + (1.0f - w) * length1;
        return Math.Clamp(length / speed, MinDurationSeconds, MaxDurationSeconds);
    }

    /// <summary>Длительность последовательности состояний при одной скорости: сумма длительностей отрезков.</summary>
    public static float DurationSeconds(MshModelGeometry geometry, MshClipSequence sequence, float speed)
    {
        var total = 0.0f;
        foreach (var segment in sequence.Segments)
            total += DurationSeconds(geometry, segment, speed);

        return total;
    }
}
