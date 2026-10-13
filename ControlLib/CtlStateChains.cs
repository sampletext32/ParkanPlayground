namespace ControlLib;

/// <summary>
/// Строит цепочки состояний по матрице весов рёбер CTL. Матрица хранится построчно: <c>EdgeWeightMatrix[to * n + from]</c> - вес ребра <c>from -&gt; to</c>
/// (<c>ControlStateGraph::BuildWeightedAdjacencyMatrix</c>, Control.dll 0x100017D0; стоимость для Дейкстры = (разница скоростей + 1) * вес).
/// Значения в файле: 0 - принудительное продолжение (следующее состояние цепочки), 1 - разрешённый переход между цепочками, 1e6 - перехода нет.
/// </summary>
public static class CtlStateChains
{
    private const float NoEdge = 1e5f;
    private const int MaxChainLength = 256;

    /// <summary>Строит цепочки из двух и более состояний.</summary>
    /// <param name="ctl">Файл CTL.</param>
    /// <param name="joinThroughTransitions">
    /// false - цепочка идёт только по рёбрам веса 0 (одна фаза движения); true - после её конца продолжается по разрешённому переходу (вес 1) в состояние,
    /// которое начинает отрезок там же, где закончился предыдущий, в том же направлении (несколько фаз подряд: левый + правый шаг).
    /// </param>
    public static IReadOnlyList<CtlStateChain> Build(CtlFile ctl, bool joinThroughTransitions)
    {
        var n = ctl.StateRecords.Count;
        var matrix = ctl.EdgeWeightMatrix;
        if (n == 0 || matrix.Count != n * n)
            return [];

        var animations = ctl.StateRecords.Select(s => s.Animation).ToArray();

        // Принудительные продолжения: forced[from] = to.
        var forced = new int[n];
        var hasForcedPredecessor = new bool[n];
        Array.Fill(forced, -1);
        for (var to = 0; to < n; to++)
        {
            for (var from = 0; from < n; from++)
            {
                if (from != to && matrix[to * n + from] == 0.0f && forced[from] < 0)
                {
                    forced[from] = to;
                    hasForcedPredecessor[to] = true;
                }
            }
        }

        var phases = new List<List<int>>();
        for (var first = 0; first < n; first++)
        {
            if (hasForcedPredecessor[first] || forced[first] < 0)
                continue;

            var phase = new List<int> { first };
            var visited = new HashSet<int> { first };
            for (var next = forced[first]; next >= 0 && visited.Add(next); next = forced[next])
                phase.Add(next);

            phases.Add(phase);
        }

        if (!joinThroughTransitions)
        {
            return phases
                .Select(p => new CtlStateChain(p, GetStart(animations[p[0]]), GetEnd(animations[p[^1]])))
                .ToList();
        }

        // Склейка: фаза продолжается в другую фазу, если есть разрешённый переход и отрезки стыкуются во времени в том же направлении.
        var phaseByFirst = phases.ToDictionary(p => p[0]);
        var joinedInto = new HashSet<int>();
        var result = new List<List<int>>();

        foreach (var start in phases)
        {
            var chain = new List<int>(start);
            var usedPhases = new HashSet<int> { start[0] };

            while (chain.Count < MaxChainLength)
            {
                var last = chain[^1];
                var continuation = phases
                    .Where(p => !usedPhases.Contains(p[0]) && matrix[p[0] * n + last] < NoEdge)
                    .FirstOrDefault(p => Continues(animations[last], animations[p[0]]));

                if (continuation == null)
                    break;

                usedPhases.Add(continuation[0]);
                chain.AddRange(continuation);
            }

            if (chain.Count > start.Count)
            {
                joinedInto.UnionWith(usedPhases.Where(f => f != start[0]));
                result.Add(chain);
            }
        }

        // Фазы, не вошедшие ни в одну склейку, остаются как есть; склейки сохраняются целиком.
        foreach (var phase in phases)
        {
            if (!result.Any(c => c.Contains(phase[0])))
                result.Add(phase);
        }

        return result
            .OrderBy(c => c[0])
            .Select(c => new CtlStateChain(c, GetStart(animations[c[0]]), GetEnd(animations[c[^1]])))
            .ToList();
    }

    // next продолжает prev: отрезок начинается там, где закончился предыдущий, направление то же.
    private static bool Continues(CtlStateAnimation prev, CtlStateAnimation next)
    {
        var direction = Math.Sign(GetEnd(prev) - GetStart(prev));
        return GetStart(next) == GetEnd(prev) &&
               Math.Sign(GetEnd(next) - GetStart(next)) == direction;
    }

    // Основной диапазон состояния - второй, если он задан, иначе первый.
    private static float GetStart(CtlStateAnimation a) => a.Anim2Start >= 0.0f ? a.Anim2Start : a.Anim1Start;

    private static float GetEnd(CtlStateAnimation a) => a.Anim2Start >= 0.0f ? a.Anim2End : a.Anim1End;
}
