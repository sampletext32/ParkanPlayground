using System.Numerics;
using FxidLib;
using MaterialLib;
using NResLib;
using NResUI.Abstractions;
using Silk.NET.OpenGL;
using TexmLib;

namespace NResUI.EffectPlayer;

/// <summary>Кадр спрайта материала: текстура OpenGL, UV страницы атласа и цвет Ambient выбранной стадии.</summary>
/// <param name="Additive">
/// Материал эффектов/частиц (версия MAT0: (version >> 2) &amp; 0xF == 2, как JET*, LASER_R, GLOW_R): рисуется с аддитивным смешиванием.
/// Выведено по wanted_device_caps в LoadMaterialByName (World3D.dll); сам режим смешивания выбирает Ngi32.dll.
/// </param>
/// <param name="HasAlpha">В текстуре есть настоящий альфа-канал (не все значения 255).</param>
public readonly record struct SpriteFrame(
    IntPtr TextureId, Vector2 Uv0, Vector2 Uv1, Vector4 Tint, int StageIndex, bool Additive = false, bool HasAlpha = true);

/// <summary>Лениво открываемый NRes архив с поиском записей по имени без учёта регистра.</summary>
public sealed class NResStore
{
    private NResArchive? _archive;
    private Dictionary<string, ListMetadataItem>? _index;

    public NResStore(string path)
    {
        Path = path;
    }

    public string Path { get; }
    public string? Error { get; private set; }

    public IReadOnlyList<ListMetadataItem> Files => _archive?.Files ?? [];

    public bool Load()
    {
        if (_archive != null)
            return true;

        if (!File.Exists(Path))
        {
            Error = $"Файл не найден: {Path}";
            return false;
        }

        var result = NResParser.ReadFile(Path);
        if (result.Archive == null)
        {
            Error = result.Error ?? "Не удалось прочитать NRes";
            return false;
        }

        _archive = result.Archive;
        _index = new Dictionary<string, ListMetadataItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in _archive.Files)
            _index.TryAdd(file.FileName, file);

        return true;
    }

    public ListMetadataItem? Find(string name)
    {
        if (!Load() || string.IsNullOrWhiteSpace(name))
            return null;

        return _index!.GetValueOrDefault(name.Trim());
    }

    public byte[] Read(ListMetadataItem entry)
    {
        using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        fs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        var data = new byte[entry.FileLength];
        fs.ReadExactly(data, 0, data.Length);
        return data;
    }
}

/// <summary>
/// Читает FXID из effects.rlb, материалы из Material.lib и текстуры из Textures.lib (папка игры берётся из config.json,
/// <see cref="ConfigJson.GameBasePath"/>) и кэширует результаты. Текстуры сразу загружаются в OpenGL.
/// </summary>
public sealed class EffectAssetCache : IService
{
    private sealed record LoadedTexture(OpenGlTexture Texture, IReadOnlyList<PageItem>? Pages, bool HasAlpha);

    private readonly GL _gl;
    private readonly IConfigProvider _configProvider;

    private string _basePath = string.Empty;
    private NResStore? _effects;
    private NResStore? _materials;
    private NResStore? _textures;
    private NResStore? _sounds;
    private OpenGlTexture? _glow;
    private readonly Dictionary<string, byte[]?> _soundCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MaterialFile?> _materialCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LoadedTexture?> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _warnings = [];

    public EffectAssetCache(GL gl, IConfigProvider configProvider)
    {
        _gl = gl;
        _configProvider = configProvider;
    }

    public IReadOnlyList<string> Warnings => _warnings;

    public string GameBasePath => _configProvider.GetConfig().GameBasePath ?? string.Empty;

    /// <summary>Сбросить кэши (например, после смены пути к игре).</summary>
    public void Reset()
    {
        foreach (var texture in _textureCache.Values)
            texture?.Texture.Dispose();

        _glow?.Dispose();
        _glow = null;
        _textureCache.Clear();
        _materialCache.Clear();
        _soundCache.Clear();
        _warnings.Clear();
        _effects = null;
        _materials = null;
        _textures = null;
        _sounds = null;
        _basePath = GameBasePath;
    }

    private void EnsureStores()
    {
        if (_effects != null && string.Equals(_basePath, GameBasePath, StringComparison.Ordinal))
            return;

        Reset();
        _effects = new NResStore(Path.Combine(_basePath, "effects.rlb"));
        _materials = new NResStore(Path.Combine(_basePath, "Material.lib"));
        _textures = new NResStore(Path.Combine(_basePath, "Textures.lib"));
        _sounds = new NResStore(Path.Combine(_basePath, "sounds.lib"));
    }

    /// <summary>Имена FXID из effects.rlb (без префикса типа).</summary>
    public IReadOnlyList<string> GetEffectNames(out string? error)
    {
        EnsureStores();
        if (!_effects!.Load())
        {
            error = _effects.Error;
            return [];
        }

        error = null;
        return _effects.Files
            .Where(f => string.Equals(f.FileType, "FXID", StringComparison.Ordinal))
            .Select(f => f.FileName)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public FxidFile? LoadFxid(string name, out string? error)
    {
        EnsureStores();
        var entry = _effects!.Find(name);
        if (entry == null)
        {
            error = $"FXID '{name}' не найден";
            return null;
        }

        try
        {
            error = null;
            return FxidFile.Read(_effects.Read(entry));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    /// <summary>
    /// Белая радиальная текстура 64x64 с плавным спадом альфы от центра к краю ((1 - r)^2); тонируется цветом света.
    /// Создаётся один раз.
    /// </summary>
    public IntPtr GetGlowTextureId()
    {
        if (_glow == null)
        {
            const int size = 64;
            var rgba = new byte[size * size * 4];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x + 0.5f - size / 2f) / (size / 2f);
                    var dy = (y + 0.5f - size / 2f) / (size / 2f);
                    var d = MathF.Sqrt(dx * dx + dy * dy);
                    var a = d >= 1f ? 0f : (1f - d) * (1f - d);
                    var o = (y * size + x) * 4;
                    rgba[o] = rgba[o + 1] = rgba[o + 2] = 255;
                    rgba[o + 3] = (byte)(a * 255f);
                }
            }

            _glow = new OpenGlTexture(_gl, size, size, rgba, PixelFormat.Rgba);
            _glow.SetMinFilter(TextureMinFilter.Linear);
            _glow.SetMagFilter(TextureMagFilter.Linear);
        }

        return (IntPtr)_glow.GlTexture;
    }

