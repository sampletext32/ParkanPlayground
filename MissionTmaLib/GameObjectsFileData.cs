namespace MissionTmaLib;

public record GameObjectsFileData(int GameObjectsFeatureSet, int GameObjectsCount, List<GameObjectInfo> GameObjectInfos, string LandString, int UnknownInt, string? MissionTechDescription, LodeData? LodeData);