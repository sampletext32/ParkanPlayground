using System.Numerics;
using NResUI.Rendering.Viewport.Meshes;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportScene
{
    private readonly List<ViewportPiece> _pieces = new();

    public IReadOnlyList<ViewportPiece> Pieces => _pieces;

    public ViewportGrid? Grid { get; set; }

    public ViewportDebugSettings Debug { get; } = new();

    public int SelectedPieceId { get; set; } = -1;

    public ViewportPiece? SelectedPiece
    {
        get
        {
            if (SelectedPieceId < 0)
                return null;

            foreach (var piece in _pieces)
            {
                if (piece.Id == SelectedPieceId)
                    return piece;
            }

            return null;
        }
    }

    public void AddPiece(ViewportPiece piece)
    {
        _pieces.Add(piece);
    }

    public void ClearPieces()
    {
        _pieces.Clear();
        ClearSelection();
    }

    public void ReplacePieces(IEnumerable<ViewportPiece> pieces)
    {
        _pieces.Clear();
        _pieces.AddRange(pieces);
        ClearSelection();
    }

    public void ClearSelection()
    {
        SelectedPieceId = -1;
    }

    public bool TryGetPieceWorldBounds(ViewportPiece piece, out ViewportBounds bounds)
    {
        return TryTransformBounds(piece.BoundsMin, piece.BoundsMax, piece.LocalTransform, out bounds);
    }

    public bool TryGetSelectedPieceWorldBounds(out ViewportBounds bounds)
    {
        var selectedPiece = SelectedPiece;
        if (selectedPiece == null)
        {
            bounds = default;
            return false;
        }

        return TryGetPieceWorldBounds(selectedPiece, out bounds);
    }

    public bool TryGetSceneWorldBounds(out ViewportBounds bounds)
    {
        var hasBounds = false;
        var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var piece in _pieces)
        {
            if (!TryGetPieceWorldBounds(piece, out var pieceBounds))
                continue;

            min = Vector3.Min(min, pieceBounds.Min);
            max = Vector3.Max(max, pieceBounds.Max);
            hasBounds = true;
        }

        bounds = hasBounds ? new ViewportBounds(min, max) : default;
        return hasBounds;
    }

    public static ViewportScene CreateDefaultCubeScene(GpuMesh cubeMesh, GpuMesh gridMesh)
    {
        var scene = new ViewportScene
        {
            Grid = new ViewportGrid(gridMesh)
        };

        scene.AddPiece(ViewportPiece.CreateUnitCube(0, "Cube", cubeMesh));

        return scene;
    }

    private static bool TryTransformBounds(
        Vector3 localMin,
        Vector3 localMax,
        Matrix4x4 transform,
        out ViewportBounds bounds)
    {
        var corners = new[]
        {
            new Vector3(localMin.X, localMin.Y, localMin.Z),
            new Vector3(localMax.X, localMin.Y, localMin.Z),
            new Vector3(localMin.X, localMax.Y, localMin.Z),
            new Vector3(localMax.X, localMax.Y, localMin.Z),
            new Vector3(localMin.X, localMin.Y, localMax.Z),
            new Vector3(localMax.X, localMin.Y, localMax.Z),
            new Vector3(localMin.X, localMax.Y, localMax.Z),
            new Vector3(localMax.X, localMax.Y, localMax.Z),
        };

        var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var corner in corners)
        {
            var world = Vector3.Transform(corner, transform);
            min = Vector3.Min(min, world);
            max = Vector3.Max(max, world);
        }

        bounds = new ViewportBounds(min, max);
        return true;
    }
}
