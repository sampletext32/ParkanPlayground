using System.Numerics;
using ArealMapLib;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;

namespace NResUI.ImGuiUI;

/// <summary>
/// Панель «Areal Map Explorer»: рисует многоугольники ареалов карты (<c>Land.map</c>), подсвечивает выбранный ареал и его соседей,
/// показывает поля записи при наведении и выборе.
/// </summary>
public class ArealMapExplorer : IImGuiPanel
{
    private static readonly string[] ColorModes = ["По Mask0x28", "По Flag0x20", "По наклону (nz)", "Один цвет"];

    private readonly ArealMapViewModel _vm;

    public ArealMapExplorer(ArealMapViewModel viewModel)
    {
        _vm = viewModel;
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Areal Map Explorer"))
        {
            ImGui.End();
            return;
        }

        ImGui.Text("Land.map - карта ареалов (многоугольники проходимости). File -> Open Areal Map (Land.map)");
        ImGui.Separator();

        var map = _vm.Map;
        if (!_vm.HasFile || map is null)
        {
            ImGui.Text("Карта не открыта");
            ImGui.End();
            return;
        }

        ImGui.Text($"{_vm.Path}");
        ImGui.Text($"Ареалов: {map.Areals.Count}, сетка {map.GridWidth} x {map.GridHeight}, " +
                   $"границы: x {_vm.MapBounds.X:0.#}..{_vm.MapBounds.Z:0.#}, y {_vm.MapBounds.Y:0.#}..{_vm.MapBounds.W:0.#}");

        var colorMode = _vm.ColorMode;
        ImGui.SetNextItemWidth(180);
        if (ImGui.Combo("Раскраска", ref colorMode, ColorModes, ColorModes.Length))
        {
            _vm.ColorMode = colorMode;
        }

        ImGui.SameLine();
        var borders = _vm.ShowBorders;
        if (ImGui.Checkbox("Границы карты", ref borders))
        {
            _vm.ShowBorders = borders;
        }

        ImGui.SameLine();
        var neighbourEdges = _vm.ShowNeighbourEdges;
        if (ImGui.Checkbox("Рёбра между ареалами", ref neighbourEdges))
        {
            _vm.ShowNeighbourEdges = neighbourEdges;
        }

        ImGui.SameLine();
        var showNeighbours = _vm.ShowSelectedNeighbours;
        if (ImGui.Checkbox("Соседи выбранного", ref showNeighbours))
        {
            _vm.ShowSelectedNeighbours = showNeighbours;
        }

        ImGui.SameLine();
        if (ImGui.Button("Вписать"))
        {
            _vm.FitRequested = true;
        }

        var avail = ImGui.GetContentRegionAvail();
        var canvasSize = new Vector2(Math.Max(100, avail.X), Math.Max(100, avail.Y - 150));
        DrawCanvas(map, canvasSize);
        if (ImGui.BeginChild("areal_info", Vector2.Zero))
        {
            DrawInfo(map);
        }

        ImGui.EndChild();

        ImGui.End();
    }

