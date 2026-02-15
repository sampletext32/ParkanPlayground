using ResTreeLib;

namespace NResUI.Models;

public class ResearchTreeViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public List<ResearchNodeData>? ResearchNodeDatas { get; set; }

    public string? Path { get; set; }

    public void SetParseResult(List<ResearchNodeData> researchNodeDatas, string path)
    {
        ResearchNodeDatas = researchNodeDatas;
        HasFile = true;
        Path = path;
    }
}