    /// <summary>Содержимое WAV из sounds.lib (null, если записи нет).</summary>
    public byte[]? GetSoundBytes(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        EnsureStores();
        if (_soundCache.TryGetValue(name, out var cached))
            return cached;

        byte[]? data = null;
        var entry = _sounds!.Find(name);
        if (entry == null)
            _warnings.Add($"Звук '{name}' не найден в sounds.lib");
        else
            data = _sounds.Read(entry);

        _soundCache[name] = data;
        return data;
    }

    public MaterialFile? GetMaterial(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        EnsureStores();
        if (_materialCache.TryGetValue(name, out var cached))
            return cached;

        MaterialFile? result = null;
        var entry = _materials!.Find(name);
        if (entry == null)
        {
            _warnings.Add($"Материал '{name}' не найден в Material.lib");
        }
        else
        {
            try
            {
                using var ms = new MemoryStream(_materials.Read(entry), writable: false);
                result = MaterialParser.ReadFromStream(ms, entry.FileName, entry.ElementCount, entry.Magic1);
            }
            catch (Exception ex)
            {
                _warnings.Add($"Материал '{name}': {ex.Message}");
            }
        }

        _materialCache[name] = result;
        return result;
    }

    private LoadedTexture? GetTexture(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        EnsureStores();
        if (_textureCache.TryGetValue(name, out var cached))
            return cached;

        LoadedTexture? result = null;
        var entry = _textures!.Find(name);
        if (entry == null)
        {
            _warnings.Add($"Текстура '{name}' не найдена в Textures.lib");
        }
        else
        {
            try
            {
                using var ms = new MemoryStream(_textures.Read(entry), writable: false);
                var parsed = TexmParser.ReadFromStream(ms, entry.FileName);
                if (parsed.TexmFile == null)
                {
                    _warnings.Add($"Текстура '{name}': {parsed.Error}");
                }
                else
                {
                    var rgba = parsed.TexmFile.GetRgba32BytesFromMipmap(0, out var width, out var height, preserveAlpha: true);
                    var hasAlpha = HasAnyTransparency(rgba);
                    // GetRgba32BytesFromMipmap возвращает байты в порядке R, G, B, A.
                    var gl = new OpenGlTexture(_gl, width, height, rgba, PixelFormat.Rgba);
                    gl.SetMinFilter(TextureMinFilter.Nearest);
                    gl.SetMagFilter(TextureMagFilter.Linear);
                    result = new LoadedTexture(gl, parsed.TexmFile.Pages?.Items, hasAlpha);
                }
            }
            catch (Exception ex)
            {
                _warnings.Add($"Текстура '{name}': {ex.Message}");
            }
        }

        _textureCache[name] = result;
        return result;
    }

    /// <summary>
    /// Многие текстуры эффектов (WAVE.0, EFFECT4.0) полностью непрозрачны (нет альфа-канала) и рассчитаны на аддитивное
    /// смешивание: чёрный фон не виден. Для них холст берёт альфу из яркости (если материал не аддитивный).
    /// </summary>
    private static bool HasAnyTransparency(byte[] rgba)
    {
        for (var i = 3; i < rgba.Length; i += 4)
        {
            if (rgba[i] != 255)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Кадр спрайта материала. Если <paramref name="phase"/> задана (0..1), кадр выбирается по фазе, как в движке;
    /// иначе по времени <paramref name="timeMs"/> с начала эффекта.
    /// </summary>
    public bool TryGetSprite(string materialName, float timeMs, out SpriteFrame frame, float phase = -1f)
    {
        frame = default;
        var material = GetMaterial(materialName);
        if (material == null)
            return false;

        var stageIndex = phase >= 0f
            ? MaterialAnimator.PickStageByPhase(material, phase)
            : MaterialAnimator.PickStage(material, timeMs);
        if (stageIndex < 0)
            return false;

        var stage = material.Stages[stageIndex];
        var texture = GetTexture(stage.TextureName);
        if (texture == null)
            return false;

        var uv0 = Vector2.Zero;
        var uv1 = Vector2.One;
        var pages = texture.Pages;
        if (pages != null && stage.TextureStageIndex >= 0 && stage.TextureStageIndex < pages.Count)
        {
            var page = pages[stage.TextureStageIndex];
            float w = texture.Texture.Width;
            float h = texture.Texture.Height;
            uv0 = new Vector2(page.X / w, page.Y / h);
            uv1 = new Vector2((page.X + page.Width) / w, (page.Y + page.Height) / h);
        }

        var tint = new Vector4(stage.AmbientR, stage.AmbientG, stage.AmbientB, MathF.Min(1f, stage.AmbientA));
        frame = new SpriteFrame(
            (IntPtr)texture.Texture.GlTexture, uv0, uv1, tint, stageIndex,
            material.MaterialRenderingType == 2, texture.HasAlpha);
        return true;
    }
}
