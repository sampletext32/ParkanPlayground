using System.Numerics;
using NResUI.Rendering.Viewport.Meshes;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportPiece
{
    public int Id { get; }
    public string Name { get; }
    public GpuMesh Mesh { get; }

    public Matrix4x4 LocalTransform { get; set; }

    public Vector3 BoundsMin { get; }
    public Vector3 BoundsMax { get; }

    public ViewportPieceDebugInfo? DebugInfo { get; }

    public ViewportPiece(
        int id,
        string name,
        GpuMesh mesh,
        Matrix4x4 localTransform,
        Vector3 boundsMin,
        Vector3 boundsMax,
        ViewportPieceDebugInfo? debugInfo = null)
    {
        Id = id;
        Name = name;
        Mesh = mesh;
        LocalTransform = localTransform;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
        DebugInfo = debugInfo;
    }

    public static ViewportPiece CreateUnitCube(int id, string name, GpuMesh mesh)
    {
        return new ViewportPiece(
            id,
            name,
            mesh,
            Matrix4x4.Identity,
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new ViewportPieceDebugInfo
            {
                SourceKind = "Debug primitive",
                SourcePieceIndex = id,
                SourceParentIndex = -1,
                GeometrySlotIndex = 0,
                Msh01Flags = 0,
                BatchCount = 1,
                TriangleCount = 12
            });
    }
}
