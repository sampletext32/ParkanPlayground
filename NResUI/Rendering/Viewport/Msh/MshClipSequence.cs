using System.Globalization;

namespace NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Элемент списка плеера: один клип (состояние CTL) или цепочка клипов, идущих подряд. Прогресс 0..1 распределяется по отрезкам
/// пропорционально их длине на шкале MSH, то есть шкала проходится с постоянной скоростью.
/// </summary>
/// <param name="Segments">Отрезки в порядке проигрывания (не пусто).</param>
public sealed record MshClipSequence(IReadOnlyList<MshClip> Segments)
{
    /// <summary>Начало первого отрезка.</summary>
    public float Start => Segments[0].MainRange.Start;

    /// <summary>Конец последнего отрезка.</summary>
    public float End => Segments[^1].MainRange.End;

    /// <summary>Есть ли у отрезков два диапазона (тогда вес имеет смысл менять).</summary>
    public bool HasTwoRanges => Segments[0].Anim1Start >= 0.0f && Segments[0].Anim2Start >= 0.0f;

    /// <summary>Вес диапазона 2 у первого отрезка при минимальной скорости (1, если диапазон один).</summary>
    public float DefaultWeight2 => HasTwoRanges ? Segments[0].Weight2AtMinSpeed : 1.0f;

    /// <summary>Подпись для списка.</summary>
    public string Label
    {
        get
        {
            if (Segments.Count == 1)
                return Segments[0].Label;

            var arrow = End < Start ? "<-" : "->";
            return string.Create(CultureInfo.InvariantCulture,
                $"{Start:0.##} {arrow} {End:0.##}  [{Segments.Count} states: #{Segments[0].StateIndex}..#{Segments[^1].StateIndex}]");
        }
    }

    /// <summary>
    /// Времена семпла и вес для прогресса 0..1 (как <c>CAniMesh::RebuildDirtyPieceMatrices</c>: время = start + (end - start) * progress
    /// по каждому диапазону отрезка).
    /// </summary>
    public MshAnimationBlend Sample(float progress, float weight2)
    {
        progress = Math.Clamp(progress, 0.0f, 1.0f);

        var total = 0.0f;
        foreach (var segment in Segments)
            total += SegmentLength(segment);

        var segmentIndex = 0;
        var local = progress;

        if (Segments.Count > 1 && total > 0.0f)
        {
            var target = progress * total;
            var accumulated = 0.0f;
            for (segmentIndex = 0; segmentIndex < Segments.Count - 1; segmentIndex++)
            {
                var length = SegmentLength(Segments[segmentIndex]);
                if (target <= accumulated + length)
                    break;

                accumulated += length;
            }

            var segmentLength = SegmentLength(Segments[segmentIndex]);
            local = segmentLength > 0.0f ? (target - accumulated) / segmentLength : 0.0f;
        }
        else if (Segments.Count > 1)
        {
            segmentIndex = Math.Min(Segments.Count - 1, (int)(progress * Segments.Count));
            local = progress * Segments.Count - segmentIndex;
        }

        var s = Segments[segmentIndex];
        local = Math.Clamp(local, 0.0f, 1.0f);
        return new MshAnimationBlend(
            s.Anim1Start + (s.Anim1End - s.Anim1Start) * local,
            s.Anim2Start + (s.Anim2End - s.Anim2Start) * local,
            weight2);
    }

    private static float SegmentLength(MshClip clip)
    {
        var (start, end) = clip.MainRange;
        return MathF.Abs(end - start);
    }
}
