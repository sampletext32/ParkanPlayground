using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;
using NResUI.Models;
using NResUI.Rendering.Inspection;
using NResUI.Rendering.Import.Msh;
using NResUI.Rendering.Materials;
using NResUI.Rendering.Viewport;
using NativeFileDialogSharp;
using NResUI.Rendering.Viewport.Meshes;
using NResUI.Rendering.Viewport.Msh;
using ControlLib;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.ImGuiUI;

public sealed class ViewportPanel : IImGuiPanel, IUpdateReceiver
{
    private readonly ViewportRenderer _renderer;
    private readonly ViewportScene _scene;
    private readonly ViewportCamera _camera = new();
    private readonly ViewportInputController _inputController = new();
    private readonly IWindow _window;
    private readonly IConfigProvider _configProvider;
    private readonly MeshViewportViewModel _viewModel;

    private ViewportMaterialSet _materialSet = ViewportMaterialSet.Empty;

    public ViewportPanel(GL gl, IWindow window, IConfigProvider configProvider, MeshViewportViewModel viewModel)
    {
        _window = window;
        _configProvider = configProvider;
        _viewModel = viewModel;
        var cubeMesh = PrimitiveMeshes.CreateCube(gl);
        var gridMesh = PrimitiveMeshes.CreateWorldGrid(gl);

        _scene = ViewportScene.CreateDefaultCubeScene(cubeMesh, gridMesh);
        _renderer = new ViewportRenderer(gl, window);
    }

    public void OnImGuiRender()
    {
        if (!ImGui.Begin("Viewport"))
        {
            ImGui.End();
            return;
        }

        DrawModelControls();
        DrawSelectionStatus();
        DrawViewportToolbar();
        DrawDebugControls();
        DrawAnimationControls();
        RebuildSceneIfNeeded();
        SyncSelectionFromViewModel();

        var imageSize = ImGui.GetContentRegionAvail();
        if (imageSize.X < 32 || imageSize.Y < 32)
        {
            ImGui.TextDisabled("Viewport is too small.");
            ImGui.End();
            return;
        }

        var textureId = _renderer.Render(
            width: Math.Max(1, (int)imageSize.X),
            height: Math.Max(1, (int)imageSize.Y),
            scene: _scene,
            camera: _camera);

        ImGui.Image(
            (IntPtr)textureId,
            imageSize,
            new Vector2(0, 1),
            new Vector2(1, 0));

        var viewportHovered = ImGui.IsItemHovered();
        var viewportFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
        var imageMin = ImGui.GetItemRectMin();

        DrawViewportHelpOverlay();

        _inputController.Handle(
            _renderer,
            _scene,
            _camera,
            viewportHovered,
            viewportFocused,
            imageMin,
            imageSize);

        if (_viewModel.RenderState.SelectedPieceId != _scene.SelectedPieceId)
            _viewModel.RenderState.SelectedPieceId = _scene.SelectedPieceId;

        ImGui.End();
    }

    public void OnUpdate(float delta)
    {
        var document = _viewModel.Document;
        if (document?.MshModelGeometry == null)
            return;

        var state = _viewModel.RenderState;
        if (!state.AnimationPlay)
            return;

        // Окно работает в событийном режиме (Program.cs: IsEventDriven): без ввода кадры не приходят, и проигрывание шло бы 1-2 кадра в секунду.
        _window.ContinueEvents();

        if (state.AnimationSource == MshAnimationSource.Channels)
        {
            AdvanceChannels(state, delta);
            ApplyCurrentAnimationPose(document);
            return;
        }

        if (state.AnimationSource != MshAnimationSource.Timeline)
        {
            AdvanceClip(state, delta);
            ApplyCurrentAnimationPose(document);
            return;
        }

        var maxAnimationTime = GetMaxAnimationTime(document);
        if (maxAnimationTime <= 0.0f)
            return;

        state.AnimationSampleTime = WrapAnimationTime(
            state.AnimationSampleTime + delta * state.AnimationSpeed,
            maxAnimationTime);
        state.AnimationPoseMode = MshAnimationPoseMode.AnimationSample;
        ApplyCurrentAnimationPose(document);
    }


