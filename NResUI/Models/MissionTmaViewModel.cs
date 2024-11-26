using MissionTmaLib.Parsing;

namespace NResUI.Models;

public class MissionTmaViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public MissionTma? Mission { get; set; }

    public string? Path { get; set; }

    public void SetParseResult(MissionTmaParseResult result, string path)
    {
        Error = result.Error;

        if (result.Mission != null)
        {
            HasFile = true;
        }

        Mission = result.Mission;
        Path = path;
    }
}