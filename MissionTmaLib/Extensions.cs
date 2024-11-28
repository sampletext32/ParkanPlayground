namespace MissionTmaLib;

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

    public static string ToReadableString(this GameObjectType type)
    {
        return type switch
        {
            GameObjectType.Building => $"Строение {type:D}",
            GameObjectType.Warbot => $"Варбот {type:D}",
            GameObjectType.Tree => $"Дерево {type:D}",
            GameObjectType.Stone => $"Камень {type:D}",
            _ => $"Неизвестный ({type:D})"
        };
    }
}