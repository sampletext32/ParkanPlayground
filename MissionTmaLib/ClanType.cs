namespace MissionTmaLib;

public enum ClanType
{
    Environment = 0,
    Player = 1,
    AI = 2,
    Neutral = 3
}

public static class Extensions
{
    public static string ToReadableString(this ClanType clanType)
    {
        return clanType switch
        {
            ClanType.Environment => $"Окружение ({clanType:D})",
            ClanType.Player => $"Игрок ({clanType:D})",
            ClanType.AI => $"AI ({clanType:D})",
            ClanType.Neutral => $"Нейтральный ({clanType:D})",
            _ => $"Неизвестный ({clanType:D})"
        };
    }
}