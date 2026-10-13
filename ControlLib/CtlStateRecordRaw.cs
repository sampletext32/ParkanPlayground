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
    IReadOnlyList<NavNodeLinkSlotRaw> Links)
{
    /// <summary>Анимационные поля состояния (диапазоны шкалы MSH, вес, длительность); остальная часть тела не расшифрована.</summary>
    public CtlStateAnimation Animation => CtlStateAnimation.FromFixedBody(FixedBody);

    /// <summary>
    /// +0x00: маска осей скорости, по которым состояние применимо (<c>ControlStateNode::IsApplicable</c>): 0x1/0x2/0x4 - тангенциальная скорость x/y/z
    /// должна лежать в полосе min..max, 0x10/0x20/0x40 - то же для нормальной скорости.
    /// </summary>
    public uint SpeedAxisMask => BitConverter.ToUInt32(FixedBody, 0x00);

    /// <summary>+0x94: сколько раз состояние ещё можно выбрать (0 - выключено, 0xFFFFFFFF - без ограничения); при выборе целью уменьшается на 1.</summary>
    public uint RemainingActivations => BitConverter.ToUInt32(FixedBody, 0x94);

    /// <summary>+0x98: режим трансформации объекта, в котором состояние доступно (0xFFFFFFFF - в любом); меняется командой 39 <c>CYCLE_OBJECT_TRANSFORMATION</c>.</summary>
    public uint RequiredTransformationMode => BitConverter.ToUInt32(FixedBody, 0x98);
}
