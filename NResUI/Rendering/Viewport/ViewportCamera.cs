using System.Numerics;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportCamera
{
    public const float FieldOfViewDegrees = 60.0f;

    public float YawDegrees { get; set; } = 35.0f;
    public float PitchDegrees { get; set; } = 25.0f;
    public float Distance { get; set; } = 4.0f;
    public Vector3 Target { get; set; } = Vector3.Zero;

    public Matrix4x4 GetSceneRotationMatrix()
    {
        // Kept as an explicit hook for future scene-wide transforms.
        // Camera orbit is handled by GetViewMatrix().
        return Matrix4x4.Identity;
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(GetEyePosition(), Target, Vector3.UnitY);
    }

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            ToRadians(FieldOfViewDegrees),
            aspectRatio,
            0.01f,
            1000.0f);
    }

    public Vector3 GetEyePosition()
    {
        return Target + GetOrbitDirection() * Distance;
    }

    public Vector3 GetForwardDirection()
    {
        return Vector3.Normalize(Target - GetEyePosition());
    }

    public Vector3 GetRightDirection()
    {
        var forward = GetForwardDirection();
        var right = Vector3.Cross(forward, Vector3.UnitY);

        if (right.LengthSquared() < 1e-8f)
            return Vector3.UnitX;

        return Vector3.Normalize(right);
    }

    public Vector3 GetUpDirection()
    {
        var right = GetRightDirection();
        var forward = GetForwardDirection();
        return Vector3.Normalize(Vector3.Cross(right, forward));
    }

    public void Pan(Vector2 mouseDelta, Vector2 viewportSize)
    {
        if (viewportSize.Y <= 1.0f)
            return;

        var worldUnitsPerPixel = 2.0f * Distance * MathF.Tan(ToRadians(FieldOfViewDegrees) * 0.5f) / viewportSize.Y;
        var right = GetRightDirection();
        var up = GetUpDirection();

        Target -= right * (mouseDelta.X * worldUnitsPerPixel);
        Target += up * (mouseDelta.Y * worldUnitsPerPixel);
    }

    public void Reset()
    {
        YawDegrees = 35.0f;
        PitchDegrees = 25.0f;
        Distance = 4.0f;
        Target = Vector3.Zero;
    }

    public void FrameBounds(Vector3 boundsMin, Vector3 boundsMax)
    {
        var center = (boundsMin + boundsMax) * 0.5f;
        var extents = (boundsMax - boundsMin) * 0.5f;
        var radius = MathF.Max(extents.Length(), 0.25f);

        Target = center;
        Distance = Math.Clamp(radius * 2.8f, 1.5f, 200.0f);
    }

    private Vector3 GetOrbitDirection()
    {
        var yaw = ToRadians(YawDegrees);
        var pitch = ToRadians(PitchDegrees);

        var cosPitch = MathF.Cos(pitch);

        return Vector3.Normalize(new Vector3(
            MathF.Sin(yaw) * cosPitch,
            MathF.Sin(pitch),
            MathF.Cos(yaw) * cosPitch));
    }

    public static float ToRadians(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }
}
