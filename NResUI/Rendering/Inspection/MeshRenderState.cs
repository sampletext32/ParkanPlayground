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

    /// <summary>Что проигрывает плеер: всю шкалу MSH, отдельный клип (состояние CTL) или цепочку клипов.</summary>
    public MshAnimationSource AnimationSource { get; set; } = MshAnimationSource.Timeline;

    /// <summary>Клипы и цепочки, найденные в CTL модели (пусто, если CTL не загружен).</summary>
    public MshClipSet ClipSet { get; set; } = MshClipSet.Empty;

    /// <summary>Имя загруженного CTL (для подписи).</summary>
    public string? ClipSourceName { get; set; }

    /// <summary>Цепочки проходят через состояния-цели (несколько фаз движения подряд).</summary>
    public bool JoinChains { get; set; }

    public int SelectedClipIndex { get; set; } = -1;

    /// <summary>Прогресс выбранного клипа или цепочки 0..1.</summary>
    public float ClipProgress { get; set; }

    /// <summary>Длительность проигрывания клипа или цепочки в предпросмотре, с.</summary>
    public float ClipDurationSeconds { get; set; } = 1.0f;

    /// <summary>Скорость движения для расчёта длительности по смещению корня (в пределах полосы скоростей состояния).</summary>
    public float ClipMoveSpeed { get; set; }

    /// <summary>Длительность считается по смещению корня и скорости (как в игре), а не вводится вручную.</summary>
    public bool ClipAutoDuration { get; set; } = true;

    public MshClipLoopMode ClipLoopMode { get; set; } = MshClipLoopMode.Loop;

    /// <summary>Направление проигрывания при «туда-обратно»: 1 - вперёд, -1 - назад.</summary>
    public int ClipDirection { get; set; } = 1;

    /// <summary>Вес диапазона 2 в предпросмотре (в игре зависит от скорости юнита: от веса при минимальной скорости до 1).</summary>
    public float ClipWeight2 { get; set; } = 1.0f;

    /// <summary>Текущие значения каналов 0..1 (по порядку <see cref="MshClipSet.Channels"/>).</summary>
    public float[] ChannelValues { get; set; } = [];

    /// <summary>Направление движения каждого канала при «туда-обратно»: 1 или -1.</summary>
    public int[] ChannelDirections { get; set; } = [];

    /// <summary>Элементы списка для текущего источника (пусто в режиме шкалы).</summary>
    public IReadOnlyList<MshClipSequence> ActiveItems => AnimationSource switch
    {
        MshAnimationSource.Clips => ClipSet.Clips,
        MshAnimationSource.Chains => JoinChains ? ClipSet.JoinedChains : ClipSet.PhaseChains,
        _ => []
    };

    public void ResetClips()
    {
        AnimationSource = MshAnimationSource.Timeline;
        ClipSet = MshClipSet.Empty;
        ClipSourceName = null;
        SelectedClipIndex = -1;
        ClipProgress = 0.0f;
        ClipDirection = 1;
        ChannelValues = [];
        ChannelDirections = [];
    }

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
