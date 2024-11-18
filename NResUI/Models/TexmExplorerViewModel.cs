using TexmLib;

namespace NResUI.Models;

public class TexmExplorerViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public TexmFile? TexmFile { get; set; }
    
    public string? Path { get; set; }
    
    public void SetParseResult(TexmParseResult result, string path)
    {
        Error = result.Error;

        if (result.TexmFile != null)
        {
            HasFile = true;
        }

        TexmFile = result.TexmFile;
        Path = path;
    }
    
}