    private void DrawModelControls()
    {
        if (ImGui.Button("Open MSH in viewport"))
        {
            var result = Dialog.FileOpen("msh");
            if (result.IsOk)
                LoadMsh(result.Path);
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset cube"))
        {
            var cubeMesh = PrimitiveMeshes.CreateCube(_renderer.Gl);
            _scene.ReplacePieces([ViewportPiece.CreateUnitCube(0, "Cube", cubeMesh)]);
            _viewModel.ClearDocument();
            _camera.Reset();
        }

        if (_viewModel.Document != null)
            ImGui.TextDisabled($"Model: {Path.GetFileName(_viewModel.Document.SourcePath)}");

        if (_viewModel.LoadError != null)
            ImGui.TextColored(new Vector4(1.0f, 0.35f, 0.25f, 1.0f), $"MSH load failed: {_viewModel.LoadError}");
    }

    private void LoadMsh(string path)
    {
        // Сначала строим CPU-документ для inspector, затем отдельно резолвим материалы и OpenGL textures.
        var loadResult = MshMeshDocumentImporter.LoadFromFile(path);
        if (!loadResult.IsSuccess)
        {
            _viewModel.SetError(loadResult.Error ?? "Unknown error.");
            return;
        }

        _viewModel.SetDocument(loadResult.Document!);

        var ctlPath = MshClipLoader.FindCtlForMsh(path);
        if (ctlPath != null && LoadClips(ctlPath))
        {
            var renderState = _viewModel.RenderState;
            renderState.AnimationSource = ChooseDefaultSource(renderState);
            EnsureValidSelection(renderState, loadResult.Document!.MshModelGeometry);
        }

        _materialSet = ViewportMaterialResolver.LoadForMsh(
            _renderer.Gl,
            path,
            _configProvider,
            loadResult.Document!.Warnings is ICollection<string> mutableWarnings ? mutableWarnings : null);
        RebuildSceneIfNeeded();

        if (_scene.TryGetSceneWorldBounds(out var bounds))
            _camera.FrameBounds(bounds.Min, bounds.Max);
        else
            _camera.Reset();
    }

    private void DrawSelectionStatus()
    {
        var selectedPiece = _scene.SelectedPiece;
        if (selectedPiece != null)
        {
            ImGui.Text($"Selected: {selectedPiece.Name}");

            var debugInfo = selectedPiece.DebugInfo;
            if (debugInfo != null)
            {
                ImGui.TextDisabled(
                    $"{debugInfo.SourceKind} | parent {debugInfo.SourceParentIndex} | slot {debugInfo.GeometrySlotIndex} | " +
                    $"flags 0x{debugInfo.Msh01Flags:X4} | batches {debugInfo.BatchCount} | tris {debugInfo.TriangleCount}");
            }
        }
        else
        {
            ImGui.TextDisabled("Selected: none");
        }
    }

    private void RebuildSceneIfNeeded()
    {
        if (!_viewModel.NeedsSceneRebuild)
            return;

        var document = _viewModel.Document;
        if (document == null)
        {
            _viewModel.MarkSceneRebuilt();
            return;
        }

        // Перестройка нужна при смене state/LOD, hide/isolate и batch filter: все это меняет состав GPU-мешей.
        var pieces = MshViewportMeshFactory.BuildViewportPieces(
            _renderer.Gl,
            document,
            _viewModel.RenderState,
            _materialSet);

        _scene.ReplacePieces(pieces);
        _scene.SelectedPieceId = _viewModel.RenderState.SelectedPieceId;
        ApplyCurrentAnimationPose(document);
        _viewModel.MarkSceneRebuilt();
    }

    private void ApplyCurrentAnimationPose(MeshDocument document)
    {
        var modelGeometry = document.MshModelGeometry;
        if (modelGeometry == null)
            return;

        var renderState = _viewModel.RenderState;
        var poseMode = renderState.AnimationPoseMode;
        var blend = new MshAnimationBlend(renderState.AnimationSampleTime, -1.0f, 0.0f);

        Dictionary<int, MshAnimationBlend>? perNodeBlend = null;

        if (renderState.AnimationSource == MshAnimationSource.Channels)
        {
            // Общее состояние - первое состояние CTL (у моделей без движений это одна поза), детали с каналами ставятся отдельно.
            var clips = renderState.ClipSet.Clips;
            blend = clips.Count > 0
                ? clips[0].Sample(0.0f, clips[0].DefaultWeight2)
                : new MshAnimationBlend(-1.0f, -1.0f, 0.0f);
            poseMode = MshAnimationPoseMode.AnimationSample;

            var channels = renderState.ClipSet.Channels;
            perNodeBlend = new Dictionary<int, MshAnimationBlend>();
            for (var i = 0; i < channels.Count && i < renderState.ChannelValues.Length; i++)
            {
                var time = channels[i].TimeAt(renderState.ChannelValues[i]);
                perNodeBlend[channels[i].PieceIndex] = channels[i].UsesRange2
                    ? new MshAnimationBlend(-1.0f, time, 1.0f)
                    : new MshAnimationBlend(time, -1.0f, 0.0f);
            }
        }
        else if (renderState.AnimationSource != MshAnimationSource.Timeline && TryGetSelectedItem(renderState, out var item))
        {
            blend = item.Sample(renderState.ClipProgress, renderState.ClipWeight2);
            poseMode = MshAnimationPoseMode.AnimationSample;
        }

        var poses = MshRestPoseBuilder.BuildPose(
            modelGeometry.Nodes,
            modelGeometry.TransformKeyframes,
            modelGeometry.AnimationMap,
            poseMode,
            blend,
            perNodeBlend,
            modelGeometry.MaxAnimationTime);

        var mshToViewportTransform = Matrix4x4.CreateRotationX(-MathF.PI * 0.5f);
        foreach (var piece in _scene.Pieces)
        {
            if (piece.Id < 0 || piece.Id >= poses.Count)
                continue;

            piece.LocalTransform = poses[piece.Id].MeshSpaceTransform * mshToViewportTransform;
        }
    }

    private void SyncSelectionFromViewModel()
    {
        if (_viewModel.Document == null)
            return;

        // Inspector может выбрать piece без клика по viewport; renderer должен подсветить тот же id.
        _scene.SelectedPieceId = _viewModel.RenderState.SelectedPieceId;
    }

    private void DrawViewportToolbar()
    {
        if (ImGui.Button("Reset view"))
            _camera.Reset();

        ImGui.SameLine();

        if (ImGui.Button("Frame selected"))
        {
            if (_scene.TryGetSelectedPieceWorldBounds(out var selectedBounds))
                _camera.FrameBounds(selectedBounds.Min, selectedBounds.Max);
        }

        ImGui.SameLine();

        if (ImGui.Button("Frame all"))
        {
            if (_scene.TryGetSceneWorldBounds(out var sceneBounds))
                _camera.FrameBounds(sceneBounds.Min, sceneBounds.Max);
            else
                _camera.Reset();
        }
    }

    private void DrawDebugControls()
    {
        var grid = _scene.Grid;
        if (grid != null)
        {
            var isVisible = grid.IsVisible;
            if (ImGui.Checkbox("Grid", ref isVisible))
                grid.IsVisible = isVisible;
        }

        ImGui.SameLine();

        var showOriginAxes = _scene.Debug.ShowOriginAxes;
        if (ImGui.Checkbox("Origin axes", ref showOriginAxes))
            _scene.Debug.ShowOriginAxes = showOriginAxes;

        ImGui.SameLine();

        var showPieceOrigins = _scene.Debug.ShowPieceOrigins;
        if (ImGui.Checkbox("Piece axes", ref showPieceOrigins))
            _scene.Debug.ShowPieceOrigins = showPieceOrigins;

        ImGui.SameLine();

        var showSelectedBounds = _scene.Debug.ShowSelectedBounds;
        if (ImGui.Checkbox("Selected bounds", ref showSelectedBounds))
            _scene.Debug.ShowSelectedBounds = showSelectedBounds;

        ImGui.SameLine();

        var showSceneBounds = _scene.Debug.ShowSceneBounds;
        if (ImGui.Checkbox("Scene bounds", ref showSceneBounds))
            _scene.Debug.ShowSceneBounds = showSceneBounds;

        ImGui.SameLine();

        var wireframe = _scene.Debug.Wireframe;
        if (ImGui.Checkbox("Wireframe", ref wireframe))
            _scene.Debug.Wireframe = wireframe;
    }

    private static readonly string[] AnimationSourceNames = ["Timeline", "Clips", "Chains", "Channels"];
    private static readonly string[] LoopModeNames = ["Once", "Loop", "Ping-pong"];

    private void DrawAnimationControls()
    {
        // Панель сворачивается: список клипов и ползунки занимали слишком много места над вьюпортом.
        if (!ImGui.CollapsingHeader("Animation", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        var document = _viewModel.Document;
        var state = _viewModel.RenderState;
        var hasAnimationDocument = document?.MshModelGeometry != null;
        var maxAnimationTime = hasAnimationDocument ? GetMaxAnimationTime(document!) : 0.0f;

        if (!hasAnimationDocument)
            ImGui.BeginDisabled();

        // Строка 1: источник, загрузка CTL, сброс.
        var sourceIndex = (int)state.AnimationSource;
        ImGui.SetNextItemWidth(100);
        if (ImGui.Combo("##source", ref sourceIndex, AnimationSourceNames, AnimationSourceNames.Length))
        {
            state.AnimationSource = (MshAnimationSource)sourceIndex;
            state.AnimationPlay = false;
            EnsureValidSelection(state, document?.MshModelGeometry);
            if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        ImGui.SameLine();
        if (ImGui.Button("Load CTL..."))
        {
            var result = Dialog.FileOpen("ctl");
            if (result.IsOk && LoadClips(result.Path))
            {
                state.AnimationSource = ChooseDefaultSource(state);
                EnsureValidSelection(state, document?.MshModelGeometry);
                if (document != null)
                    ApplyCurrentAnimationPose(document);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Rest pose"))
        {
            state.AnimationPlay = false;
            state.AnimationSource = MshAnimationSource.Timeline;
            state.AnimationPoseMode = MshAnimationPoseMode.DefaultKeyframe;
            if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        if (state.AnimationSource == MshAnimationSource.Timeline)
            DrawTimelineControls(document, state, maxAnimationTime);
        else if (state.AnimationSource == MshAnimationSource.Channels)
            DrawChannelControls(document, state);
        else
            DrawClipControls(document, state, maxAnimationTime);

        if (!hasAnimationDocument)
            ImGui.EndDisabled();
    }

    private void DrawTimelineControls(MeshDocument? document, MeshRenderState state, float maxAnimationTime)
    {
        var play = state.AnimationPlay;
        if (ImGui.Checkbox("Play", ref play))
        {
            state.AnimationPlay = play;
            if (play)
                state.AnimationPoseMode = MshAnimationPoseMode.AnimationSample;
            else if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        var speed = state.AnimationSpeed;
        if (ImGui.DragFloat("Speed", ref speed, 0.05f, -10.0f, 10.0f, "%.2f"))
            state.AnimationSpeed = speed;

        ImGui.SetNextItemWidth(-1);
        var animationSampleTime = state.AnimationSampleTime;
        if (ImGui.SliderFloat("##time", ref animationSampleTime, 0.0f, maxAnimationTime, "time %.1f"))
        {
            state.AnimationSampleTime = animationSampleTime;
            state.AnimationPoseMode = MshAnimationPoseMode.AnimationSample;
            if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        ImGui.TextDisabled("Whole timeline: all movements of the model are stored back to back. Choose Clips/Chains to split it.");
    }

    private void DrawClipControls(MeshDocument? document, MeshRenderState state, float maxAnimationTime)
    {
        var items = state.ActiveItems;

        // Строка 2: выбор элемента одним выпадающим списком + шаг назад/вперёд + склейка цепочек.
        if (state.AnimationSource == MshAnimationSource.Chains)
        {
            var join = state.JoinChains;
            if (ImGui.Checkbox("Join phases", ref join))
            {
                state.JoinChains = join;
                state.SelectedClipIndex = -1;
                EnsureValidSelection(state, document?.MshModelGeometry);
                if (document != null)
                    ApplyCurrentAnimationPose(document);
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Continue a phase into the next one through an allowed transition (e.g. left + right step).");

            ImGui.SameLine();
        }

        if (items.Count == 0)
        {
            ImGui.TextDisabled(state.ClipSourceName == null
                ? "CTL not found next to the MSH: use \"Load CTL...\" (bases.rlb/*_CTLD_<name>.ctl)."
                : $"{state.ClipSourceName}: no items.");
            return;
        }

        var arrowWidth = ImGui.GetFrameHeight();
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var selectedLabel = state.SelectedClipIndex >= 0 && state.SelectedClipIndex < items.Count
            ? items[state.SelectedClipIndex].Label
            : "-";

        ImGui.SetNextItemWidth(Math.Max(120.0f, ImGui.GetContentRegionAvail().X - 2 * (arrowWidth + spacing)));
        if (ImGui.BeginCombo("##item", $"{state.SelectedClipIndex + 1}/{items.Count}  {selectedLabel}", ImGuiComboFlags.HeightLarge))
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (ImGui.Selectable(items[i].Label, i == state.SelectedClipIndex))
                    ChangeSelection(document, state, i);
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.ArrowButton("##prev", ImGuiDir.Left))
            ChangeSelection(document, state, (state.SelectedClipIndex - 1 + items.Count) % items.Count);

        ImGui.SameLine();
        if (ImGui.ArrowButton("##next", ImGuiDir.Right))
            ChangeSelection(document, state, (state.SelectedClipIndex + 1) % items.Count);

        if (!TryGetSelectedItem(state, out var item))
            return;

        // Строка 3: проигрывание, режим конца, скорость.
        if (ImGui.Button(state.AnimationPlay ? "Pause" : "Play"))
        {
            if (!state.AnimationPlay && state.ClipLoopMode == MshClipLoopMode.Once && state.ClipProgress >= 1.0f)
                state.ClipProgress = 0.0f;

            state.AnimationPlay = !state.AnimationPlay;
        }

        ImGui.SameLine();
        var loopIndex = (int)state.ClipLoopMode;
        ImGui.SetNextItemWidth(90);
        if (ImGui.Combo("##loop", ref loopIndex, LoopModeNames, LoopModeNames.Length))
        {
            state.ClipLoopMode = (MshClipLoopMode)loopIndex;
            state.ClipDirection = 1;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        var speed = state.AnimationSpeed;
        if (ImGui.DragFloat("Speed", ref speed, 0.05f, -10.0f, 10.0f, "%.2f"))
            state.AnimationSpeed = speed;

        // Строка 4: прогресс.
        ImGui.SetNextItemWidth(-1);
        var progress = state.ClipProgress;
        if (ImGui.SliderFloat("##progress", ref progress, 0.0f, 1.0f, "progress %.3f"))
        {
            state.ClipProgress = progress;
            state.AnimationPlay = false;
            if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        if (!ImGui.TreeNode("Details"))
            return;

        var first = item.Segments[0];
        var geometry = document?.MshModelGeometry;

        if (first.MaxSpeed > 0.0f)
        {
            var speedValue = state.ClipMoveSpeed;
            var low = MathF.Min(first.MinSpeed, first.MaxSpeed);
            ImGui.SetNextItemWidth(160);
            if (ImGui.SliderFloat("Move speed", ref speedValue, low, first.MaxSpeed, "%.2f"))
            {
                state.ClipMoveSpeed = speedValue;
                UpdateAutoDuration(state, item, geometry);

                // В игре вес диапазона 2 следует за скоростью.
                if (item.HasTwoRanges)
                {
                    state.ClipWeight2 = MshRootMotion.Weight2AtSpeed(first, speedValue);
                    if (document != null)
                        ApplyCurrentAnimationPose(document);
                }
            }
        }

        var autoDuration = state.ClipAutoDuration;
        if (ImGui.Checkbox("Auto duration", ref autoDuration))
        {
            state.ClipAutoDuration = autoDuration;
            UpdateAutoDuration(state, item, geometry);
        }

        var duration = state.ClipDurationSeconds;
        ImGui.SetNextItemWidth(120);
        if (ImGui.InputFloat("Duration, s", ref duration, 0.01f, 0.1f, "%.3f"))
        {
            state.ClipDurationSeconds = MathF.Max(0.01f, duration);
            state.ClipAutoDuration = false;
        }

        if (item.HasTwoRanges)
        {
            var weight = state.ClipWeight2;
            ImGui.SetNextItemWidth(120);
            if (ImGui.SliderFloat("Weight of range 2", ref weight, 0.0f, 1.0f))
            {
                state.ClipWeight2 = weight;
                if (document != null)
                    ApplyCurrentAnimationPose(document);
            }

            ImGui.TextDisabled(
                $"Range 1: {first.Anim1Start:0.##}..{first.Anim1End:0.##}; in game the weight of range 2 grows from {first.Weight2AtMinSpeed:0.##} to 1 with speed");
        }

        var firstSegment = item.Segments[0];
        ImGui.TextDisabled(item.Segments.Count == 1
            ? $"State #{firstSegment.StateIndex} | flags 0x{firstSegment.Flags:X} | " +
              (firstSegment.HasFixedDuration ? $"duration in state {firstSegment.DurationMs:0} ms" : "duration = root motion / speed (as in the game)")
            : $"{item.Segments.Count} states in a row | timeline 0..{maxAnimationTime:0}");

        ImGui.TextDisabled($"CTL: {state.ClipSourceName}");
        ImGui.TreePop();
    }

    private void DrawChannelControls(MeshDocument? document, MeshRenderState state)
    {
        var channels = state.ClipSet.Channels;
        if (channels.Count == 0 || state.ChannelValues.Length != channels.Count)
        {
            ImGui.TextDisabled(state.ClipSourceName == null
                ? "CTL not found next to the MSH: use \"Load CTL...\"."
                : $"{state.ClipSourceName}: no piece channels with an animation range.");
            return;
        }

        var animate = state.AnimationPlay;
        if (ImGui.Checkbox("Animate", ref animate))
        {
            state.AnimationPlay = animate;
            if (!animate && document != null)
                ApplyCurrentAnimationPose(document);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70);
        var speed = state.AnimationSpeed;
        if (ImGui.DragFloat("Speed", ref speed, 0.01f, -10.0f, 10.0f, "%.2f"))
            state.AnimationSpeed = speed;

        ImGui.SameLine();
        if (ImGui.Button("Defaults"))
        {
            for (var i = 0; i < channels.Count; i++)
            {
                state.ChannelValues[i] = channels[i].DefaultValue;
                state.ChannelDirections[i] = 1;
            }

            if (document != null)
                ApplyCurrentAnimationPose(document);
        }

        // Список прокручивается внутри фиксированной высоты, чтобы не раздувать панель.
        var height = ImGui.GetFrameHeightWithSpacing() * Math.Min(channels.Count, 6) + 4;
        if (ImGui.BeginChild("##channels", new Vector2(-1, height)))
        {
            for (var i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                var name = document != null && channel.PieceIndex >= 0 && channel.PieceIndex < document.Pieces.Count
                    ? document.Pieces[channel.PieceIndex].Name
                    : $"piece {channel.PieceIndex}";

                var value = state.ChannelValues[i];
                ImGui.SetNextItemWidth(-1);
                if (ImGui.SliderFloat(
                        $"##ch{i}",
                        ref value,
                        0.0f,
                        1.0f,
                        $"{name}  {channel.Start:0.##}..{channel.End:0.##}  %.2f"))
                {
                    state.ChannelValues[i] = value;
                    state.AnimationPlay = false;
                    if (document != null)
                        ApplyCurrentAnimationPose(document);
                }
            }
        }

        ImGui.EndChild();
        ImGui.TextDisabled("Each channel moves one piece along its range of the MSH timeline; the rest stay in the state pose.");
    }

    private static MshAnimationSource ChooseDefaultSource(MeshRenderState state)
    {
        if (state.ClipSet.PhaseChains.Count > 0)
            return MshAnimationSource.Chains;

        // Модели без движений (одно состояние) оживляют каналы деталей: колёса, подвески, стволы.
        if (state.ClipSet.Channels.Count > 0 && state.ClipSet.Clips.Count <= 1)
            return MshAnimationSource.Channels;

        return state.ClipSet.Clips.Count > 0 ? MshAnimationSource.Clips : MshAnimationSource.Timeline;
    }

    private static void AdvanceChannels(MeshRenderState state, float delta)
    {
        var channels = state.ClipSet.Channels;
        if (state.ChannelValues.Length != channels.Count)
            return;

        for (var i = 0; i < channels.Count; i++)
        {
            var step = channels[i].Speed * state.AnimationSpeed * delta;
            var value = state.ChannelValues[i];

            if (channels[i].Wraps)
            {
                value += step;
                value -= MathF.Floor(value);
            }
            else
            {
                value += step * state.ChannelDirections[i];
                if (value >= 1.0f)
                {
                    value = 1.0f;
                    state.ChannelDirections[i] = -1;
                }
                else if (value <= 0.0f)
                {
                    value = 0.0f;
                    state.ChannelDirections[i] = 1;
                }
            }

            state.ChannelValues[i] = value;
        }
    }

    private void ChangeSelection(MeshDocument? document, MeshRenderState state, int index)
    {
        SelectItem(state, index, document?.MshModelGeometry);
        if (document != null)
            ApplyCurrentAnimationPose(document);
    }

    private static void EnsureValidSelection(MeshRenderState state, MshModelGeometry? geometry)
    {
        var items = state.ActiveItems;
        if (items.Count == 0)
        {
            state.SelectedClipIndex = -1;
            return;
        }

        if (state.SelectedClipIndex < 0 || state.SelectedClipIndex >= items.Count)
            SelectItem(state, 0, geometry);
    }

    private static bool TryGetSelectedItem(MeshRenderState state, out MshClipSequence item)
    {
        var items = state.ActiveItems;
        if (state.SelectedClipIndex >= 0 && state.SelectedClipIndex < items.Count)
        {
            item = items[state.SelectedClipIndex];
            return true;
        }

        item = null!;
        return false;
    }

    private static void SelectItem(MeshRenderState state, int index, MshModelGeometry? geometry)
    {
        state.SelectedClipIndex = index;
        state.ClipProgress = 0.0f;
        state.ClipDirection = 1;
        var item = state.ActiveItems[index];

        var first = item.Segments[0];

        // Скорость по умолчанию - верх полосы состояния (бег, а не шаг); у стоячих состояний полосы нет.
        state.ClipMoveSpeed = first.MaxSpeed;
        state.ClipAutoDuration = true;
        UpdateAutoDuration(state, item, geometry);

        state.ClipWeight2 = item.HasTwoRanges ? MshRootMotion.Weight2AtSpeed(first, state.ClipMoveSpeed) : 1.0f;
    }

    // Длительность как в игре: заданная в состоянии либо смещение корня / скорость. Без геометрии (корня) остаётся 1 с на состояние.
    private static void UpdateAutoDuration(MeshRenderState state, MshClipSequence item, MshModelGeometry? geometry)
    {
        if (!state.ClipAutoDuration)
            return;

        state.ClipDurationSeconds = geometry != null
            ? MathF.Max(0.01f, MshRootMotion.DurationSeconds(geometry, item, state.ClipMoveSpeed))
            : item.Segments.Count;
    }

    private static void AdvanceClip(MeshRenderState state, float delta)
    {
        if (!TryGetSelectedItem(state, out _))
            return;

        var progress = state.ClipProgress + state.ClipDirection * delta * state.AnimationSpeed / state.ClipDurationSeconds;

        switch (state.ClipLoopMode)
        {
            case MshClipLoopMode.Loop:
                progress -= MathF.Floor(progress);
                break;

            case MshClipLoopMode.PingPong:
                if (progress > 1.0f)
                {
                    progress = 2.0f - progress;
                    state.ClipDirection = -1;
                }
                else if (progress < 0.0f)
                {
                    progress = -progress;
                    state.ClipDirection = 1;
                }

                progress = Math.Clamp(progress, 0.0f, 1.0f);
                break;

            default:
                if (progress >= 1.0f || progress <= 0.0f)
                {
                    progress = Math.Clamp(progress, 0.0f, 1.0f);
                    state.AnimationPlay = false;
                }

                break;
        }

        state.ClipProgress = progress;
    }

    private bool LoadClips(string ctlPath)
    {
        try
        {
            var ctl = ControlResourceReader.ReadCtlFile(ctlPath);
            var state = _viewModel.RenderState;
            state.ClipSet = MshClipLoader.FromCtl(ctl);
            state.ClipSourceName = Path.GetFileName(ctlPath);
            state.ChannelValues = state.ClipSet.Channels.Select(c => c.DefaultValue).ToArray();
            state.ChannelDirections = state.ClipSet.Channels.Select(_ => 1).ToArray();
            state.SelectedClipIndex = -1;
            state.ClipProgress = 0.0f;
            return true;
        }
        catch (Exception ex)
        {
            _viewModel.RenderState.ResetClips();
            _viewModel.RenderState.ClipSourceName = $"load failed: {ex.Message}";
            return false;
        }
    }

    private static float GetMaxAnimationTime(MeshDocument document)
    {
        var maxAnimationTime = document.MshModelGeometry?.MaxAnimationTime ?? 0;
        if (maxAnimationTime > 0)
            return maxAnimationTime;

        var keyframes = document.MshModelGeometry?.TransformKeyframes;
        return keyframes == null || keyframes.Count == 0
            ? 0.0f
            : MathF.Max(0.0f, keyframes.Max(x => x.Time));
    }

    private static float WrapAnimationTime(float time, float maxAnimationTime)
    {
        if (maxAnimationTime <= 0.0f)
            return 0.0f;

        var wrapped = time % maxAnimationTime;
        return wrapped < 0.0f ? wrapped + maxAnimationTime : wrapped;
    }

    private static void DrawViewportHelpOverlay()
    {
        var drawList = ImGui.GetWindowDrawList();

        var imageMin = ImGui.GetItemRectMin();
        var imageMax = ImGui.GetItemRectMax();

        const string helpText =
            "LMB click: select / deselect\n" +
            "MMB drag: rotate\n" +
            "RMB drag or Alt+MMB: pan\n" +
            "Mouse wheel: zoom\n" +
            "F: frame selected   Home: frame all";

        var textSize = ImGui.CalcTextSize(helpText);
        var padding = new Vector2(8.0f, 6.0f);

        var boxMin = new Vector2(
            imageMin.X + 8.0f,
            imageMax.Y - textSize.Y - padding.Y * 2.0f - 8.0f);

        var boxMax = new Vector2(
            boxMin.X + textSize.X + padding.X * 2.0f,
            boxMin.Y + textSize.Y + padding.Y * 2.0f);

        drawList.AddRectFilled(
            boxMin,
            boxMax,
            ImGui.GetColorU32(new Vector4(0.0f, 0.0f, 0.0f, 0.45f)),
            4.0f);

        drawList.AddText(
            boxMin + padding,
            ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, 0.9f)),
            helpText);
    }
}
