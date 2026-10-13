namespace ArealMapLib;

/// <summary>
/// Карта ареалов (<c>Land.map</c>, NRes с единственной записью <c>ArealMap</c> типа 0x0C). Содержимое записи: ареалы и сетка ячеек,
/// в каждой ячейке - список индексов ареалов, пересекающих её. Во всех 29 картах игры сетка 128 x 128, а размер записи равен сумме частей.
/// </summary>
/// <param name="Areals">Ареалы (303..4990 в картах игры).</param>
/// <param name="GridWidth">Ширина сетки в ячейках (128).</param>
/// <param name="GridHeight">Высота сетки в ячейках (128).</param>
/// <param name="Cells">Ячейки сетки: <c>Cells[x * GridHeight + y]</c> - индексы ареалов в <see cref="Areals"/>. В файле ячейки идут столбцами: внешний цикл по x.</param>
public sealed record ArealMapFile(
    IReadOnlyList<ArealRecord> Areals,
    int GridWidth,
    int GridHeight,
    IReadOnlyList<ushort[]> Cells)
{
    /// <summary>Индексы ареалов, пересекающих ячейку (x, y).</summary>
    /// <param name="x">Номер столбца сетки.</param>
    /// <param name="y">Номер строки сетки.</param>
    public ushort[] GetCell(int x, int y) => Cells[x * GridHeight + y];
}
