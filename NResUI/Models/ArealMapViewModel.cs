using System.Numerics;
using ArealMapLib;
using NResUI.ArealMapView;

namespace NResUI.Models;

/// <summary>Состояние панели карты ареалов: загруженная карта, треугольники для заливки и настройки вида.</summary>
public class ArealMapViewModel
{
    public bool HasFile { get; private set; }
    public string? Path { get; private set; }
    public ArealMapFile? Map { get; private set; }

    /// <summary>Треугольники каждого ареала (тройки индексов его вершин).</summary>
    public int[][] Triangles { get; private set; } = [];

    /// <summary>Границы ареала в плоскости XY: (minX, minY, maxX, maxY).</summary>
    public Vector4[] Bounds { get; private set; } = [];

    /// <summary>Границы всей карты: (minX, minY, maxX, maxY).</summary>
    public Vector4 MapBounds { get; private set; }

    /// <summary>Сколько ареалов с каждым значением Mask0x28 (по возрастанию значения).</summary>
    public SortedDictionary<int, int> MaskCounts { get; private set; } = new();

    /// <summary>Выбранный ареал, -1 - нет.</summary>
    public int Selected { get; set; } = -1;

    /// <summary>Центр вида в координатах карты.</summary>
    public Vector2 ViewCenter { get; set; }

    /// <summary>Масштаб: пикселей на единицу карты.</summary>
    public float Zoom { get; set; } = 0.2f;

    /// <summary>Раскраска: 0 - по Mask0x28, 1 - по проходимости (Flag0x20), 2 - по наклону (nz), 3 - одним цветом.</summary>
    public int ColorMode { get; set; }

    public bool ShowBorders { get; set; } = true;
    public bool ShowNeighbourEdges { get; set; }
    public bool ShowSelectedNeighbours { get; set; } = true;

    /// <summary>Рисовать сетку ячеек (приближённо: размер ячейки - протяжённость карты / ширина сетки).</summary>
    public bool ShowGrid { get; set; }

    /// <summary>Нужно вписать карту в окно при следующей отрисовке.</summary>
    public bool FitRequested { get; set; } = true;

    /// <summary>Загружает карту и готовит данные для отрисовки.</summary>
    /// <param name="map">Разобранная карта ареалов.</param>
    /// <param name="path">Путь к файлу или описание источника.</param>
    public void SetParseResult(ArealMapFile map, string path)
    {
        Map = map;
        Path = path;
        HasFile = true;
        Selected = -1;
        FitRequested = true;

        MaskCounts = new SortedDictionary<int, int>();
        foreach (var areal in map.Areals)
        {
            MaskCounts[areal.Mask0x28] = MaskCounts.GetValueOrDefault(areal.Mask0x28) + 1;
        }

        Triangles = new int[map.Areals.Count][];
        Bounds = new Vector4[map.Areals.Count];
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        for (var i = 0; i < map.Areals.Count; i++)
        {
            var areal = map.Areals[i];
            Triangles[i] = ArealTriangulator.Triangulate(areal);

            var bx0 = float.MaxValue;
            var by0 = float.MaxValue;
            var bx1 = float.MinValue;
            var by1 = float.MinValue;
            foreach (var v in areal.Vertices)
            {
                bx0 = Math.Min(bx0, v.X);
                by0 = Math.Min(by0, v.Y);
                bx1 = Math.Max(bx1, v.X);
                by1 = Math.Max(by1, v.Y);
            }

            Bounds[i] = new Vector4(bx0, by0, bx1, by1);
            minX = Math.Min(minX, bx0);
            minY = Math.Min(minY, by0);
            maxX = Math.Max(maxX, bx1);
            maxY = Math.Max(maxY, by1);
        }

        MapBounds = new Vector4(minX, minY, maxX, maxY);
    }
}
