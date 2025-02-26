using ScrLib;

namespace NResUI.Models;

public class ScrViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public ScrFile? Scr { get; set; }

    public string? Path { get; set; }

    public void SetParseResult(ScrFile scrFile, string path)
    {
        Scr = scrFile;
        HasFile = true;
        Path = path;
    }
}