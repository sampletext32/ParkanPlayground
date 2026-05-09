namespace MissionTmaLib;

/// <summary>Информация о клане из mission TMA.</summary>
/// <param name="ClanName">Имя клана.</param>
/// <param name="UnkInt1">Неизвестное целочисленное поле.</param>
/// <param name="X">TODO</param>
/// <param name="Y">TODO</param>
/// <param name="ClanType">Тип клана: 1 = игрок, 2 = AI, 3 = нейтральный.</param>
/// <param name="ScriptsString">TODO</param>
/// <param name="UnknownClanPartCount">Количество записей UnknownParts.</param>
/// <param name="UnknownParts">TODO</param>
/// <param name="ResearchNResPath">Путь к NRes с деревом исследований. Игра называет этот путь TreeName.</param>
/// <param name="Brains">Количество "мозгов" AI/brains.</param>
/// <param name="AlliesMapCount">Количество записей AlliesMap.</param>
/// <param name="AlliesMap">Мапа союзников: ключ = имя клана, значение всегда 0 или 1.</param>
public record ClanInfo(
    string ClanName,
    int UnkInt1,
    float X,
    float Y,
    ClanType ClanType,
    string ScriptsString,
    int UnknownClanPartCount,
    List<UnknownClanTreeInfoPart> UnknownParts,
    string ResearchNResPath,
    int Brains,
    int AlliesMapCount,
    Dictionary<string, int> AlliesMap);
