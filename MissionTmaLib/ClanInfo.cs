namespace MissionTmaLib;

/// <summary>Информация о клане из mission TMA.</summary>
/// <param name="ClanName">Имя клана.</param>
/// <param name="Reserved">Резерв: игра читает поле, но кладёт в структуру -1 (+0x38).</param>
/// <param name="X">X позиции клана (pos_x в ClanInfo).</param>
/// <param name="Y">Y позиции клана (pos_y в ClanInfo).</param>
/// <param name="ClanType">Тип клана: 1 = игрок, 2 = AI, 3 = нейтральный.</param>
/// <param name="ScriptsString">Путь к папке скриптов клана.</param>
/// <param name="MigrationPlaceCount">Количество записей MigrationPlaces.</param>
/// <param name="MigrationPlaces">Места миграции клана (ISystemArealMap.CreateMigrationArealsForClan).</param>
/// <param name="ResearchNResPath">Путь к NRes с деревом исследований. Игра называет этот путь TreeName.</param>
/// <param name="Brains">Количество "мозгов" AI/brains.</param>
/// <param name="AlliesMapCount">Количество записей AlliesMap.</param>
/// <param name="AlliesMap">Отношение к другим кланам: ключ = имя клана, значение 0 или 1. Игра строит из этого матрицу
/// отношений: 0 - враг, 1 - союзник, 2 - свой клан; не перечисленные кланы считаются нейтральными (как 1).</param>
public record ClanInfo(
    string ClanName,
    int Reserved,
    float X,
    float Y,
    ClanType ClanType,
    string ScriptsString,
    int MigrationPlaceCount,
    List<MigrationPlace> MigrationPlaces,
    string ResearchNResPath,
    int Brains,
    int AlliesMapCount,
    Dictionary<string, int> AlliesMap);
