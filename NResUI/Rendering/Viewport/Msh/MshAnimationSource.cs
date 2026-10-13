namespace NResUI.Rendering.Viewport.Msh;

/// <summary>Источник времени анимации в плеере.</summary>
public enum MshAnimationSource
{
    /// <summary>Вся шкала MSH подряд (все движения склеены).</summary>
    Timeline,

    /// <summary>Отдельное состояние CTL.</summary>
    Clips,

    /// <summary>Цепочка состояний: целая фаза движения.</summary>
    Chains,

    /// <summary>Каналы отдельных деталей из привязок CTL (колёса, подвески).</summary>
    Channels
}
