namespace NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Канал анимации одной детали из привязки CTL (<c>CtlPieceBindingRecordRaw</c>): значение 0..1 переводится во время шкалы MSH как
/// <c>Start + value * (End - Start)</c>, и только эта деталь ставится на найденное время (остальные идут по общему состоянию).
/// Так у колёсных и шагающих машин крутятся колёса и работают подвески, хотя состояний движения у модели почти нет.
/// </summary>
/// <param name="BindingIndex">Индекс привязки в CTL.</param>
/// <param name="PieceIndex">Индекс детали (узла 0x01) в меше.</param>
/// <param name="Start">Начало диапазона шкалы MSH.</param>
/// <param name="End">Конец диапазона шкалы MSH.</param>
/// <param name="DefaultValue">Значение канала при загрузке (0..1), уже с учётом флага инверсии 0x2.</param>
/// <param name="Speed">Скорость канала, единиц значения в секунду (<c>ChannelSpeed</c>).</param>
/// <param name="Wraps">Значение циклическое (флаг 0x1), иначе ходит туда-обратно.</param>
/// <param name="UsesRange2">Диапазон взят из Anim2 (иначе из Anim1).</param>
public sealed record MshChannel(
    int BindingIndex,
    int PieceIndex,
    float Start,
    float End,
    float DefaultValue,
    float Speed,
    bool Wraps,
    bool UsesRange2)
{
    /// <summary>Время шкалы для значения канала.</summary>
    public float TimeAt(float value) => Start + value * (End - Start);
}
