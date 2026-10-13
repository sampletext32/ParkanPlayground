namespace NResUI.Rendering.Viewport.Msh;

/// <summary>Результат разбора CTL для плеера: отдельные клипы и цепочки двух видов.</summary>
/// <param name="Clips">Состояния с различающимися диапазонами, по одному на элемент.</param>
/// <param name="PhaseChains">Цепочки, обрывающиеся на состояниях, которые можно выбрать целью (одна фаза движения: шаг).</param>
/// <param name="JoinedChains">Цепочки, проходящие через такие состояния (несколько фаз подряд: левый + правый шаг).</param>
/// <param name="Channels">Каналы анимации отдельных деталей из привязок CTL (колёса, подвески, стволы).</param>
public sealed record MshClipSet(
    IReadOnlyList<MshClipSequence> Clips,
    IReadOnlyList<MshClipSequence> PhaseChains,
    IReadOnlyList<MshClipSequence> JoinedChains,
    IReadOnlyList<MshChannel> Channels)
{
    public static MshClipSet Empty { get; } = new([], [], [], []);
}
