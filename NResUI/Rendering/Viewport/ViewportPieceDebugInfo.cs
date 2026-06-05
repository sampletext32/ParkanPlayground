namespace NResUI.Rendering.Viewport;

public sealed class ViewportPieceDebugInfo
{
    public int SourcePieceIndex { get; init; }
    public int SourceParentIndex { get; init; }
    public int GeometrySlotIndex { get; init; }
    public uint Msh01Flags { get; init; }
    public int BatchCount { get; init; }
    public int TriangleCount { get; init; }

    public string SourceKind { get; init; } = "MSH";
}