    private void DrawCanvas(ArealMapFile map, Vector2 size)
    {
        ImGui.InvisibleButton("areal_canvas", size, ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
        var hovered = ImGui.IsItemHovered();
        var dl = ImGui.GetWindowDrawList();
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var center = (min + max) * 0.5f;

        if (_vm.FitRequested)
        {
            var b = _vm.MapBounds;
            var w = Math.Max(1f, b.Z - b.X);
            var h = Math.Max(1f, b.W - b.Y);
            _vm.Zoom = Math.Min(size.X / w, size.Y / h) * 0.95f;
            _vm.ViewCenter = new Vector2((b.X + b.Z) * 0.5f, (b.Y + b.W) * 0.5f);
            _vm.FitRequested = false;
        }

        var io = ImGui.GetIO();
        var zoom = _vm.Zoom;
        var viewCenter = _vm.ViewCenter;

        Vector2 ToScreen(float x, float y) => center + new Vector2((x - viewCenter.X) * zoom, -(y - viewCenter.Y) * zoom);

        Vector2 ToWorld(Vector2 screen) => new(
            (screen.X - center.X) / zoom + viewCenter.X,
            -(screen.Y - center.Y) / zoom + viewCenter.Y);

        // масштаб колесом вокруг курсора, перемещение перетаскиванием
        if (hovered && Math.Abs(io.MouseWheel) > 0.001f)
        {
            var before = ToWorld(io.MousePos);
            zoom = Math.Clamp(zoom * MathF.Pow(1.15f, io.MouseWheel), 0.01f, 200f);
            _vm.Zoom = zoom;
            var after = new Vector2(
                (io.MousePos.X - center.X) / zoom + viewCenter.X,
                -(io.MousePos.Y - center.Y) / zoom + viewCenter.Y);
            viewCenter += before - after;
            _vm.ViewCenter = viewCenter;
        }

        var dragging = ImGui.IsItemActive() &&
                       (ImGui.IsMouseDragging(ImGuiMouseButton.Left, 4f) || ImGui.IsMouseDragging(ImGuiMouseButton.Right, 1f));
        if (dragging)
        {
            var delta = io.MouseDelta;
            viewCenter += new Vector2(-delta.X / zoom, delta.Y / zoom);
            _vm.ViewCenter = viewCenter;
        }

        dl.PushClipRect(min, max, true);
        dl.AddRectFilled(min, max, ImGui.GetColorU32(new Vector4(0.12f, 0.13f, 0.16f, 1f)));

        // наведённый ареал
        var hoveredIndex = -1;
        if (hovered)
        {
            var world = ToWorld(io.MousePos);
            for (var i = map.Areals.Count - 1; i >= 0; i--)
            {
                var bnd = _vm.Bounds[i];
                if (world.X < bnd.X || world.X > bnd.Z || world.Y < bnd.Y || world.Y > bnd.W)
                {
                    continue;
                }

                if (PointInPolygon(map.Areals[i], world))
                {
                    hoveredIndex = i;
                    break;
                }
            }
        }

        var selected = _vm.Selected;
        var neighbourSet = new HashSet<int>();
        if (selected >= 0 && selected < map.Areals.Count && _vm.ShowSelectedNeighbours)
        {
            foreach (var e in map.Areals[selected].Edges)
            {
                if (e.HasNeighbour)
                {
                    neighbourSet.Add(e.NeighbourIndex);
                }
            }
        }

        for (var i = 0; i < map.Areals.Count; i++)
        {
            var areal = map.Areals[i];
            var bnd = _vm.Bounds[i];
            var topLeft = ToScreen(bnd.X, bnd.W);
            var bottomRight = ToScreen(bnd.Z, bnd.Y);
            if (bottomRight.X < min.X || topLeft.X > max.X || bottomRight.Y < min.Y || topLeft.Y > max.Y)
            {
                continue;
            }

            var color = FillColor(areal);
            if (neighbourSet.Contains(i))
            {
                color = new Vector4(0.95f, 0.85f, 0.2f, 0.85f);
            }

            if (i == selected)
            {
                color = new Vector4(1f, 1f, 1f, 0.95f);
            }
            else if (i == hoveredIndex)
            {
                color = Vector4.Min(color * 1.4f, Vector4.One);
            }

            var packed = ImGui.GetColorU32(color);
            var tri = _vm.Triangles[i];
            for (var t = 0; t + 2 < tri.Length; t += 3)
            {
                var a = areal.Vertices[tri[t]];
                var b2 = areal.Vertices[tri[t + 1]];
                var c = areal.Vertices[tri[t + 2]];
                dl.AddTriangleFilled(ToScreen(a.X, a.Y), ToScreen(b2.X, b2.Y), ToScreen(c.X, c.Y), packed);
            }

            // рёбра
            var borderColor = ImGui.GetColorU32(new Vector4(1f, 0.25f, 0.2f, 1f));
            var edgeColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.45f));
            var v = areal.Vertices;
            for (var e = 0; e < v.Length; e++)
            {
                var edge = areal.Edges[e];
                var p0 = ToScreen(v[e].X, v[e].Y);
                var p1 = ToScreen(v[(e + 1) % v.Length].X, v[(e + 1) % v.Length].Y);
                if (!edge.HasNeighbour)
                {
                    if (_vm.ShowBorders)
                    {
                        dl.AddLine(p0, p1, borderColor, 2f);
                    }
                }
                else if (_vm.ShowNeighbourEdges || i == selected)
                {
                    dl.AddLine(p0, p1, edgeColor, i == selected ? 2f : 1f);
                }
            }
        }

