using MaterialLib;

namespace NResUI.EffectPlayer;

/// <summary>Выбор стадии материала по времени (анимация материала = последовательность стадий с ключами по времени).</summary>
public static class MaterialAnimator
{
    /// <summary>
    /// Ключ <c>DurationMs</c> в данных строго возрастает (50, 100, ..., 500), поэтому он трактуется как время окончания стадии
    /// с начала цикла. Интерполяция цвета между стадиями не выполняется (во всех проверенных материалах Target = 0).
    /// </summary>
    public static int PickStage(MaterialFile material, float timeMs)
    {
        if (material.Stages.Count == 0)
            return -1;

        if (material.Animations.Count == 0 || material.Animations[0].Keys.Count == 0)
            return 0;

        var animation = material.Animations[0];
        var keys = animation.Keys;
        float total = keys[^1].DurationMs;
        if (total <= 0)
            return ClampStage(keys[0].StageIndex, material);

        var t = timeMs;
        switch (animation.LoopMode)
        {
            case AnimationLoopMode.PingPong:
            {
                var p = t % (2 * total);
                t = p > total ? 2 * total - p : p;
                break;
            }
            case AnimationLoopMode.Clamp:
                t = MathF.Min(t, total - 0.001f);
                break;
            default:
                t %= total;
                break;
        }

        foreach (var key in keys)
        {
            if (t < key.DurationMs)
                return ClampStage(key.StageIndex, material);
        }

        return ClampStage(keys[^1].StageIndex, material);
    }

    /// <summary>Полная длительность анимации материала в мс (время окончания последней стадии), 0 если анимации нет.</summary>
    public static float TotalDurationMs(MaterialFile material) =>
        material.Animations.Count > 0 && material.Animations[0].Keys.Count > 0
            ? material.Animations[0].Keys[^1].DurationMs
            : 0f;

    /// <summary>
    /// Стадия по фазе 0..1: так движок выбирает кадр анимированного материала (<c>EffectContents::Draw</c> передаёт
    /// <c>phase</c> в менеджер материалов, не время). Фаза растягивается на всю анимацию один раз.
    /// </summary>
    public static int PickStageByPhase(MaterialFile material, float phase)
    {
        var total = TotalDurationMs(material);
        return PickStage(material, Math.Clamp(phase, 0f, 0.9999f) * total);
    }

    private static int ClampStage(int index, MaterialFile material) =>
        Math.Clamp(index, 0, material.Stages.Count - 1);
}
