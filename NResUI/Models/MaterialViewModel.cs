using MaterialLib;

namespace NResUI.Models;

public class MaterialViewModel
{
    public MaterialFile? MaterialFile { get; private set; }
    public string? FilePath { get; private set; }
    public string? Error { get; private set; }
    
    public bool HasFile => MaterialFile != null;

    public void SetParseResult(MaterialFile materialFile, string filePath)
    {
        MaterialFile = materialFile;
        FilePath = filePath;
        Error = null;
    }

    public void SetError(string error)
    {
        MaterialFile = null;
        FilePath = null;
        Error = error;
    }
}
