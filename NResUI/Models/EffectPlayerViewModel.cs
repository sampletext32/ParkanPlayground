using NResUI.Abstractions;
using NResUI.EffectPlayer;

namespace NResUI.Models;

/// <summary>Откуда берётся «сила» эффекта 0..1, которая сравнивается с окнами активности компонентов.</summary>
public enum EffectStrengthMode
{
    /// <summary>Время / длительность, как при <c>StartEffect(handle, 1)</c> (так игра запускает, например, щит).</summary>
    Life = 0,

    /// <summary>Источник из заголовка FXID, если он зависит только от времени (иначе как <see cref="Life"/>).</summary>
    FromFile = 1,

    /// <summary>Ручной ползунок: для источников, зависящих от состояния игры (скорость, жизнь, анимация детали).</summary>
    Manual = 2,
}

public sealed class EffectPlayerViewModel : IService
{
    public IReadOnlyList<string> EffectNames { get; set; } = [];
    public string? ListError { get; set; }
    public bool NamesLoaded { get; set; }

    public string Filter = string.Empty;
    public string? SelectedName { get; set; }
    public PreparedEffect? Effect { get; set; }
    public string? Error { get; set; }

    /// <summary>Секунды с начала эффекта.</summary>
    public float TimeSec;

    public bool Playing;
    public bool MuteSound;

    /// <summary>Скорость точки привязки вдоль +X для шлейфов, ед./с.</summary>
    public float AttachSpeed = 80f;
    public float Volume = 0.8f;

    /// <summary>Сила на предыдущем кадре (для срабатывания одиночных звуков); отрицательная после перезапуска.</summary>
    public float PrevStrength = -1f;
    public bool Loop = true;
    public float Speed = 1f;

    public int StrengthModeIndex;
    public float ManualStrength;

    /// <summary>Поворот камеры вокруг вертикальной оси, градусы (0 = сбоку, 90 = спереди).</summary>
    public float YawDeg;

    /// <summary>Наклон камеры, градусы (0 = горизонтально, 90 = сверху).</summary>
    public float PitchDeg;

    public float PixelsPerUnit = 120f;
    public float SpriteScale = 1f;
    public bool ShowGrid = true;

    /// <summary>Режим смешивания: 0 - по материалу, 1 - всегда alpha, 2 - всегда аддитивно.</summary>
    public int BlendModeIndex;
    public bool ShowMarkers = true;
    public bool DarkBackground = true;
    public bool ShowInactiveInTimeline = true;

    public EffectStrengthMode StrengthMode => (EffectStrengthMode)StrengthModeIndex;
    public EffectView View => EffectView.FromAngles(YawDeg, PitchDeg);
}
