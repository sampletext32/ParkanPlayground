namespace NResUI.Rendering.Viewport;

public sealed class ViewportDebugSettings
{
    public bool ShowOriginAxes { get; set; } = true;
    public bool ShowPieceOrigins { get; set; } = true;
    public bool ShowSelectedBounds { get; set; } = true;
    public bool ShowSceneBounds { get; set; }
    public bool Wireframe { get; set; }
}
