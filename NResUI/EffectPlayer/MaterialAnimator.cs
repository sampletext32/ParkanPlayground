using MaterialLib;

namespace NResUI.EffectPlayer;

/// <summary>
/// Результат выбора кадра анимации материала: стадия текстуры, стадия, к которой интерполируются цвета, доля и маска полей.
/// </summary>
/// <param name="StageIndex">Стадия начального ключа: из неё берутся текстура, страница и цвета без интерполяции.</param>
/// <param name="NextStageIndex">Стадия следующего ключа (у последнего - первого).</param>
/// <param name="Progress">Доля времени внутри интервала ключа 0..1.</param>
/// <param name="Fields">Поля, которые интерполируются (<see cref="AnimationTarget"/>: Ambient 1, Diffuse 2, Specular 4, Emissive 8, AmbientAlpha 0x10 = ambient.A).</param>
public readonly record struct MaterialFrame(int StageIndex, int NextStageIndex, float Progress, AnimationTarget Fields);

/// <summary>
/// Выбор стадии материала по времени или фазе, как <c>MatManager::AnimateFrame</c> / <c>GetMaterialPhase</c> (World3D.dll):
/// ключ хранит конец своего интервала, соседние стадии смешиваются по маске полей (<c>Interpolate</c>, 0x100032D0).
/// </summary>
public static class MaterialAnimator
{
    /// <summary>Кадр анимации в момент <paramref name="timeMs"/> с начала (в движке - с <c>lastUsedTime</c> материала).</summary>
    public static MaterialFrame Sample(MaterialFile material, float timeMs)
    {
        if (material.Stages.Count == 0)
            return new MaterialFrame(-1, -1, 0f, 0);

        if (material.Animations.Count == 0 || material.Animations[0].Keys.Count == 0)
            return new MaterialFrame(0, 0, 0f, 0);

        var animation = material.Animations[0];
        var keys = animation.Keys;
        float total = keys[^1].DurationMs;
        if (total <= 0 || keys.Count < 2)
            return new MaterialFrame(ClampStage(keys[0].StageIndex, material), ClampStage(keys[0].StageIndex, material), 0f, 0);

        var t = timeMs;
        switch (animation.LoopMode)
        {
            case AnimationLoopMode.PingPong:
            {
                var p = t % (2 * total);
                t = p > total ? 2 * total - p : p;
                break;
            }
            case AnimationLoopMode.Once:
                if (t >= total)
                {
                    // AnimateFrame (loop_mode 2) после конца: начальный ключ = последний, позиция 0, следующий = ключ 0, поэтому
                    // доля (0 - начало) / (конец - начало) отрицательна, и цвета по маске экстраполируются от последней стадии.
                    var last = keys.Count - 1;
                    float begin = keys[last - 1].DurationMs;
                    var span = total - begin;
                    return new MaterialFrame(
                        ClampStage(keys[last].StageIndex, material),
                        ClampStage(keys[0].StageIndex, material),
                        span > 0f ? -begin / span : 0f,
                        animation.Target);
                }

                break;
            case AnimationLoopMode.Random:
                // Каждый вызов берёт случайную позицию 0..длительность (lcg_rand); во вьюере случайность привязана к кадру времени.
                t = (float)(Hash((uint)Math.Max(0f, MathF.Floor(timeMs))) % (uint)Math.Max(1f, total));
                break;
            default:
                t %= total;
                break;
        }

        return FindInterval(material, animation, t, total);
    }

    /// <summary>
    /// Кадр по фазе 0..1 (<c>EffectContents::Draw</c> ограничивает фазу и передаёт менеджеру материалов): позиция = фаза · длительность.
    /// Как в движке, позиция, не попавшая ни в один интервал (фаза = 1.0), даёт первый ключ.
    /// </summary>
    public static MaterialFrame SampleByPhase(MaterialFile material, float phase)
    {
        if (material.Stages.Count == 0)
            return new MaterialFrame(-1, -1, 0f, 0);

        if (material.Animations.Count == 0 || material.Animations[0].Keys.Count < 2)
            return Sample(material, 0f);

        var animation = material.Animations[0];
        float total = animation.Keys[^1].DurationMs;
        var clamped = Math.Clamp(phase, 0f, 1f);
        return FindInterval(material, animation, clamped * total, total);
    }

    private static MaterialFrame FindInterval(MaterialFile material, MaterialAnimation animation, float t, float total)
    {
        var keys = animation.Keys;

        // Движок идёт от последнего ключа вниз и ищет prev <= t < cur; не нашёл - ключ 0 (при k = 0 нижняя граница 0).
        var k = 0;
        for (var i = keys.Count - 1; i > 0; i--)
        {
            if (keys[i - 1].DurationMs <= t && t < keys[i].DurationMs)
            {
                k = i;
                break;
            }
        }

        var start = keys[k];
        var next = keys[(k + 1) % keys.Count];
        var begin = k == 0 ? 0f : keys[k - 1].DurationMs;
        var span = start.DurationMs - begin;
        var progress = span > 0f ? Math.Clamp((t - begin) / span, 0f, 1f) : 0f;
        return new MaterialFrame(
            ClampStage(start.StageIndex, material),
            ClampStage(next.StageIndex, material),
            progress,
            animation.Target);
    }

    /// <summary>Стадия по времени (без интерполяции цветов).</summary>
    public static int PickStage(MaterialFile material, float timeMs) => Sample(material, timeMs).StageIndex;

    /// <summary>Полная длительность анимации материала в мс (время окончания последней стадии), 0 если анимации нет.</summary>
    public static float TotalDurationMs(MaterialFile material) =>
        material.Animations.Count > 0 && material.Animations[0].Keys.Count > 0
            ? material.Animations[0].Keys[^1].DurationMs
            : 0f;

    /// <summary>Стадия по фазе 0..1 (без интерполяции цветов).</summary>
    public static int PickStageByPhase(MaterialFile material, float phase) => SampleByPhase(material, phase).StageIndex;

    private static uint Hash(uint x)
    {
        x ^= x >> 16;
        x *= 0x7FEB352Du;
        x ^= x >> 15;
        x *= 0x846CA68Bu;
        x ^= x >> 16;
        return x;
    }

    private static int ClampStage(int index, MaterialFile material) =>
        Math.Clamp(index, 0, material.Stages.Count - 1);
}
