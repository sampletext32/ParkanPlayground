using MaterialLib;
using NResLib;
using NResUI.Abstractions;
using NResUI.Rendering.Viewport;
using NResUI.Rendering.Viewport.OpenGL;
using Silk.NET.OpenGL;
using TexmLib;

namespace NResUI.Rendering.Materials;

/// <summary>Набор материалов, готовых для viewport. Ключ — material id из WEA/MSH batch.</summary>
public sealed class ViewportMaterialSet
{
    private readonly Dictionary<int, ViewportMaterial> _materialsById;

    public ViewportMaterialSet(Dictionary<int, ViewportMaterial> materialsById)
    {
        _materialsById = materialsById;
    }

    public static ViewportMaterialSet Empty { get; } = new([]);

    public ViewportMaterial? FindMaterial(int id)
    {
        return _materialsById.GetValueOrDefault(id);
    }
}

/// <summary>
/// Связывает WEA -> Material.lib -> Textures.lib -> OpenGL texture.
/// Этот слой намеренно остается в NResUI, потому что только UI знает GameBasePath и GL context.
/// </summary>
public static class ViewportMaterialResolver
{
    /// <summary>
    /// Загружает материалы для MSH. Любая ошибка резолва не фатальна: модель должна остаться видимой без текстур.
    /// </summary>
    public static ViewportMaterialSet LoadForMsh(
        GL gl,
        string mshPath,
        IConfigProvider configProvider,
        ICollection<string>? warnings = null)
    {
        var weaMatch = WeaResourceResolver.TryLoadMatchingWeaForMsh(mshPath, warnings);
        if (weaMatch == null || weaMatch.File.Materials.Count == 0)
            return ViewportMaterialSet.Empty;

        var materialLibPath = Path.Combine(configProvider.GetConfig().GameBasePath, "Material.lib");
        if (!File.Exists(materialLibPath))
        {
            warnings?.Add($"Material.lib was not found at '{materialLibPath}'.");
            return ViewportMaterialSet.Empty;
        }

        using var materialFs = new FileStream(materialLibPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var materialArchiveResult = NResParser.ReadFile(materialFs);
        if (materialArchiveResult.Archive == null)
        {
            warnings?.Add($"Failed to parse Material.lib: {materialArchiveResult.Error}");
            return ViewportMaterialSet.Empty;
        }

        var result = new Dictionary<int, ViewportMaterial>();
        foreach (var materialRef in weaMatch.File.Materials)
        {
            var texture = TryLoadMaterialTexture(
                gl,
                materialFs,
                materialArchiveResult.Archive,
                materialRef.Name,
                configProvider,
                warnings,
                out var texm);

            result[materialRef.Id] = texture != null
                ? new ViewportMaterial(materialRef.Name, texture.Value, texm)
                : new ViewportMaterial(materialRef.Name);
        }

        return new ViewportMaterialSet(result);
    }

    private static uint? TryLoadMaterialTexture(
        GL gl,
        FileStream materialFs,
        NResArchive materialArchive,
        string materialName,
        IConfigProvider configProvider,
        ICollection<string>? warnings,
        out TexmFile? texm)
    {
        texm = null;
        var materialEntry = FindEntry(materialArchive, materialName);
        if (materialEntry == null)
        {
            warnings?.Add($"Material '{materialName}' referenced by WEA was not found in Material.lib.");
            return null;
        }

        materialFs.Seek(materialEntry.OffsetInFile, SeekOrigin.Begin);
        var materialData = new byte[materialEntry.FileLength];
        materialFs.ReadExactly(materialData, 0, materialData.Length);

        using var ms = new MemoryStream(materialData, writable: false);
        var materialFile = MaterialParser.ReadFromStream(
            ms,
            materialName,
            materialEntry.ElementCount,
            materialEntry.Magic1);

        var textureName = materialFile.Stages.FirstOrDefault()?.TextureName;
        if (string.IsNullOrWhiteSpace(textureName))
            return null;

        // Текстуры живут в отдельной NRes-библиотеке, поэтому material parsing и texture parsing разделены.
        var textureLibPath = Path.Combine(configProvider.GetConfig().GameBasePath, "Textures.lib");
        if (!File.Exists(textureLibPath))
        {
            warnings?.Add($"Textures.lib was not found at '{textureLibPath}'.");
            return null;
        }

        using var textureFs = new FileStream(textureLibPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var textureArchiveResult = NResParser.ReadFile(textureFs);
        if (textureArchiveResult.Archive == null)
        {
            warnings?.Add($"Failed to parse Textures.lib: {textureArchiveResult.Error}");
            return null;
        }

        var textureEntry = FindEntry(textureArchiveResult.Archive, textureName);
        if (textureEntry == null)
        {
            warnings?.Add($"Texture '{textureName}' referenced by material '{materialName}' was not found in Textures.lib.");
            return null;
        }

        textureFs.Seek(textureEntry.OffsetInFile, SeekOrigin.Begin);
        var textureData = new byte[textureEntry.FileLength];
        textureFs.ReadExactly(textureData, 0, textureData.Length);

        using var textureMs = new MemoryStream(textureData, writable: false);
        var texmResult = TexmParser.ReadFromStream(textureMs, textureEntry.FileName);
        if (texmResult.TexmFile == null)
        {
            warnings?.Add($"Failed to parse TEXM '{textureName}': {texmResult.Error}");
            return null;
        }

        texm = texmResult.TexmFile;
        var rgba = texm.GetRgba32BytesFromMipmap(0, out var width, out var height);
        return ViewportTextureLoader.CreateRgbaTexture(gl, rgba, width, height);
    }

    private static ListMetadataItem? FindEntry(NResArchive archive, string name)
    {
        static string Normalize(string value) => value.Trim().ToLowerInvariant();

        var normalized = Normalize(name);
        return archive.Files.FirstOrDefault(x =>
            string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase) ||
            Normalize(x.FileName) == normalized);
    }
}
