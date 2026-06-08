using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using NResUI.Rendering.Inspection;

namespace NResUI.ImGuiUI;

/// <summary>
/// Инспектор CPU-документа меша. Он не создает OpenGL-ресурсы сам, а только меняет MeshRenderState,
/// после чего ViewportPanel перестраивает видимую геометрию.
/// </summary>
public sealed class MeshInspectorPanel : IImGuiPanel
{
    private readonly MeshViewportViewModel _viewModel;
    private readonly HashSet<int> _expandedPieceDetails = [];
    private static readonly string[] ModelStateLabels = ["Обычное", "Collapsed", "Неизвестное 2"];
    private static readonly string[] LodLabels = ["LOD 0", "LOD -1", "LOD -2", "LOD -3", "LOD -4"];

    public MeshInspectorPanel(MeshViewportViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Инспектор меша"))
        {
            ImGui.End();
            return;
        }

        var document = _viewModel.Document;
        if (document == null)
        {
            ImGui.TextDisabled("Меш не загружен.");
            if (_viewModel.LoadError != null)
                ImGui.TextColored(new Vector4(1.0f, 0.35f, 0.25f, 1.0f), _viewModel.LoadError);
            ImGui.End();
            return;
        }

        DrawHeader(document);
        DrawRenderStateControls(document);
        DrawPieceTree(document);
        DrawWarnings(document);

        ImGui.End();
    }

    private void DrawHeader(MeshDocument document)
    {
        ImGui.Text($"Файл: {Path.GetFileName(document.SourcePath)}");
        ImGui.TextDisabled($"тип: {GetMeshTypeLabel(document.MeshType.ToString())} | частей: {document.Pieces.Count} | материалов: {document.Materials.Count}");
        ImGui.Separator();
    }

    private void DrawRenderStateControls(MeshDocument document)
    {
        var state = _viewModel.RenderState;

        var useStoredNormals = state.UseStoredNormals;
        if (ImGui.Checkbox("Нормали из MSH", ref useStoredNormals))
        {
            state.UseStoredNormals = useStoredNormals;
            _viewModel.MarkSceneRebuildNeeded();
        }
        DrawTooltip("Если включено, нормали вершин берутся из компонента MSH 0x04. Если выключено, нормали считаются из треугольников по позициям вершин, поэтому модель выглядит более граненой и показывает сырую геометрию.");

        ImGui.SameLine();

        var showEmptyPieces = state.ShowEmptyPieces;
        if (ImGui.Checkbox("Показывать пустые части", ref showEmptyPieces))
        {
            state.ShowEmptyPieces = showEmptyPieces;
            _viewModel.MarkSceneRebuildNeeded();
        }
        DrawTooltip("Показывает части без активного слота геометрии, без батчей или без валидных треугольников маленькими маркерами осей, чтобы их можно было выбрать.");

        var isolatePiece = state.IsolateSelectedPiece;
        if (ImGui.Checkbox("Изолировать выбранную часть", ref isolatePiece))
        {
            state.IsolateSelectedPiece = isolatePiece;
            _viewModel.MarkSceneRebuildNeeded();
        }
        DrawTooltip("Рендерит только выбранную часть. Если выбрать другую часть, изоляция переключится на нее.");

        ImGui.SameLine();

        var isolateBatch = state.IsolateSelectedBatch;
        if (ImGui.Checkbox("Изолировать выбранный батч", ref isolateBatch))
        {
            state.IsolateSelectedBatch = isolateBatch;
            _viewModel.MarkSceneRebuildNeeded();
        }
        DrawTooltip("Рендерит только выбранный батч из компонента 0x0D. Батч выбирается в таблице свойств части.");

        if (ImGui.Button("Показать все"))
        {
            state.HiddenPieceIds.Clear();
            state.IsolateSelectedPiece = false;
            state.IsolateSelectedBatch = false;
            state.ClearAllPieceOverrides();
            _viewModel.MarkSceneRebuildNeeded();
        }

        ImGui.Separator();
    }

