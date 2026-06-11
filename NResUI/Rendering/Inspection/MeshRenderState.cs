namespace NResUI.Rendering.Inspection;

using NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Состояние просмотра меша. State/LOD задаются только для конкретных pieces,
/// потому что 0x01 хранит таблицу slot indices отдельно для каждого узла.
/// </summary>
public sealed class MeshRenderState
{
    public bool UseStoredNormals { get; set; } = true;
    public bool ShowEmptyPieces { get; set; } = true;
    public int SelectedPieceId { get; set; } = -1;
    public int SelectedBatchIndex { get; set; } = -1;
    public bool IsolateSelectedPiece { get; set; }
    public bool IsolateSelectedBatch { get; set; }
    public HashSet<int> HiddenPieceIds { get; } = [];
    public Dictionary<int, int> PieceStateOverrides { get; } = [];
    public Dictionary<int, int> PieceLodOverrides { get; } = [];
    public MshAnimationPoseMode AnimationPoseMode { get; set; } = MshAnimationPoseMode.DefaultKeyframe;
    public float AnimationSampleTime { get; set; }
    public float AnimationSpeed { get; set; } = 1.0f;
    public bool AnimationPlay { get; set; }

    public void ClearSelection()
    {
        SelectedPieceId = -1;
        SelectedBatchIndex = -1;
        IsolateSelectedPiece = false;
        IsolateSelectedBatch = false;
    }

    public int GetPieceState(int pieceId)
    {
        return PieceStateOverrides.GetValueOrDefault(pieceId, 0);
    }

    public int GetPieceLod(int pieceId)
    {
        return PieceLodOverrides.GetValueOrDefault(pieceId, 0);
    }

    public void SetPieceStateOverride(int pieceId, int state)
    {
        PieceStateOverrides[pieceId] = state;
    }

    public void SetPieceLodOverride(int pieceId, int lod)
    {
        PieceLodOverrides[pieceId] = lod;
    }

    public void ClearPieceOverrides(int pieceId)
    {
        PieceStateOverrides.Remove(pieceId);
        PieceLodOverrides.Remove(pieceId);
    }

    public void ClearAllPieceOverrides()
    {
        PieceStateOverrides.Clear();
        PieceLodOverrides.Clear();
    }

    public bool IsPieceVisible(int pieceId)
    {
        if (HiddenPieceIds.Contains(pieceId))
            return false;

        return !IsolateSelectedPiece || SelectedPieceId < 0 || pieceId == SelectedPieceId;
    }

    public bool IsBatchVisible(int batchIndex)
    {
        return !IsolateSelectedBatch || SelectedBatchIndex < 0 || batchIndex == SelectedBatchIndex;
    }
}
