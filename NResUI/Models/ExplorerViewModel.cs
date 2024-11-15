using NResLib;

namespace NResUI.Models;

public class ExplorerViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public NResArchive? Archive { get; set; }

    public string? Path { get; set; }

    public void SetParseResult(NResParseResult result, string path)
    {
        Error = result.Error;

        if (result.Archive != null)
        {
            HasFile = true;
        }

        Archive = result.Archive;
        Path = path;
    }
}