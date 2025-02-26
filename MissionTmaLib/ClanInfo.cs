namespace MissionTmaLib;

public class ClanInfo
{
    public string ClanName { get; set; }
    public int UnkInt1 { get; set; }
    public float X { get; set; }
    public float Y { get; set; }

    /// <summary>
    /// 1 - игрок, 2 AI, 3 - нейтральный
    /// </summary>
    public ClanType ClanType { get; set; }

    public string ScriptsString { get; set; }
    public int UnknownClanPartCount { get; set; }
    public List<UnknownClanTreeInfoPart> UnknownParts { get; set; }
    
    /// <summary>
    /// Игра называет этот путь TreeName
    /// </summary>
    public string ResearchNResPath { get; set; }
    public int UnkInt3 { get; set; }
    public int AlliesMapCount { get; set; }

    /// <summary>
    /// мапа союзников (ключ - имя клана, значение - число, всегда либо 0 либо 1)
    /// </summary>
    public Dictionary<string, int> AlliesMap { get; set; }
}