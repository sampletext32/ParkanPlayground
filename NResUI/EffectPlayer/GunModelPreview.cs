using System.Numerics;
using NResUI.Abstractions;
using NResUI.Rendering.Import.Msh;
using NResUI.Rendering.Inspection;
using NResUI.Rendering.Materials;
using MshLib;
using NResUI.Rendering.Viewport;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.EffectPlayer;

/// <summary>
/// Рисует модель орудия (MESH + WEAR из guns.rlb) ортографической камерой, совпадающей с видом холста Weapon Viewer, чтобы
/// модель стояла на месте выстрела. Использует тот же рендерер, что и панель Viewport; MSH-ресурс пишется во временный файл,
/// потому что импортёр работает с файлами.
/// </summary>
public sealed class GunModelPreview : IService
{
    private readonly GL _gl;
    private readonly IWindow _window;
    private readonly WeaponCatalog _catalog;
    private readonly IConfigProvider _config;
    /// <summary>
    /// Кадры анимации материалов. У материалов орудий (R_GB50..52, R_GB16 в Material.lib) 4 стадии по 50 мс в цикле 200 мс;
    /// стадия выбирается по времени (MatManager::AnimateFrame), поэтому для каждого кадра строится своя сцена.
    /// </summary>
    public const int FrameCount = 4;
    public const float FrameDurationSec = 0.05f;

    private readonly ViewportScene[] _scenes = Enumerable.Range(0, FrameCount).Select(_ => new ViewportScene()).ToArray();
    private readonly ViewportCamera _camera = new();
    private ViewportRenderer? _renderer;

    public GunModelPreview(GL gl, IWindow window, WeaponCatalog catalog, IConfigProvider config)
    {
        _gl = gl;
        _window = window;
        _catalog = catalog;
        _config = config;
    }

    /// <summary>Матрицы деталей в пространстве объекта (MSH: X вправо, Y вперёд, Z вверх) в позе привязок по умолчанию.</summary>
    public IReadOnlyList<Matrix4x4> PieceTransforms { get; private set; } = [];

    public string? LoadedName { get; private set; }
    public string? Error { get; private set; }
    public bool HasModel => LoadedName != null && _scenes[0].Pieces.Count > 0;

