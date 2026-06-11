using System.Numerics;
using NResUI.Rendering.Viewport.Meshes;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportPiece : IDisposable
{
    public int Id { get; }
    public string Name { get; }
    public IReadOnlyList<GpuMesh> Meshes { get; }

    public GpuMesh Mesh => Meshes[0];

    public Matrix4x4 LocalTransform { get; set; }

    public Vector3 BoundsMin { get; }
    public Vector3 BoundsMax { get; }

    public ViewportPieceDebugInfo? DebugInfo { get; }
    public IReadOnlyList<int> SourceBatchIndices { get; }

    public ViewportPiece(
        int id,
        string name,
        GpuMesh mesh,
        Matrix4x4 localTransform,
        Vector3 boundsMin,
        Vector3 boundsMax,
        ViewportPieceDebugInfo? debugInfo = null,
        IReadOnlyList<int>? sourceBatchIndices = null)
        : this(id, name, new[] { mesh }, localTransform, boundsMin, boundsMax, debugInfo, sourceBatchIndices)
    {
    }

    public ViewportPiece(
        int id,
        string name,
        IReadOnlyList<GpuMesh> meshes,
        Matrix4x4 localTransform,
        Vector3 boundsMin,
        Vector3 boundsMax,
        ViewportPieceDebugInfo? debugInfo = null,
        IReadOnlyList<int>? sourceBatchIndices = null)
    {
        if (meshes.Count == 0)
            throw new ArgumentException("A viewport piece must contain at least one mesh.", nameof(meshes));

        Id = id;
        Name = name;
        Meshes = meshes;
        LocalTransform = localTransform;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
        DebugInfo = debugInfo;
        SourceBatchIndices = sourceBatchIndices ?? [];
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

    public void Dispose()
    {
        foreach (var mesh in Meshes)
            mesh.Dispose();
    }
}
