namespace ControlLib;

/// <summary>
/// Узел графа состояний управления (<c>ControlState</c>): фиксированная часть 0x9C байт и набор звеньев опоры. Состояния образуют граф перемещений, по именам функций Control.dll
/// (<c>QueuePathToControlState</c>, <c>SelectNextControlState</c>, <c>AdvanceControlStateSimulation</c>, <c>ApplyControlStateInterpolation</c>) объект ведётся
/// по цепочке состояний, а веса рёбер берутся из матрицы <see cref="CtlFile.EdgeWeightMatrix"/>. В данных состояния есть у 145 файлов из 449 (всего 692 узла);
/// поля фиксированной части не расшифрованы.
/// </summary>
/// <param name="Offset">Смещение узла от начала файла (для отладки).</param>
/// <param name="FixedBody">Фиксированная часть узла (0x9C байт) без разбора.</param>
/// <param name="Links">Звенья узла: число задаётся <see cref="CtlHeader.ControlStateLinkSlotCount"/> (в данных 0..6 на узел).</param>
public sealed record CtlStateRecordRaw(
    long Offset,
    byte[] FixedBody,
    IReadOnlyList<NavNodeLinkSlotRaw> Links);
