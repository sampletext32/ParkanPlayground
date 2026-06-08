using WeaLib;

namespace NResUI.Rendering.Materials;

/// <summary>Результат эвристического поиска WEA рядом с MSH.</summary>
public sealed class WeaResourceMatch
{
    public required string Path { get; init; }
    public required string Reason { get; init; }
    public required WeaFile File { get; init; }
}

/// <summary>
/// UI-side resolver для связи MSH-файла с WEA-файлом.
/// Это не часть WeaLib, потому что matching зависит от экспортированных имен и расположения ресурсов на диске.
/// </summary>
public static class WeaResourceResolver
{
    /// <summary>
    /// Ищет WEA рядом с MSH и сразу парсит его.
    /// Если файл не найден или битый, viewport все равно может показать модель без материалов.
    /// </summary>
    public static WeaResourceMatch? TryLoadMatchingWeaForMsh(string mshPath, ICollection<string>? warnings = null)
    {
        var match = FindMatchingWeaPath(mshPath, out var reason);
        if (match == null)
            return null;

        var parseResult = WeaParser.ReadFile(match);
        if (parseResult.WeaFile == null)
        {
            warnings?.Add($"Failed to parse WEA '{match}': {parseResult.Error}");
            return null;
        }

        return new WeaResourceMatch
        {
            Path = match,
            Reason = reason,
            File = parseResult.WeaFile
        };
    }

    private static string? FindMatchingWeaPath(string mshPath, out string reason)
    {
        reason = "No matching WEA found.";
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

        var exact = candidates.FirstOrDefault(x => string.Equals(x.ObjectStem, mshObjectStem,
            StringComparison.OrdinalIgnoreCase));

        // Лучший случай: MESH_* и WEAR_* после нормализации указывают на один объект.
        if (exact != null)
        {
            reason = "Object stem matched exactly.";
            return exact.Path;
        }

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

        // Fallback нужен для экспортов, где суффиксы MESH/WEAR отличаются, но начало имени объекта совпадает.
        if (best?.Score > 0)
        {
            reason = $"Shared first {best.Score} resource token(s).";
            return best.Path;
        }

        return null;
    }

    private static string GetExportedResourceNameWithoutExtension(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var firstUnderscore = name.IndexOf('_');
        // Экспортер часто добавляет numeric prefix: 58_MESH_..., 81_WEAR_...
        if (firstUnderscore > 0 && name[..firstUnderscore].All(char.IsDigit))
            name = name[(firstUnderscore + 1)..];

        return name;
    }

    private static string ExtractObjectStem(string resourceName)
    {
        var normalized = resourceName;
        // Тип ресурса не является частью имени объекта, поэтому MESH_o_x и WEAR_o_x должны совпасть.
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
}