    /// <summary>Загружает модель орудия по имени CTL (<c>o_gun_ba_23.ctl</c>).</summary>
    public void Load(string ctlName, IReadOnlyDictionary<int, float>? poseTimes = null)
    {
        if (string.Equals(LoadedName, ctlName, StringComparison.OrdinalIgnoreCase))
            return;

        foreach (var scene in _scenes)
            scene.ClearPieces();

        LoadedName = null;
        Error = null;
        PieceTransforms = [];

        try
        {
            var baseName = Path.GetFileNameWithoutExtension(ctlName);
            var msh = _catalog.ReadGunResource(baseName + ".msh");
            if (msh == null)
            {
                Error = $"Модель {baseName}.msh не найдена в guns.rlb";
                return;
            }

            var directory = Path.Combine(Path.GetTempPath(), "ParkanWeaponPreview");
            Directory.CreateDirectory(directory);
            var mshPath = Path.Combine(directory, baseName + ".msh");
            File.WriteAllBytes(mshPath, msh);

            var wea = _catalog.ReadGunResource(baseName + ".wea");
            var weaPath = Path.Combine(directory, baseName + ".wea");
            if (wea != null)
                File.WriteAllBytes(weaPath, wea);
            else if (File.Exists(weaPath))
                File.Delete(weaPath);

            var warnings = new List<string>();
            var result = MshMeshDocumentImporter.LoadFromFile(mshPath);
            if (result.Document == null)
            {
                Error = result.Error ?? "Не удалось разобрать модель";
                return;
            }

            for (var frame = 0; frame < FrameCount; frame++)
            {
                var materials = ViewportMaterialResolver.LoadForMsh(_gl, mshPath, _config, warnings, frame);
                var pieces = MshViewportMeshFactory.BuildViewportPieces(
                    _gl, result.Document, new MeshRenderState(), materials);
                _scenes[frame].ReplacePieces(pieces);
            }

            ApplyPose(result.Document, poseTimes);
            LoadedName = ctlName;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    /// <summary>
    /// Ставит детали по значениям привязок: деталь с привязкой берёт анимацию в момент `начало + default * (конец - начало)`,
    /// остальные - ключ по умолчанию. Результат (матрицы в пространстве объекта) нужен для положения дул CPT.
    /// </summary>
    private void ApplyPose(MeshDocument document, IReadOnlyDictionary<int, float>? poseTimes)
    {
        var geometry = document.MshModelGeometry;
        if (geometry == null)
            return;

        var nodes = geometry.Nodes.Nodes;
        var local = new Matrix4x4[nodes.Count];
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var key = node.DefaultKeyframeIndex0x08;
            MshSampledPieceTransform sampled;
            if (poseTimes != null && poseTimes.TryGetValue(i, out var time))
            {
                sampled = MshAnimationSampler.SamplePieceAnimationAtTime(
                    node, geometry.TransformKeyframes, geometry.AnimationMap, time, geometry.MaxAnimationTime);
            }
            else if (key != ushort.MaxValue && key < geometry.TransformKeyframes.Count)
            {
                sampled = MshAnimationSampler.FromKeyframe(geometry.TransformKeyframes[key]);
            }
            else
            {
                sampled = MshSampledPieceTransform.Identity;
            }

            local[i] = MshAnimationSampler.ToMatrix(sampled);
            if (node.ParentIndexOrLink == ushort.MaxValue)
                local[i].Translation = Vector3.Zero;
        }

        var world = new Matrix4x4[nodes.Count];
        var done = new bool[nodes.Count];
        Matrix4x4 Resolve(int i, int depth)
        {
            if (done[i])
                return world[i];

            var parent = nodes[i].ParentIndexOrLink;
            var m = local[i];
            if (parent != ushort.MaxValue && parent < nodes.Count && parent != i && depth < nodes.Count)
                m = local[i] * Resolve(parent, depth + 1);

            world[i] = m;
            done[i] = true;
            return m;
        }

        for (var i = 0; i < nodes.Count; i++)
            Resolve(i, 0);

        PieceTransforms = world;

        // MSH (Z вверх, Y вперёд) -> пространство viewport (Y вверх, -Z вперёд).
        var toViewport = Matrix4x4.CreateRotationX(-MathF.PI * 0.5f);
        foreach (var scene in _scenes)
        {
            foreach (var piece in scene.Pieces)
            {
                if (piece.Id >= 0 && piece.Id < world.Length)
                    piece.LocalTransform = world[piece.Id] * toViewport;
            }
        }
    }

    /// <summary>
    /// Рисует модель в текстуру. <paramref name="right"/> и <paramref name="up"/> - оси экрана в системе вида эффектов
    /// (X вперёд, Y влево, Z вверх), <paramref name="target"/> - точка в центре экрана, <paramref name="unitsHigh"/> - высота
    /// кадра в единицах, <paramref name="timeSec"/> - время для анимации материалов.
    /// </summary>
    public uint Render(
        int width, int height, Vector3 right, Vector3 up, Vector3 target, float unitsHigh, Vector3 background,
        float timeSec = 0f)
    {
        _renderer ??= new ViewportRenderer(_gl, _window);
        _renderer.ClearColor = background;

        // Вид эффектов (X вперёд, Y влево, Z вверх) -> пространство viewport (X вправо, Y вверх, -Z вперёд).
        static Vector3 ToViewport(Vector3 v) => new(-v.Y, v.Z, -v.X);

        var toCamera = ToViewport(Vector3.Cross(right, up));
        if (toCamera.LengthSquared() < 1e-8f)
            toCamera = Vector3.UnitX;

        toCamera = Vector3.Normalize(toCamera);
        _camera.PitchDegrees = MathF.Asin(Math.Clamp(toCamera.Y, -1f, 1f)) * 180f / MathF.PI;
        _camera.YawDegrees = MathF.Atan2(toCamera.X, toCamera.Z) * 180f / MathF.PI;
        _camera.Distance = 100f;
        _camera.Target = ToViewport(target);
        _camera.OrthographicHeight = MathF.Max(unitsHigh, 0.01f);

        var frame = (int)MathF.Floor(MathF.Max(timeSec, 0f) / FrameDurationSec) % FrameCount;
        return _renderer.Render(width, height, _scenes[frame], _camera);
    }
}
