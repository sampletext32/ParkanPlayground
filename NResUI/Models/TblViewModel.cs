using TblLib;

namespace NResUI.Models;

public class TblViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }
    public string? Path { get; set; }
    public List<TblBinding> Bindings { get; set; } = [];

    public void SetParseResult(List<TblBinding> bindings, string path)
    {
        Bindings = bindings;
        Path = path;
        Error = null;
        HasFile = true;
    }
}
