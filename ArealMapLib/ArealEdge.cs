namespace ArealMapLib;

/// <summary>
/// Ребро ареала: отрезок от вершины <c>i</c> до вершины <c>(i + 1) % V</c> и соседний ареал за ним.
/// Проверено по 30951 ареалу в 29 картах: у каждого ребра с соседом ребро соседа с индексом <see cref="NeighbourEdgeIndex"/> - тот же отрезок в обратном направлении
/// (6420 из 6420 в карте 11), то есть связь симметрична.
/// </summary>
/// <param name="NeighbourIndex">Индекс соседнего ареала в <see cref="ArealMapFile.Areals"/>; -1 - соседа нет (внешняя граница карты).</param>
/// <param name="NeighbourEdgeIndex">Номер ребра в соседнем ареале, совпадающего с этим; -1, если соседа нет.</param>
public readonly record struct ArealEdge(int NeighbourIndex, int NeighbourEdgeIndex)
{
    /// <summary>Есть ли у ребра сосед.</summary>
    public bool HasNeighbour => NeighbourIndex >= 0;
}
