using NResUI.Rendering.Inspection;

namespace NResUI.Models;

/// <summary>
/// Общее состояние mesh viewport и inspector.
/// Панели синхронизируются через эту модель, чтобы выбор piece/batch не жил отдельно в каждом окне.
/// </summary>
public sealed class MeshViewportViewModel
{
    public MeshDocument? Document { get; private set; }
    public MeshRenderState RenderState { get; } = new();
    public string? LoadError { get; private set; }
    public bool NeedsSceneRebuild { get; private set; }

    public bool HasDocument => Document != null;

    /// <summary>Заменяет текущий документ и сбрасывает все viewport-only overrides.</summary>
    public void SetDocument(MeshDocument document)
    {
        Document = document;
        LoadError = null;
        RenderState.HiddenPieceIds.Clear();
        RenderState.ClearAllPieceOverrides();
        RenderState.ClearSelection();
        NeedsSceneRebuild = true;
    }

    public void SetError(string error)
    {
        Document = null;
        LoadError = error;
        RenderState.HiddenPieceIds.Clear();
        RenderState.ClearAllPieceOverrides();
        RenderState.ClearSelection();
        NeedsSceneRebuild = true;
    }

    public void ClearDocument()
    {
        Document = null;
        LoadError = null;
        RenderState.HiddenPieceIds.Clear();
        RenderState.ClearAllPieceOverrides();
        RenderState.ClearSelection();
        NeedsSceneRebuild = true;
    }

    public void MarkSceneRebuildNeeded()
    {
        NeedsSceneRebuild = true;
    }

    public void MarkSceneRebuilt()
    {
        NeedsSceneRebuild = false;
    }
}
