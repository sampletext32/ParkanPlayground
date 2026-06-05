using System.Numerics;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportCamera
{
    public float YawDegrees { get; set; } = 35.0f;
    public float PitchDegrees { get; set; } = 25.0f;
    public float Distance { get; set; } = 4.0f;

    public Matrix4x4 GetSceneRotationMatrix()
    {
        return Matrix4x4.CreateRotationY(ToRadians(YawDegrees)) *
               Matrix4x4.CreateRotationX(ToRadians(PitchDegrees));
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(
            new Vector3(0.0f, 0.0f, Distance),
            Vector3.Zero,
            Vector3.UnitY);
    }

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            ToRadians(60.0f),
            aspectRatio,
            0.01f,
            100.0f);
    }

    public static float ToRadians(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }
}
