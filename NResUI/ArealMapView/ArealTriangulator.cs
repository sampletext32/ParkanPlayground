using ArealMapLib;

namespace NResUI.ArealMapView;

/// <summary>
/// Разбиение многоугольника ареала на треугольники (метод отсечения «ушей») в плоскости XY для заливки в ImGui.
/// Около 30% ареалов в картах игры не выпуклые, поэтому простой веер вершин не подходит.
/// </summary>
public static class ArealTriangulator
{
    private const float Epsilon = 1e-4f;

    /// <summary>Возвращает тройки индексов вершин (по три подряд на треугольник).</summary>
    /// <param name="areal">Ареал; используются только X и Y вершин.</param>
    public static int[] Triangulate(ArealRecord areal)
    {
        var v = areal.Vertices;
        var n = v.Length;
        var indices = new List<int>(n);
        for (var i = 0; i < n; i++)
        {
            indices.Add(i);
        }

        // ориентация: ушко проверяется для обхода против часовой стрелки
        double area2 = 0;
        for (var i = 0; i < n; i++)
        {
            var a = v[i];
            var b = v[(i + 1) % n];
            area2 += (double) a.X * b.Y - (double) b.X * a.Y;
        }

        if (area2 < 0)
        {
            indices.Reverse();
        }

        var result = new List<int>((n - 2) * 3);
        var guard = n * n + 8;
        while (indices.Count > 3 && guard-- > 0)
        {
            var count = indices.Count;
            var clipped = false;
            for (var i = 0; i < count; i++)
            {
                var ip = indices[(i + count - 1) % count];
                var ic = indices[i];
                var inx = indices[(i + 1) % count];
                var cross = Cross(v[ip], v[ic], v[inx]);

                if (Math.Abs(cross) <= Epsilon)
                {
                    // вырожденная (лежит на прямой) вершина - отбрасываем без треугольника
                    indices.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (cross < 0)
                {
                    continue; // вогнутая вершина
                }

                var empty = true;
                for (var k = 0; k < count && empty; k++)
                {
                    var ik = indices[k];
                    if (ik == ip || ik == ic || ik == inx)
                    {
                        continue;
                    }

                    // вершина совпадает по координатам с вершиной ушка (замкнутые «перешейки») - не мешает
                    if (SamePoint(v[ik], v[ip]) || SamePoint(v[ik], v[ic]) || SamePoint(v[ik], v[inx]))
                    {
                        continue;
                    }

                    if (InsideTriangle(v[ip], v[ic], v[inx], v[ik]))
                    {
                        empty = false;
                    }
                }

                if (!empty)
                {
                    continue;
                }

                result.Add(ip);
                result.Add(ic);
                result.Add(inx);
                indices.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped)
            {
                break;
            }
        }

        // остаток (треугольник или не поддавшийся многоугольник) - веером
        for (var k = 1; k + 1 < indices.Count; k++)
        {
            result.Add(indices[0]);
            result.Add(indices[k]);
            result.Add(indices[k + 1]);
        }

        return result.ToArray();
    }

    private static float Cross(Common.Vector3 a, Common.Vector3 b, Common.Vector3 c) =>
        (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);

    private static bool InsideTriangle(Common.Vector3 a, Common.Vector3 b, Common.Vector3 c, Common.Vector3 p)
    {
        var d1 = Cross(a, b, p);
        var d2 = Cross(b, c, p);
        var d3 = Cross(c, a, p);
        // граница включена: вершина на стороне ушка тоже делает его недопустимым
        return d1 >= -Epsilon && d2 >= -Epsilon && d3 >= -Epsilon;
    }

    private static bool SamePoint(Common.Vector3 a, Common.Vector3 b) =>
        Math.Abs(a.X - b.X) <= Epsilon && Math.Abs(a.Y - b.Y) <= Epsilon;
}
