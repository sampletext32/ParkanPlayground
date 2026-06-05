using System.Numerics;

namespace NResUI.Rendering.Viewport;

public readonly struct ViewportBounds
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public ViewportBounds(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;
}
