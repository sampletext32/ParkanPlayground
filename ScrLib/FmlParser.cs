using System.Text;

namespace ScrLib;

/// <summary>Разбор текстового файла формул <c>.fml</c>.</summary>
public static class FmlParser
{
    private const string Prefix = "FUNCTION(";

    /// <summary>Читает файл формул (кодировка windows-1251).</summary>
    /// <param name="path">Путь к <c>.fml</c>.</param>
    public static FmlFile ReadFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Read(fs);
    }

    /// <summary>Читает файл формул из потока.</summary>
    /// <param name="stream">Поток с текстом <c>.fml</c> (windows-1251).</param>
    public static FmlFile Read(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = new StreamReader(stream, Encoding.GetEncoding(1251));

        var formulas = new List<FmlFormula>();
        while (reader.ReadLine() is { } rawLine)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (!line.StartsWith(Prefix, StringComparison.Ordinal) || !line.EndsWith(')'))
            {
                throw new FormatException($"Строка не похожа на FUNCTION(...): \"{line}\"");
            }

            // поля разделены запятыми; выражение запятых не содержит (проверено), поэтому делим по первой и последней
            var body = line.Substring(Prefix.Length, line.Length - Prefix.Length - 1);
            var first = body.IndexOf(',');
            var last = body.LastIndexOf(',');
            if (first < 0 || last == first)
            {
                throw new FormatException($"В FUNCTION(...) ожидалось три поля: \"{line}\"");
            }

            formulas.Add(new FmlFormula(
                formulas.Count,
                body[..first].Trim(),
                body.Substring(first + 1, last - first - 1).Trim(),
                body[(last + 1)..].Trim()));
        }

        return new FmlFile(formulas);
    }
}
