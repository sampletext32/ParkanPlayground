using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Rendering.Viewport;
using NativeFileDialogSharp;
using NResUI.Rendering.Viewport.Meshes;
using NResUI.Rendering.Viewport.Msh;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.ImGuiUI;

public sealed class ViewportPanel : IImGuiPanel
{
    private readonly ViewportRenderer _renderer;
    private readonly ViewportScene _scene;
    private readonly ViewportCamera _camera = new();
    private readonly ViewportInputController _inputController = new();
    private readonly IConfigProvider _configProvider;

    private string? _loadedModelPath;
    private string? _loadError;

    public ViewportPanel(GL gl, IWindow window, IConfigProvider configProvider)
    {
        _configProvider = configProvider;
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

        DrawModelControls();
        DrawSelectionStatus();
        DrawViewportToolbar();
        DrawDebugControls();

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
        var viewportFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
        var imageMin = ImGui.GetItemRectMin();

        DrawViewportHelpOverlay();

        _inputController.Handle(
            _scene,
            _camera,
            viewportHovered,
            viewportFocused,
            imageMin,
            imageSize);

        ImGui.End();
    }


    private void DrawModelControls()
    {
        if (ImGui.Button("Open MSH in viewport"))
        {
            var result = Dialog.FileOpen("msh");
            if (result.IsOk)
                LoadMsh(result.Path);
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset cube"))
        {
            var cubeMesh = PrimitiveMeshes.CreateCube(_renderer.Gl);
            _scene.ReplacePieces([ViewportPiece.CreateUnitCube(0, "Cube", cubeMesh)]);
            _loadedModelPath = null;
            _loadError = null;
            _camera.Reset();
        }

        if (_loadedModelPath != null)
            ImGui.TextDisabled($"Model: {Path.GetFileName(_loadedModelPath)}");

        if (_loadError != null)
            ImGui.TextColored(new Vector4(1.0f, 0.35f, 0.25f, 1.0f), $"MSH load failed: {_loadError}");
    }

    private void LoadMsh(string path)
    {
        var loadResult = MshViewportLoader.LoadFromFile(_renderer.Gl, path, _configProvider);
        if (!loadResult.IsSuccess)
        {
            _loadError = loadResult.Error ?? "Unknown error.";
            return;
        }

        _scene.ReplacePieces(loadResult.Pieces);
        _loadedModelPath = loadResult.SourcePath;
        _loadError = null;

        if (_scene.TryGetSceneWorldBounds(out var bounds))
            _camera.FrameBounds(bounds.Min, bounds.Max);
        else
            _camera.Reset();
    }

    private void DrawSelectionStatus()
    {
        var selectedPiece = _scene.SelectedPiece;
        if (selectedPiece != null)
        {
            ImGui.Text($"Selected: {selectedPiece.Name}");

            var debugInfo = selectedPiece.DebugInfo;
            if (debugInfo != null)
            {
                ImGui.TextDisabled(
                    $"{debugInfo.SourceKind} | parent {debugInfo.SourceParentIndex} | slot {debugInfo.GeometrySlotIndex} | " +
                    $"flags 0x{debugInfo.Msh01Flags:X4} | batches {debugInfo.BatchCount} | tris {debugInfo.TriangleCount}");
            }
        }
        else
        {
            ImGui.TextDisabled("Selected: none");
        }
    }

    private void DrawViewportToolbar()
    {
        if (ImGui.Button("Reset view"))
            _camera.Reset();

        ImGui.SameLine();

        if (ImGui.Button("Frame selected"))
        {
            if (_scene.TryGetSelectedPieceWorldBounds(out var selectedBounds))
                _camera.FrameBounds(selectedBounds.Min, selectedBounds.Max);
        }

        ImGui.SameLine();

        if (ImGui.Button("Frame all"))
        {
            if (_scene.TryGetSceneWorldBounds(out var sceneBounds))
                _camera.FrameBounds(sceneBounds.Min, sceneBounds.Max);
            else
                _camera.Reset();
        }
    }

    private void DrawDebugControls()
    {
        var grid = _scene.Grid;
        if (grid != null)
        {
            var isVisible = grid.IsVisible;
            if (ImGui.Checkbox("Grid", ref isVisible))
                grid.IsVisible = isVisible;
        }

        ImGui.SameLine();

        var showOriginAxes = _scene.Debug.ShowOriginAxes;
        if (ImGui.Checkbox("Origin axes", ref showOriginAxes))
            _scene.Debug.ShowOriginAxes = showOriginAxes;

        ImGui.SameLine();

        var showPieceOrigins = _scene.Debug.ShowPieceOrigins;
        if (ImGui.Checkbox("Piece axes", ref showPieceOrigins))
            _scene.Debug.ShowPieceOrigins = showPieceOrigins;

        ImGui.SameLine();

        var showSelectedBounds = _scene.Debug.ShowSelectedBounds;
        if (ImGui.Checkbox("Selected bounds", ref showSelectedBounds))
            _scene.Debug.ShowSelectedBounds = showSelectedBounds;

        ImGui.SameLine();

        var showSceneBounds = _scene.Debug.ShowSceneBounds;
        if (ImGui.Checkbox("Scene bounds", ref showSceneBounds))
            _scene.Debug.ShowSceneBounds = showSceneBounds;

        ImGui.SameLine();

        var wireframe = _scene.Debug.Wireframe;
        if (ImGui.Checkbox("Wireframe", ref wireframe))
            _scene.Debug.Wireframe = wireframe;
    }

    private static void DrawViewportHelpOverlay()
    {
        var drawList = ImGui.GetWindowDrawList();

        var imageMin = ImGui.GetItemRectMin();
        var imageMax = ImGui.GetItemRectMax();

        const string helpText =
            "LMB click: select / deselect\n" +
            "MMB drag: rotate\n" +
            "RMB drag or Alt+MMB: pan\n" +
            "Mouse wheel: zoom\n" +
            "F: frame selected   Home: frame all";

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
