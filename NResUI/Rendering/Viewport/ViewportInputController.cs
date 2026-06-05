using System.Numerics;
using ImGuiNET;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportInputController
{
    private const float MouseRotationSpeed = 0.25f;
    private const float MouseZoomSpeed = 0.35f;

    private bool _isRotatingViewport;
    private bool _isPanningViewport;

    public void Handle(
        ViewportScene scene,
        ViewportCamera camera,
        bool viewportHovered,
        bool viewportFocused,
        Vector2 imageMin,
        Vector2 imageSize)
    {
        var io = ImGui.GetIO();

        var wantsPanWithMiddle = io.KeyAlt && ImGui.IsMouseClicked(ImGuiMouseButton.Middle);
        var wantsPanWithRight = ImGui.IsMouseClicked(ImGuiMouseButton.Right);

        if (viewportHovered && wantsPanWithMiddle)
            _isPanningViewport = true;
        else if (viewportHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            _isRotatingViewport = true;

        if (viewportHovered && wantsPanWithRight)
            _isPanningViewport = true;

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Middle))
            _isRotatingViewport = false;

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Right) && !(io.KeyAlt && ImGui.IsMouseDown(ImGuiMouseButton.Middle)))
            _isPanningViewport = false;

        if (_isRotatingViewport)
        {
            camera.YawDegrees += -io.MouseDelta.X * MouseRotationSpeed;
            camera.PitchDegrees += io.MouseDelta.Y * MouseRotationSpeed;
            camera.PitchDegrees = Math.Clamp(camera.PitchDegrees, -89.0f, 89.0f);
        }

        if (_isPanningViewport)
            camera.Pan(io.MouseDelta, imageSize);

        if (viewportHovered && Math.Abs(io.MouseWheel) > float.Epsilon)
        {
            camera.Distance -= io.MouseWheel * MouseZoomSpeed * MathF.Max(1.0f, camera.Distance * 0.15f);
            camera.Distance = Math.Clamp(camera.Distance, 0.25f, 400.0f);
        }

        if (viewportHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            var mouse = ImGui.GetMousePos();
            var localMouse = mouse - imageMin;

            scene.SelectedPieceId = ViewportSelection.PickPiece(
                scene,
                camera,
                localMouse,
                imageSize);
        }

        if (viewportFocused || viewportHovered)
            HandleKeyboardShortcuts(scene, camera);
    }

    private static void HandleKeyboardShortcuts(ViewportScene scene, ViewportCamera camera)
    {
        if (ImGui.IsKeyPressed(ImGuiKey.F))
        {
            if (scene.TryGetSelectedPieceWorldBounds(out var selectedBounds))
                camera.FrameBounds(selectedBounds.Min, selectedBounds.Max);
            else if (scene.TryGetSceneWorldBounds(out var sceneBounds))
                camera.FrameBounds(sceneBounds.Min, sceneBounds.Max);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Home))
        {
            if (scene.TryGetSceneWorldBounds(out var sceneBounds))
                camera.FrameBounds(sceneBounds.Min, sceneBounds.Max);
            else
                camera.Reset();
        }
    }
}
