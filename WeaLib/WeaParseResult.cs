namespace WeaLib;

public sealed record WeaParseResult(WeaFile? WeaFile = null, string? Error = null)
{
    public bool IsSuccess => WeaFile != null;
}
