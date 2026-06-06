using MaterialLib;
using NResLib;
using NResUI.Rendering.Viewport.OpenGL;
using Silk.NET.OpenGL;
using TexmLib;

namespace NResUI.Rendering.Viewport.Msh;

public sealed class WeaMaterialLibrary
{
    private readonly Dictionary<int, ViewportMaterial> _materialsById;

    private WeaMaterialLibrary(Dictionary<int, ViewportMaterial> materialsById)
    {
        _materialsById = materialsById;
    }

    public static WeaMaterialLibrary Empty { get; } = new(new Dictionary<int, ViewportMaterial>());

    public ViewportMaterial? FindMaterial(int id)
    {
        return _materialsById.TryGetValue(id, out var material)
            ? material
            : null;
    }

    public static WeaMaterialLibrary TryLoadForMsh(GL gl, string mshPath)
    {
        var weaPath = FindMatchingWeaPath(mshPath);
        if (!File.Exists(weaPath))
            return Empty;

        var materialRefs = ParseMaterialRefs(weaPath);
        if (materialRefs.Count == 0)
            return Empty;

        // TODO: Hardcoded for now
        var materialLibFs = "C:\\IronStrategy\\Material.lib";

        using var materialFs = new FileStream(materialLibFs, FileMode.Open, FileAccess.Read, FileShare.Read);
        var parseResult = NResParser.ReadFile(materialFs);
        if (parseResult.Archive == null)
            return Empty;

        var result = new Dictionary<int, ViewportMaterial>();

        foreach (var materialRef in materialRefs)
        {
            var texture = TryLoadMaterialTexture(gl, materialFs, parseResult.Archive, materialRef.Name, out var texm);
            result[materialRef.Id] = texture != null
                ? new ViewportMaterial(materialRef.Name, texture.Value, texm)
                : new ViewportMaterial(materialRef.Name);
        }

        return new WeaMaterialLibrary(result);
    }

    private static string? FindMatchingWeaPath(string mshPath)
    {
        var directory = Path.GetDirectoryName(mshPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return null;

        var mshResourceName = GetExportedResourceNameWithoutExtension(mshPath);
        var mshObjectStem = ExtractObjectStem(mshResourceName);

        var candidates = Directory.EnumerateFiles(directory, "*.wea", SearchOption.TopDirectoryOnly)
            .Select(path => new
            {
                Path = path,
                ResourceName = GetExportedResourceNameWithoutExtension(path),
            })
            .Select(x => new
            {
                x.Path,
                x.ResourceName,
                ObjectStem = ExtractObjectStem(x.ResourceName),
            })
            .ToList();

        // Best case: exact resource stem match after type prefix normalization.
        var exact = candidates.FirstOrDefault(x => string.Equals(x.ObjectStem, mshObjectStem,
            StringComparison.OrdinalIgnoreCase));

        if (exact != null)
            return exact.Path;

        // Fallback: choose a WEA whose resource name shares the longest token prefix.
        var best = candidates
            .Select(x => new
            {
                x.Path,
                Score = CommonPrefixTokenCount(
                    SplitResourceTokens(mshObjectStem),
                    SplitResourceTokens(x.ObjectStem))
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return best?.Score > 0 ? best.Path : null;
    }

    private static string GetExportedResourceNameWithoutExtension(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);

        // Exported files are usually like:
        // 58_MESH_o_tur_ba_06
        // 81_WEAR_o_tur_ba_02
        //
        // Strip numeric export prefix.
        var firstUnderscore = name.IndexOf('_');
        if (firstUnderscore > 0 &&
            name[..firstUnderscore].All(char.IsDigit))
        {
            name = name[(firstUnderscore + 1)..];
        }

        return name;
    }

    private static string ExtractObjectStem(string resourceName)
    {
        // Normalize known resource kind prefixes.
        // MESH_o_tur_ba_06 -> o_tur_ba
        // WEAR_o_tur_ba_02 -> o_tur_ba
        var normalized = resourceName;

        foreach (var prefix in new[] { "MESH_", "WEAR_", "TEXT_", "ANIM_" })
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[prefix.Length..];
                break;
            }
        }

        var tokens = normalized.Split('_', StringSplitOptions.RemoveEmptyEntries);

        return string.Join('_', tokens);
    }

