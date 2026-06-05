namespace NResUI.Rendering.Viewport.Msh;

public sealed class MshViewportLoadResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? SourcePath { get; }
    public IReadOnlyList<ViewportPiece> Pieces { get; }

    private MshViewportLoadResult(
        bool isSuccess,
        string? error,
        string? sourcePath,
        IReadOnlyList<ViewportPiece> pieces)
    {
        IsSuccess = isSuccess;
        Error = error;
        SourcePath = sourcePath;
        Pieces = pieces;
    }

    public static MshViewportLoadResult Success(string sourcePath, IReadOnlyList<ViewportPiece> pieces)
    {
        return new MshViewportLoadResult(true, null, sourcePath, pieces);
    }

    public static MshViewportLoadResult Failure(string error, string? sourcePath = null)
    {
        return new MshViewportLoadResult(false, error, sourcePath, Array.Empty<ViewportPiece>());
    }
}
