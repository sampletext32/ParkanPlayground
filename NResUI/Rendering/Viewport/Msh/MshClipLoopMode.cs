namespace NResUI.Rendering.Viewport.Msh;

/// <summary>Поведение плеера по достижении конца клипа или цепочки.</summary>
public enum MshClipLoopMode
{
    /// <summary>Остановиться на последнем кадре.</summary>
    Once,

    /// <summary>Начать сначала.</summary>
    Loop,

    /// <summary>Проигрывать то вперёд, то назад.</summary>
    PingPong
}
