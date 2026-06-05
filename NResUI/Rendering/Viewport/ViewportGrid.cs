using System.Numerics;
using NResUI.Rendering.Viewport.Meshes;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportGrid
{
    public GpuMesh Mesh { get; }
    public bool IsVisible { get; set; } = true;
    public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

    public ViewportGrid(GpuMesh mesh)
    {
        Mesh = mesh;
    }
}
