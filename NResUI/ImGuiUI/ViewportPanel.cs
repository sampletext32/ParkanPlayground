using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Rendering.Viewport;
using NResUI.Rendering.Viewport.Meshes;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.ImGuiUI;

public sealed class ViewportPanel : IImGuiPanel
{
    private readonly ViewportRenderer _renderer;
    private readonly ViewportScene _scene;
    private readonly ViewportCamera _camera = new();
    private readonly ViewportInputController _inputController = new();

    public ViewportPanel(GL gl, IWindow window)
    {
        var cubeMesh = PrimitiveMeshes.CreateCube(gl);
        var gridMesh = PrimitiveMeshes.CreateWorldGrid(gl);

        _scene = ViewportScene.CreateDefaultCubeScene(cubeMesh, gridMesh);
        _renderer = new ViewportRenderer(gl, window);
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Viewport"))
        {
            ImGui.End();
            return;
        }

        DrawSelectionStatus();
        DrawGridToggle();

        var imageSize = ImGui.GetContentRegionAvail();
        if (imageSize.X < 32 || imageSize.Y < 32)
        {
            ImGui.TextDisabled("Viewport is too small.");
            ImGui.End();
            return;
        }

        var textureId = _renderer.Render(
            width: Math.Max(1, (int)imageSize.X),
            height: Math.Max(1, (int)imageSize.Y),
            scene: _scene,
            camera: _camera);

        ImGui.Image(
            (IntPtr)textureId,
            imageSize,
            new Vector2(0, 1),
            new Vector2(1, 0));

        var viewportHovered = ImGui.IsItemHovered();
        var imageMin = ImGui.GetItemRectMin();

        DrawViewportHelpOverlay();

        _inputController.Handle(
            _scene,
            _camera,
            viewportHovered,
            imageMin,
            imageSize);

        ImGui.End();
    }

    private void DrawSelectionStatus()
    {
        var selectedPiece = _scene.SelectedPiece;
        if (selectedPiece != null)
            ImGui.Text($"Selected: {selectedPiece.Name}");
        else
            ImGui.TextDisabled("Selected: none");
    }

    private void DrawGridToggle()
    {
        var grid = _scene.Grid;
        if (grid == null)
            return;

        var isVisible = grid.IsVisible;
        if (ImGui.Checkbox("Show grid", ref isVisible))
            grid.IsVisible = isVisible;
    }

    private static void DrawViewportHelpOverlay()
    {
        var drawList = ImGui.GetWindowDrawList();

        var imageMin = ImGui.GetItemRectMin();
        var imageMax = ImGui.GetItemRectMax();

        const string helpText = "LMB click: select / deselect\nMMB drag: rotate\nMouse wheel: zoom";

        var textSize = ImGui.CalcTextSize(helpText);
        var padding = new Vector2(8.0f, 6.0f);

        var boxMin = new Vector2(
            imageMin.X + 8.0f,
            imageMax.Y - textSize.Y - padding.Y * 2.0f - 8.0f);

        var boxMax = new Vector2(
            boxMin.X + textSize.X + padding.X * 2.0f,
            boxMin.Y + textSize.Y + padding.Y * 2.0f);

        drawList.AddRectFilled(
            boxMin,
            boxMax,
            ImGui.GetColorU32(new Vector4(0.0f, 0.0f, 0.0f, 0.45f)),
            4.0f);

        drawList.AddText(
            boxMin + padding,
            ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, 0.9f)),
            helpText);
    }
}
