using ArealMapLib;

/// <summary>Проверка разбора <c>Land.map</c> на всех картах каталога <c>DATA/MAPS</c>.</summary>
public static class ArealMapCheck
{
    /// <summary>Разбирает каждый <c>Land.map</c> и печатает статистику.</summary>
    /// <param name="mapsDir">Каталог <c>DATA/MAPS</c> игры.</param>
    public static void Run(string mapsDir)
    {
        var total = 0;
        var min = int.MaxValue;
        var max = 0;
        var maps = 0;
        var masks = new SortedDictionary<int, int>();
        var asymmetric = 0;
        foreach (var path in Directory.GetFiles(mapsDir, "Land.map", SearchOption.AllDirectories))
        {
            var map = ArealMapParser.ReadFile(path);
            maps++;
            total += map.Areals.Count;
            min = Math.Min(min, map.Areals.Count);
            max = Math.Max(max, map.Areals.Count);
            foreach (var areal in map.Areals)
            {
                masks[areal.Mask0x28] = masks.GetValueOrDefault(areal.Mask0x28) + 1;
            }

            for (var i = 0; i < map.Areals.Count; i++)
            {
                var edges = map.Areals[i].Edges;
                for (var e = 0; e < edges.Length; e++)
                {
                    if (!edges[e].HasNeighbour)
                    {
                        continue;
                    }

                    var back = map.Areals[edges[e].NeighbourIndex].Edges[edges[e].NeighbourEdgeIndex];
                    if (back.NeighbourIndex != i || back.NeighbourEdgeIndex != e)
                    {
                        asymmetric++;
                    }
                }
            }
        }

        Console.WriteLine($"Карт: {maps}, ареалов: {total} (в карте от {min} до {max}), несимметричных рёбер: {asymmetric}");
        Console.WriteLine("Mask0x28: " + string.Join(", ", masks.Select(p => $"{p.Key}:{p.Value}")));
    }
}
