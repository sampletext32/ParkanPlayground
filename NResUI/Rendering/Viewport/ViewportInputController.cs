using System.Numerics;
using ImGuiNET;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportInputController
{
    private const float MouseRotationSpeed = 0.25f;
    private const float MouseZoomSpeed = 0.35f;

    private bool _isRotatingViewport;

    public void Handle(
        ViewportScene scene,
        ViewportCamera camera,
        bool viewportHovered,
        Vector2 imageMin,
        Vector2 imageSize)
    {
        var io = ImGui.GetIO();

        if (viewportHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
            _isRotatingViewport = true;

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Middle))
            _isRotatingViewport = false;

        if (_isRotatingViewport)
        {
            camera.YawDegrees += io.MouseDelta.X * MouseRotationSpeed;
            camera.PitchDegrees += io.MouseDelta.Y * MouseRotationSpeed;
            camera.PitchDegrees = Math.Clamp(camera.PitchDegrees, -89.0f, 89.0f);
        }

        if (viewportHovered && Math.Abs(io.MouseWheel) > float.Epsilon)
        {
            camera.Distance -= io.MouseWheel * MouseZoomSpeed;
            camera.Distance = Math.Clamp(camera.Distance, 1.5f, 40.0f);
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
    }
}