    private void DrawPieceTree(MeshDocument document)
    {
        if (!ImGui.CollapsingHeader("Дерево частей", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var childrenByParent = document.Pieces
            .GroupBy(x => x.ParentId)
            .ToDictionary(x => x.Key, x => x.OrderBy(p => p.Id).ToList());

        // Parent -1 приходит из 0xFFFF в 0x01 и означает корневой piece.
        if (childrenByParent.TryGetValue(-1, out var roots))
        {
            foreach (var root in roots)
                DrawPieceNode(document, root, childrenByParent);
        }

        // Если parent index битый или указывает за пределы таблицы, показываем piece как orphan, а не теряем его.
        foreach (var orphan in document.Pieces.Where(x => x.ParentId != -1 && !document.Pieces.Any(p => p.Id == x.ParentId)))
            DrawPieceNode(document, orphan, childrenByParent);
    }

    private void DrawPieceNode(
        MeshDocument document,
        MeshPieceInfo piece,
        IReadOnlyDictionary<int, List<MeshPieceInfo>> childrenByParent)
    {
        var state = _viewModel.RenderState;
        var childCount = childrenByParent.TryGetValue(piece.Id, out var children) ? children.Count : 0;
        var hasChildren = childCount > 0;
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;
        if (state.SelectedPieceId == piece.Id)
            flags |= ImGuiTreeNodeFlags.Selected;

        var hidden = state.HiddenPieceIds.Contains(piece.Id);
        var childMarker = childCount > 0 ? $" | дочерних: {childCount}" : "";
        var hiddenMarker = hidden ? " | скрыта" : "";
        var label = $"{piece.Name}{childMarker}{hiddenMarker}##piece_{piece.Id}";
        var open = ImGui.TreeNodeEx(label, flags);
        var nodeClicked = ImGui.IsItemClicked();
        if (nodeClicked)
        {
            state.SelectedPieceId = piece.Id;
            if (state.IsolateSelectedPiece)
                _viewModel.MarkSceneRebuildNeeded();
        }

        if (ImGui.BeginPopupContextItem($"piece_context_{piece.Id}"))
        {
            if (ImGui.MenuItem(hidden ? "Показать часть" : "Скрыть часть"))
            {
                if (hidden)
                    state.HiddenPieceIds.Remove(piece.Id);
                else
                    state.HiddenPieceIds.Add(piece.Id);
                _viewModel.MarkSceneRebuildNeeded();
            }

            if (ImGui.MenuItem("Изолировать часть"))
            {
                state.SelectedPieceId = piece.Id;
                state.IsolateSelectedPiece = true;
                _viewModel.MarkSceneRebuildNeeded();
            }

            ImGui.EndPopup();
        }

        if (open)
        {
            if (state.SelectedPieceId == piece.Id)
                DrawPieceDetailsToggle(piece);

            if (_expandedPieceDetails.Contains(piece.Id))
                DrawPieceDetailsChild(document, piece);

            if (childrenByParent.TryGetValue(piece.Id, out var childNodes))
            {
                foreach (var child in childNodes)
                    DrawPieceNode(document, child, childrenByParent);
            }

            ImGui.TreePop();
        }
    }

    private void DrawPieceDetailsToggle(MeshPieceInfo piece)
    {
        var detailsOpen = _expandedPieceDetails.Contains(piece.Id);
        var buttonText = detailsOpen ? "Скрыть свойства" : "Свойства";
        if (ImGui.SmallButton($"{buttonText}##piece_details_button_{piece.Id}"))
        {
            if (detailsOpen)
                _expandedPieceDetails.Remove(piece.Id);
            else
                _expandedPieceDetails.Add(piece.Id);
        }
    }

    private void DrawPieceDetailsChild(MeshDocument document, MeshPieceInfo piece)
    {
        ImGui.Indent();
        ImGui.PushID(piece.Id);
        // Child-панель держит длинные таблицы внутри выбранного узла и не превращает дерево в прыгающий список.
        var height = MathF.Min(ImGui.GetTextLineHeightWithSpacing() * 24.0f, MathF.Max(220.0f, ImGui.GetContentRegionAvail().Y * 0.55f));
        if (ImGui.BeginChild("piece_details_child", new Vector2(0, height), ImGuiChildFlags.Border, ImGuiWindowFlags.None))
        {
            DrawSelectedPieceDetails(document, piece);
        }
        ImGui.EndChild();
        ImGui.PopID();
        ImGui.Unindent();
    }

    private void DrawSelectedPieceDetails(MeshDocument document, MeshPieceInfo piece)
    {
        var state = _viewModel.RenderState;
        ImGui.SeparatorText("Свойства части");

        DrawSelectedPieceStateControls(document, piece);

        var pieceState = state.GetPieceState(piece.Id);
        var pieceLod = state.GetPieceLod(piece.Id);
        var slot = piece.ResolveSlotIndex(pieceState, pieceLod);
        ImGui.Text($"Имя: {piece.Name}");
        ImGui.Text($"Родитель: {piece.ParentId}");
        ImGui.Text($"Флаги: {piece.Flags} (0x{(ushort)piece.Flags:X4})");
        ImGui.Text($"Состояние/LOD: {GetStateLabel(pieceState)}, {GetLodLabel(pieceLod)}");
        ImGui.Text($"Активный слот: {(slot == ushort.MaxValue ? "нет" : slot.ToString())}");
        ImGui.Text($"Поза покоя: {(piece.HasRestPose ? $"fallback-ключ {piece.FallbackKeyframeIndex}" : "нет")}");
        ImGui.Text($"Минимум границ: {piece.BoundsMin.X:F2}, {piece.BoundsMin.Y:F2}, {piece.BoundsMin.Z:F2}");
        ImGui.Text($"Максимум границ: {piece.BoundsMax.X:F2}, {piece.BoundsMax.Y:F2}, {piece.BoundsMax.Z:F2}");

        DrawSlotMatrix(document, piece);
        DrawBatchTable(piece);
    }

    private void DrawSelectedPieceStateControls(MeshDocument document, MeshPieceInfo piece)
    {
        var state = _viewModel.RenderState;
        NormalizePieceStateLod(document, piece);

        var availableStates = GetAvailablePieceStates(document, piece, state.GetPieceLod(piece.Id));
        var pieceState = state.GetPieceState(piece.Id);
        if (DrawValueCombo("Состояние части", pieceState, availableStates, GetStateLabel, out pieceState))
        {
            // Override хранится на piece, потому что 0x01 содержит отдельную state/LOD таблицу для каждого узла.
            state.SetPieceStateOverride(piece.Id, pieceState);
            state.SetPieceLodOverride(piece.Id, GetAvailablePieceLods(document, piece, pieceState).FirstOrDefault(state.GetPieceLod(piece.Id)));
            _viewModel.MarkSceneRebuildNeeded();
        }

        var availableLods = GetAvailablePieceLods(document, piece, state.GetPieceState(piece.Id));
        var pieceLod = state.GetPieceLod(piece.Id);
        if (DrawValueCombo("LOD части", pieceLod, availableLods, GetLodLabel, out pieceLod))
        {
            state.SetPieceStateOverride(piece.Id, state.GetPieceState(piece.Id));
            state.SetPieceLodOverride(piece.Id, pieceLod);
            _viewModel.MarkSceneRebuildNeeded();
        }

        if (ImGui.Button("Сбросить переопределение части"))
        {
            state.ClearPieceOverrides(piece.Id);
            _viewModel.MarkSceneRebuildNeeded();
        }
    }

    private static void DrawSlotMatrix(MeshDocument document, MeshPieceInfo piece)
    {
        if (!ImGui.CollapsingHeader("0x01 слоты состояния/LOD"))
            return;

        if (!ImGui.BeginTable("slot_matrix", document.LodCount + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            return;

        ImGui.TableSetupColumn("Состояние");
        for (var lod = 0; lod < document.LodCount; lod++)
            ImGui.TableSetupColumn(GetLodLabel(lod));
        ImGui.TableHeadersRow();

        for (var state = 0; state < document.ModelStateCount; state++)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(GetStateLabel(state));

            for (var lod = 0; lod < document.LodCount; lod++)
            {
                ImGui.TableNextColumn();
                var slot = piece.ResolveSlotIndex(state, lod);
                // 0xFFFF в 0x01 значит "нет геометрии" для этой пары state/LOD.
                ImGui.Text(slot == ushort.MaxValue ? "-" : slot.ToString());
            }
        }

        ImGui.EndTable();
    }

    private void DrawBatchTable(MeshPieceInfo piece)
    {
        if (!ImGui.CollapsingHeader($"0x0D батчи ({piece.Batches.Count})", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        if (!ImGui.BeginTable("mesh_batches", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            return;

        ImGui.TableSetupColumn("#");
        ImGui.TableSetupColumn("Материал");
        ImGui.TableSetupColumn("Флаги");
        ImGui.TableSetupColumn("Треуг.");
        ImGui.TableSetupColumn("Диапазон индексов");
        ImGui.TableSetupColumn("Базовая вершина");
        ImGui.TableSetupColumn("Предупреждения");
        ImGui.TableHeadersRow();

        foreach (var batch in piece.Batches)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var selected = _viewModel.RenderState.SelectedBatchIndex == batch.BatchIndex;
            if (ImGui.Selectable(batch.BatchIndex.ToString(), selected, ImGuiSelectableFlags.SpanAllColumns))
            {
                _viewModel.RenderState.SelectedBatchIndex = batch.BatchIndex;
                _viewModel.RenderState.IsolateSelectedBatch = true;
                _viewModel.MarkSceneRebuildNeeded();
            }

            ImGui.TableNextColumn();
            ImGui.Text(batch.MaterialName ?? $"#{batch.MaterialId}");
            ImGui.TableNextColumn();
            ImGui.Text($"0x{(ushort)batch.Flags:X4}");
            ImGui.TableNextColumn();
            ImGui.Text(batch.TriangleCount.ToString());
            ImGui.TableNextColumn();
            ImGui.Text($"{batch.IndexStart}..{batch.IndexStart + batch.IndexCount}");
            ImGui.TableNextColumn();
            ImGui.Text(batch.BaseVertex.ToString());
            ImGui.TableNextColumn();
            ImGui.Text(batch.Warnings.Count == 0 ? "" : string.Join("; ", batch.Warnings));
        }

        ImGui.EndTable();
    }

    private void NormalizePieceStateLod(MeshDocument document, MeshPieceInfo piece)
    {
        var renderState = _viewModel.RenderState;
        var pieceState = renderState.GetPieceState(piece.Id);
        var pieceLod = renderState.GetPieceLod(piece.Id);
        if (HasSlot(document, piece, pieceState, pieceLod))
            return;

        var firstPair = EnumerateStateLodPairs(document, piece).FirstOrDefault();
        renderState.SetPieceStateOverride(piece.Id, firstPair.State);
        renderState.SetPieceLodOverride(piece.Id, firstPair.Lod);
    }

    private static IReadOnlyList<int> GetAvailablePieceStates(MeshDocument document, MeshPieceInfo piece, int lod)
    {
        return Enumerable.Range(0, document.ModelStateCount)
            .Where(state => HasSlot(document, piece, state, lod))
            .ToList();
    }

    private static IReadOnlyList<int> GetAvailablePieceLods(MeshDocument document, MeshPieceInfo piece, int state)
    {
        return Enumerable.Range(0, document.LodCount)
            .Where(lod => HasSlot(document, piece, state, lod))
            .ToList();
    }

    private static IEnumerable<(int State, int Lod)> EnumerateStateLodPairs(MeshDocument document, MeshPieceInfo piece)
    {
        for (var state = 0; state < document.ModelStateCount; state++)
        {
            for (var lod = 0; lod < document.LodCount; lod++)
            {
                if (HasSlot(document, piece, state, lod))
                    yield return (state, lod);
            }
        }
    }

    private static bool HasSlot(MeshDocument document, MeshPieceInfo piece, int state, int lod)
    {
        var slot = piece.ResolveSlotIndex(state, lod);
        if (slot == ushort.MaxValue)
            return false;

        return document.MshModelGeometry == null || slot < document.MshModelGeometry.GeometrySlots.Slots.Count;
    }

    private static bool DrawValueCombo(
        string label,
        int currentValue,
        IReadOnlyList<int> values,
        Func<int, string> getLabel,
        out int selectedValue)
    {
        selectedValue = currentValue;
        if (values.Count == 0)
        {
            ImGui.BeginDisabled();
            ImGui.Text($"{label}: нет");
            ImGui.EndDisabled();
            return false;
        }

        var preview = values.Contains(currentValue) ? getLabel(currentValue) : getLabel(values[0]);
        var changed = false;
        if (!ImGui.BeginCombo(label, preview))
            return false;

        foreach (var value in values)
        {
            var isSelected = value == currentValue;
            if (ImGui.Selectable(getLabel(value), isSelected))
            {
                selectedValue = value;
                changed = value != currentValue;
            }

            if (isSelected)
                ImGui.SetItemDefaultFocus();
        }

        ImGui.EndCombo();
        return changed;
    }

    private static string GetStateLabel(int state)
    {
        return state >= 0 && state < ModelStateLabels.Length
            ? ModelStateLabels[state]
            : $"Состояние {state}";
    }

    private static string GetLodLabel(int lod)
    {
        return lod >= 0 && lod < LodLabels.Length
            ? LodLabels[lod]
            : $"LOD {lod}";
    }

    private static string GetMeshTypeLabel(string meshType)
    {
        return meshType switch
        {
            "Model" => "модель",
            "Landscape" => "ландшафт",
            "Unknown" => "неизвестно",
            _ => meshType
        };
    }

    private static void DrawTooltip(string text)
    {
        if (!ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 32.0f);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    private static void DrawWarnings(MeshDocument document)
    {
        if (document.Warnings.Count == 0)
            return;

        if (!ImGui.CollapsingHeader($"Предупреждения ({document.Warnings.Count})"))
            return;

        foreach (var warning in document.Warnings)
            ImGui.TextWrapped(warning);
    }
}