        dl.PopClipRect();

        if (hovered && ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !ImGui.IsMouseDragging(ImGuiMouseButton.Left, 4f))
        {
            _vm.Selected = hoveredIndex;
        }

        if (hovered && hoveredIndex >= 0)
        {
            DrawTooltip(map, hoveredIndex);
        }

    }

    private Vector4 FillColor(ArealRecord areal)
    {
        switch (_vm.ColorMode)
        {
            case 0:
                return Hue(areal.Mask0x28 * 0.61803f, 0.55f, 0.75f);
            case 1:
                return areal.Flag0x20 != 0 ? new Vector4(0.9f, 0.5f, 0.2f, 0.8f) : new Vector4(0.35f, 0.55f, 0.75f, 0.8f);
            case 2:
                // чем больше наклон, тем краснее: nz 1.0 - зелёный, 0.3 - красный
                var t = Math.Clamp((areal.Normal.Z - 0.3f) / 0.7f, 0f, 1f);
                return new Vector4(1f - t, 0.3f + 0.6f * t, 0.2f, 0.8f);
            default:
                return new Vector4(0.4f, 0.6f, 0.5f, 0.8f);
        }
    }

    private static Vector4 Hue(float h, float s, float v)
    {
        h -= MathF.Floor(h);
        ImGui.ColorConvertHSVtoRGB(h, s, v, out var r, out var g, out var b);
        return new Vector4(r, g, b, 0.8f);
    }

    private static bool PointInPolygon(ArealRecord areal, Vector2 p)
    {
        var inside = false;
        var v = areal.Vertices;
        for (int i = 0, j = v.Length - 1; i < v.Length; j = i++)
        {
            if ((v[i].Y > p.Y) != (v[j].Y > p.Y) &&
                p.X < (v[j].X - v[i].X) * (p.Y - v[i].Y) / (v[j].Y - v[i].Y) + v[i].X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void DrawTooltip(ArealMapFile map, int index)
    {
        var a = map.Areals[index];
        ImGui.BeginTooltip();
        ImGui.Text($"Ареал {index}");
        ImGui.Text($"Вершин: {a.Vertices.Length}, площадь {a.Area:0.#}");
        ImGui.Text($"Mask0x28 = {a.Mask0x28}, Flag0x20 = {a.Flag0x20}");
        ImGui.Text($"Центр ({a.Center.X:0.#}, {a.Center.Y:0.#}, {a.Center.Z:0.#}), nz {a.Normal.Z:0.00}");
        var neighbours = a.Edges.Count(e => e.HasNeighbour);
        ImGui.Text($"Соседей: {neighbours}, рёбер границы: {a.Edges.Length - neighbours}");
        ImGui.EndTooltip();
    }

    private void DrawInfo(ArealMapFile map)
    {
        var selected = _vm.Selected;
        if (selected < 0 || selected >= map.Areals.Count)
        {
            ImGui.Text("Выберите ареал щелчком. Колесо - масштаб, перетаскивание - перемещение.");
            return;
        }

        var a = map.Areals[selected];
        ImGui.Text($"Выбран ареал {selected}: вершин {a.Vertices.Length}, площадь {a.Area:0.##}, " +
                   $"Mask0x28 = {a.Mask0x28}, Flag0x20 = {a.Flag0x20}");
        ImGui.Text($"Центр ({a.Center.X:0.##}, {a.Center.Y:0.##}, {a.Center.Z:0.##}), нормаль ({a.Normal.X:0.00}, {a.Normal.Y:0.00}, {a.Normal.Z:0.00})");
        ImGui.Text("Соседи по рёбрам (ребро i - от вершины i к i+1):");
        for (var e = 0; e < a.Edges.Length; e++)
        {
            var edge = a.Edges[e];
            if (!edge.HasNeighbour)
            {
                ImGui.Text($"ребро {e}: граница карты");
                continue;
            }

            ImGui.Text($"ребро {e}: ареал");
            ImGui.SameLine();
            if (ImGui.SmallButton($"{edge.NeighbourIndex}##n{e}"))
            {
                _vm.Selected = edge.NeighbourIndex;
            }

            ImGui.SameLine();
            ImGui.Text($"(его ребро {edge.NeighbourEdgeIndex})");
        }
    }
}
