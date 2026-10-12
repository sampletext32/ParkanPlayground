using ScrLib;

namespace NResUI.Models;

public class ScrViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public ScrFile? Scr { get; set; }

    public string? Path { get; set; }

    /// <summary>Формулы одноимённого <c>.fml</c> рядом с <c>.scr</c>; null, если файла нет или он не разобран.</summary>
    public FmlFile? Fml { get; set; }

    public void SetParseResult(ScrFile scrFile, string path)
    {
        Scr = scrFile;
        HasFile = true;
        Path = path;

        Fml = null;
        var fmlPath = System.IO.Path.ChangeExtension(path, ".fml");
        if (File.Exists(fmlPath))
        {
            try
            {
                Fml = FmlParser.ReadFile(fmlPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось прочитать {fmlPath}: {e.Message}");
            }
        }
    }
}