    private static string[] SplitResourceTokens(string value)
    {
        return value.Split('_', StringSplitOptions.RemoveEmptyEntries);
    }

    private static int CommonPrefixTokenCount(string[] a, string[] b)
    {
        var count = Math.Min(a.Length, b.Length);
        var result = 0;

        for (var i = 0; i < count; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                break;

            result++;
        }

        return result;
    }

    private static IReadOnlyList<WeaMaterialRef> ParseMaterialRefs(string weaPath)
    {
        var lines = File.ReadAllLines(weaPath)
            .Select(x => x.Trim())
            .Where(x => x.Length != 0)
            .ToList();

        if (lines.Count == 0 || !int.TryParse(lines[0], out var materialCount) || materialCount <= 0)
            return [];

        var result = new List<WeaMaterialRef>();

        for (var i = 0; i < materialCount && i + 1 < lines.Count; i++)
        {
            var line = lines[i + 1];
            if (line.Equals("LIGHTMAPS", StringComparison.OrdinalIgnoreCase))
                break;

            var parts = line.Split((char[]?)null, 2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
                continue;

            if (!int.TryParse(parts[0], out var id))
                continue;

            result.Add(new WeaMaterialRef(id, parts[1]));
        }

        return result;
    }

    private static uint? TryLoadMaterialTexture(
        GL gl, FileStream materialFs, NResArchive materialArchive, string materialName, out TexmFile? texm
    )
    {
        texm = null;
        var entry = FindMaterialEntry(materialArchive, materialName);
        if (entry == null)
            return null;

        materialFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        var materialData = new byte[entry.FileLength];
        materialFs.ReadExactly(materialData, 0, materialData.Length);

        using var ms = new MemoryStream(materialData, writable: false);

        var materialFile = MaterialParser.ReadFromStream(ms, materialName, entry.ElementCount, entry.Magic1);

        var texture = materialFile.Stages[0].TextureName;

        var textureLibFs = "C:\\IronStrategy\\Textures.lib";

        using var textureFs = new FileStream(textureLibFs, FileMode.Open, FileAccess.Read, FileShare.Read);
        var textureResult = NResParser.ReadFile(textureFs);
        if (textureResult.Archive == null)
            return 0;

        var textureEntry = FindTextureEntry(textureResult.Archive, texture);
        if (textureEntry is null)
        {
            Console.WriteLine($"Didnt find texture {texture}");
            return 0;
        }
        
        textureFs.Seek(textureEntry.OffsetInFile, SeekOrigin.Begin);
        var textureData = new byte[textureEntry.FileLength];
        textureFs.ReadExactly(textureData, 0, textureData.Length);

        using var textureMs = new MemoryStream(textureData, writable: false);
        
        var texmResult = TexmParser.ReadFromStream(textureMs, entry.FileName);
        if (texmResult.TexmFile == null)
            return null;

        texm = texmResult.TexmFile;
        var rgba = texmResult.TexmFile.GetRgba32BytesFromMipmap(0, out var width, out var height);
        return ViewportTextureLoader.CreateRgbaTexture(gl, rgba, width, height);
    }

    private static ListMetadataItem? FindMaterialEntry(NResArchive materialArchive, string materialName)
    {
        static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        var normalized = Normalize(materialName);

        return materialArchive.Files.FirstOrDefault(x =>
            string.Equals(x.FileName, materialName, StringComparison.OrdinalIgnoreCase) ||
            Normalize(x.FileName) == normalized);
    }

    private static ListMetadataItem? FindTextureEntry(NResArchive archive, string name)
    {
        static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        var normalized = Normalize(name);

        return archive.Files.FirstOrDefault(x =>
            string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase) ||
            Normalize(x.FileName) == normalized);
    }

    private static string? FindTexturesLib(string? startDirectory)
    {
        if (string.IsNullOrWhiteSpace(startDirectory))
            return null;

        var directory = new DirectoryInfo(startDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "Textures.lib");
            if (Directory.Exists(candidate))
                return candidate;

            candidate = Path.Combine(directory.FullName, "textures.lib");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
    }

    private readonly record struct WeaMaterialRef(int Id, string Name);
}
