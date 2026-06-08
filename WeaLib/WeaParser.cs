namespace WeaLib;

/// <summary>
/// Парсер текстовых `.wea` таблиц. Библиотека не знает про Material.lib, Textures.lib и OpenGL:
/// она только возвращает id/name строки из файла.
/// </summary>
public static class WeaParser
{
    /// <summary>Читает `.wea` с диска. Ошибки IO возвращаются как WeaParseResult.Error.</summary>
    public static WeaParseResult ReadFile(string path)
    {
        try
        {
            return ReadLines(File.ReadLines(path), path);
        }
        catch (Exception ex)
        {
            return new WeaParseResult(Error: ex.Message);
        }
    }

    /// <summary>Читает `.wea` из stream, оставляя stream открытым для вызывающего кода.</summary>
    public static WeaParseResult ReadFromStream(Stream stream, string fileName)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        return ReadLines(ReadAllLines(reader), fileName);
    }

    /// <summary>Удобный вход для тестов и анализа маленьких WEA-фрагментов.</summary>
    public static WeaParseResult ReadText(string text, string fileName = "<memory>")
    {
        return ReadLines(text.Split(["\r\n", "\n"], StringSplitOptions.None), fileName);
    }

    private static WeaParseResult ReadLines(IEnumerable<string> rawLines, string fileName)
    {
        // Пустые строки в реальных WEA не несут смысла, поэтому игнорируем их до валидации counts.
        var lines = rawLines
            .Select(x => x.Trim())
            .Where(x => x.Length != 0)
            .ToList();

        if (lines.Count == 0)
            return new WeaParseResult(Error: "WEA file is empty.");

        if (!int.TryParse(lines[0], out var materialCount) || materialCount < 0)
            return new WeaParseResult(Error: "WEA material count is missing or invalid.");

        var materials = new List<WeaMaterialRef>(materialCount);
        var cursor = 1;

        for (var i = 0; i < materialCount; i++, cursor++)
        {
            if (cursor >= lines.Count)
                return new WeaParseResult(Error: $"WEA expected {materialCount} material rows, but found {i}.");

            if (IsLightmapsMarker(lines[cursor]))
                return new WeaParseResult(Error: $"WEA expected material row {i}, but found LIGHTMAPS.");

            if (!TryParseRef(lines[cursor], out var id, out var name))
                return new WeaParseResult(Error: $"WEA material row {i} is malformed: '{lines[cursor]}'.");

            materials.Add(new WeaMaterialRef(id, name));
        }

        var lightmaps = new List<WeaLightmapRef>();
        if (cursor < lines.Count)
        {
            if (!IsLightmapsMarker(lines[cursor]))
                return new WeaParseResult(Error: $"WEA unexpected content after material rows: '{lines[cursor]}'.");

            // LIGHTMAPS — отдельная секция со своим count; без count секция считается битой.
            cursor++;
            if (cursor >= lines.Count || !int.TryParse(lines[cursor], out var lightmapCount) || lightmapCount < 0)
                return new WeaParseResult(Error: "WEA LIGHTMAPS count is missing or invalid.");

            cursor++;
            for (var i = 0; i < lightmapCount; i++, cursor++)
            {
                if (cursor >= lines.Count)
                    return new WeaParseResult(Error: $"WEA expected {lightmapCount} lightmap rows, but found {i}.");

                if (!TryParseRef(lines[cursor], out var id, out var name))
                    return new WeaParseResult(Error: $"WEA lightmap row {i} is malformed: '{lines[cursor]}'.");

                lightmaps.Add(new WeaLightmapRef(id, name));
            }
        }

        return new WeaParseResult(new WeaFile(fileName, materials, lightmaps));
    }

    private static IEnumerable<string> ReadAllLines(TextReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    private static bool IsLightmapsMarker(string line)
    {
        return line.Equals("LIGHTMAPS", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseRef(string line, out int id, out string name)
    {
        id = 0;
        name = string.Empty;

        var parts = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out id))
            return false;

        name = parts[1];
        return name.Length != 0;
    }
}
