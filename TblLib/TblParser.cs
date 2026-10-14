using System.Globalization;

namespace TblLib;

/// <summary>Разбор текстовой таблицы привязок ввода игры.</summary>
public static class TblParser
{
    /// <summary>Читает файл таблицы.</summary>
    /// <param name="path">Путь к <c>.tbl</c>.</param>
    public static List<TblBinding> ReadFile(string path)
    {
        return Parse(File.ReadAllLines(path));
    }

    /// <summary>Разбирает строки таблицы; пустые строки и строки-комментарии пропускаются.</summary>
    /// <param name="lines">Строки файла.</param>
    /// <exception cref="FormatException">Строка не соответствует формату.</exception>
    public static List<TblBinding> Parse(IEnumerable<string> lines)
    {
        var result = new List<TblBinding>();
        var lineNumber = 0;

        foreach (var raw in lines)
        {
            lineNumber++;

            var comment = string.Empty;
            var line = raw;
            var commentStart = line.IndexOf("//", StringComparison.Ordinal);
            if (commentStart >= 0)
            {
                comment = line[(commentStart + 2)..].Trim();
                line = line[..commentStart];
            }

            var parts = line.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            if (parts.Length != 11)
            {
                throw new FormatException($"Строка {lineNumber}: ожидалось 11 полей, найдено {parts.Length}.");
            }

            var device = parts[0].ToUpperInvariant() switch
            {
                "KEY" => TblDevice.Key,
                "MOUSE" => TblDevice.Mouse,
                _ => throw new FormatException($"Строка {lineNumber}: неизвестное устройство \"{parts[0]}\".")
            };

            result.Add(new TblBinding(
                device,
                parts[1],
                parts[2],
                ParseInt(parts[3], lineNumber) != 0,
                parts[4],
                parts[5],
                ParseFloat(parts[6], lineNumber),
                ParseInt(parts[7], lineNumber),
                parts[8],
                ParseFloat(parts[9], lineNumber),
                ParseInt(parts[10], lineNumber),
                comment));
        }

        return result;
    }

    private static int ParseInt(string text, int lineNumber)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Строка {lineNumber}: \"{text}\" не целое число.");
        }

        return value;
    }

    private static float ParseFloat(string text, int lineNumber)
    {
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Строка {lineNumber}: \"{text}\" не число.");
        }

        return value;
    }
}
