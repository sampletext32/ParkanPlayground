using NResUI.Rendering.Viewport.Meshes;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportScene
{
    private readonly List<ViewportPiece> _pieces = new();

    public IReadOnlyList<ViewportPiece> Pieces => _pieces;

    public ViewportGrid? Grid { get; set; }

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

    public void ClearSelection()
    {
        SelectedPieceId = -1;
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
}
