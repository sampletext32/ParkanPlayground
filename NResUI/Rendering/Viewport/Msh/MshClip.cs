using System.Globalization;

namespace NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Клип анимации: отрезок общей шкалы MSH, который игра проигрывает в одном состоянии управления (<c>ControlStateNode</c> из CTL).
/// Само MSH клипов не хранит - у него одна шкала времени (<c>MaxAnimationTime</c>), на которой лежат ключи всех движений подряд.
/// </summary>
/// <param name="StateIndex">Индекс состояния в CTL.</param>
/// <param name="Anim1Start">Начало диапазона 1 (время шкалы MSH; отрицательное - диапазон не используется).</param>
/// <param name="Anim1End">Конец диапазона 1.</param>
/// <param name="Anim2Start">Начало диапазона 2.</param>
/// <param name="Anim2End">Конец диапазона 2.</param>
/// <param name="Weight2AtMinSpeed">Вес диапазона 2 при минимальной скорости состояния; при максимальной он равен 1.</param>
/// <param name="DurationMs">Длительность из состояния, мс (0 - не задана: движок считает её по смещению корня и скорости).</param>
/// <param name="MinSpeed">Нижняя граница полосы скоростей состояния: наибольшая по модулю компонента минимальной скорости.</param>
/// <param name="MaxSpeed">Верхняя граница полосы скоростей: наибольшая по модулю компонента максимальной скорости.</param>
/// <param name="Flags">Флаги состояния CTL.</param>
public sealed record MshClip(
    int StateIndex,
    float Anim1Start,
    float Anim1End,
    float Anim2Start,
    float Anim2End,
    float Weight2AtMinSpeed,
    float DurationMs,
    float MinSpeed,
    float MaxSpeed,
    uint Flags)
{
    /// <summary>Основной диапазон: второй, если он задан, иначе первый.</summary>
    public (float Start, float End) MainRange =>
        Anim2Start >= 0.0f ? (Anim2Start, Anim2End) : (Anim1Start, Anim1End);

    /// <summary>Длительность задана состоянием (поле не нулевое); иначе движок считает её по смещению корня и скорости.</summary>
    public bool HasFixedDuration => DurationMs > 0.0f;

    /// <summary>Диапазон проигрывается от большего времени к меньшему.</summary>
    public bool IsReverse => MainRange.End < MainRange.Start;

    /// <summary>Подпись для списка.</summary>
    public string Label
    {
        get
        {
            var (start, end) = MainRange;
            var arrow = IsReverse ? "<-" : "->";
            var label = string.Create(CultureInfo.InvariantCulture,
                $"#{StateIndex}  {start:0.##} {arrow} {end:0.##}");

            if (Anim2Start >= 0.0f && Anim1Start >= 0.0f && Weight2AtMinSpeed < 1.0f)
                label += string.Create(CultureInfo.InvariantCulture, $"  (+ {Anim1Start:0.##}..{Anim1End:0.##}, w {Weight2AtMinSpeed:0.##})");

            return label;
        }
    }
}
