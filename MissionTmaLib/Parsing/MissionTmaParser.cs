using Common;

namespace MissionTmaLib.Parsing;

public class MissionTmaParser
{
    public static MissionTmaParseResult ReadFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ReadFile(fs);
    }

    public static MissionTmaParseResult ReadFile(Stream fs)
    {
        var arealData = LoadAreals(fs);

        var clansData = LoadClans(fs);

        if (clansData is null) return new MissionTmaParseResult(null, "Не обнаружена информация о кланах");

        var gameObjectsData = LoadGameObjects(fs);

        var missionDat = new MissionTma(arealData, clansData, gameObjectsData);
        return new MissionTmaParseResult(missionDat, null);
    }

    private static ArealsFileData LoadAreals(Stream fileStream)
    {
        var unusedHeader = fileStream.ReadInt32LittleEndian();
        var arealCount = fileStream.ReadInt32LittleEndian();

        // В демо миссии нет ареалов, ровно как и в первой миссии кампании
        // Span<byte> arealBuffer = stackalloc byte[12];

        List<ArealInfo> infos = [];
        for (var i = 0; i < arealCount; i++)
        {
            // игра читает 4 байта - видимо количество
            var unknown4Bytes = fileStream.ReadInt32LittleEndian();

            var count = fileStream.ReadInt32LittleEndian();

            List<Vector3> vectors = [];
            if (0 < count)
            {
                for (var i1 = 0; i1 < count; i1++)
                {
                    // потом читает 12 байт разом (тут видимо какой-то вектор)
                    var unknownFloat1 = fileStream.ReadFloatLittleEndian();
                    var unknownFloat2 = fileStream.ReadFloatLittleEndian();
                    var unknownFloat3 = fileStream.ReadFloatLittleEndian();

                    vectors.Add(new Vector3(unknownFloat1, unknownFloat2, unknownFloat3));
                }
            }

            infos.Add(new ArealInfo(unknown4Bytes, count, vectors));
        }

        return new ArealsFileData(unusedHeader, arealCount, infos);
    }

    private static ClansFileData? LoadClans(Stream fileStream)
    {
        var clanFeatureSet = fileStream.ReadInt32LittleEndian();

        if (clanFeatureSet is <= 0 or >= 7) return null;

        var clanCount = fileStream.ReadInt32LittleEndian();

        List<ClanInfo> infos = [];
        for (var i = 0; i < clanCount; i++)
        {
            var clanName = fileStream.ReadLengthPrefixedString();
            var unkInt1 = fileStream.ReadInt32LittleEndian();
            var x = fileStream.ReadFloatLittleEndian();
            var y = fileStream.ReadFloatLittleEndian();
            var clanType = (ClanType) fileStream.ReadInt32LittleEndian();
            var scriptsString = string.Empty;
            var unknownClanPartCount = 0;
            List<UnknownClanTreeInfoPart> unknownParts = [];
            var researchNResPath = string.Empty;
            var brains = 0;
            var alliesMapCount = 0;
            Dictionary<string, int> alliesMap = [];

            if (1 < clanFeatureSet)
            {
                // MISSIONS\SCRIPTS\default
                // MISSIONS\SCRIPTS\tut1_pl
                // MISSIONS\SCRIPTS\tut1_en
                scriptsString = fileStream.ReadLengthPrefixedString();
            }

            if (2 < clanFeatureSet)
            {
                unknownClanPartCount = fileStream.ReadInt32LittleEndian();

                // тут игра читает число, затем 12 байт и ещё 2 числа

                for (var i1 = 0; i1 < unknownClanPartCount; i1++)
                {
                    unknownParts.Add(
                        new UnknownClanTreeInfoPart(
                            fileStream.ReadInt32LittleEndian(),
                            new Vector3(
                                fileStream.ReadFloatLittleEndian(),
                                fileStream.ReadFloatLittleEndian(),
                                fileStream.ReadFloatLittleEndian()
                            ),
                            fileStream.ReadFloatLittleEndian(),
                            fileStream.ReadFloatLittleEndian()
                        )
                    );
                }
            }

            if (3 < clanFeatureSet)
            {
                // MISSIONS\SCRIPTS\auto.trf
                // MISSIONS\SCRIPTS\data.trf
                // указатель на NRes файл с данными
                // может быть пустым, например у Ntrl в туториале
                researchNResPath = fileStream.ReadLengthPrefixedString();
            }

            if (4 < clanFeatureSet)
            {
                brains = fileStream.ReadInt32LittleEndian();
            }

            if (5 < clanFeatureSet)
            {
                alliesMapCount = fileStream.ReadInt32LittleEndian();

                // тут какая-то мапа 
                // в демо миссии тут 
                // player -> 1
                // player2 -> 0

                // в туториале 
                // Plr -> 1
                // Trgt -> 1
                // Enm -> 0
                // Ntrl -> 1
                for (var i1 = 0; i1 < alliesMapCount; i1++)
                {
                    var keyIdString = fileStream.ReadLengthPrefixedString();
                    // это число всегда либо 0 либо 1
                    var unkNumber = fileStream.ReadInt32LittleEndian();

                    alliesMap[keyIdString] = unkNumber;
                }
            }

            infos.Add(new ClanInfo(
                clanName,
                unkInt1,
                x,
                y,
                clanType,
                scriptsString,
                unknownClanPartCount,
                unknownParts,
                researchNResPath,
                brains,
                alliesMapCount,
                alliesMap));
        }

        var clanInfo = new ClansFileData(clanFeatureSet, clanCount, infos);

        return clanInfo;
    }

    private static GameObjectsFileData LoadGameObjects(Stream fileStream)
    {
        var gameObjectsFeatureSet = fileStream.ReadInt32LittleEndian();

        var gameObjectsCount = fileStream.ReadInt32LittleEndian();

        Span<byte> settingVal1 = stackalloc byte[4];
        Span<byte> settingVal2 = stackalloc byte[4];
        Span<byte> settingVal3 = stackalloc byte[4];

        List<GameObjectInfo> gameObjectInfos = [];

        for (var i = 0; i < gameObjectsCount; i++)
        {
            // ReadGameObjectData
            var type = (GameObjectType) fileStream.ReadInt32LittleEndian();
            var unknownFlags = fileStream.ReadInt32LittleEndian();

            // UNITS\UNITS\HERO\hero_t.dat
            var datString = fileStream.ReadLengthPrefixedString();
            var owningClanIndex = 0;
            var order = 0;
            var unknownString2 = string.Empty;
            var unknownInt4 = 0;
            var unknownInt5 = 0;
            var unknownInt6 = 0;
            var settingsData = new GameObjectSettings(0, 0, []);

            if (2 < gameObjectsFeatureSet)
            {
                owningClanIndex = fileStream.ReadInt32LittleEndian();
            }

            if (3 < gameObjectsFeatureSet)
            {
                order = fileStream.ReadInt32LittleEndian();
                if (type == GameObjectType.Building)
                {
                    order += int.MaxValue;
                }
            }

            // читает 12 байт
            var position = new Vector3(
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian()
            );

            // ещё раз читает 12 байт
            var rotation = new Vector3(
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian()
            );

            Vector3 scale;
            if (gameObjectsFeatureSet < 10)
            {
                // если фичесет меньше 10, то игра забивает вектор единицами
                scale = new Vector3(1, 1, 1);
            }
            else
            {
                // в противном случае читает ещё вектор из файла
                scale = new Vector3(
                    fileStream.ReadFloatLittleEndian(),
                    fileStream.ReadFloatLittleEndian(),
                    fileStream.ReadFloatLittleEndian()
                );
            }

            if (6 < gameObjectsFeatureSet)
            {
                // у HERO пустая строка
                unknownString2 = fileStream.ReadLengthPrefixedString();
            }

            if (7 < gameObjectsFeatureSet)
            {
                unknownInt4 = fileStream.ReadInt32LittleEndian();
            }

            if (8 < gameObjectsFeatureSet)
            {
                unknownInt5 = fileStream.ReadInt32LittleEndian();
                unknownInt6 = fileStream.ReadInt32LittleEndian();
            }

            if (5 < gameObjectsFeatureSet)
            {
                // тут игра вызывает ещё одну функцию чтения файла - видимо это настройки объекта

                var unused = fileStream.ReadInt32LittleEndian();

                var innerCount = fileStream.ReadInt32LittleEndian();

                List<GameObjectSetting> settings = [];
                for (var i1 = 0; i1 < innerCount; i1++)
                {
                    // судя по всему это тип настройки
                    // 0 - float, 1 - int, 2?
                    var settingType = fileStream.ReadInt32LittleEndian();

                    settingVal1.Clear();
                    settingVal2.Clear();
                    settingVal3.Clear();
                    fileStream.ReadExactly(settingVal1);
                    fileStream.ReadExactly(settingVal2);
                    fileStream.ReadExactly(settingVal3);

                    IntFloatValue val1;
                    IntFloatValue val2;
                    IntFloatValue val3;

                    if (settingType == 0)
                    {
                        // float
                        val1 = new IntFloatValue(settingVal1);
                        val2 = new IntFloatValue(settingVal2);
                        val3 = new IntFloatValue(settingVal3);
                        // var innerFloat1 = fileStream.ReadFloatLittleEndian();
                        // var innerFloat2 = fileStream.ReadFloatLittleEndian();
                        // судя по всему это значение настройки
                        // var innerFloat3 = fileStream.ReadFloatLittleEndian();
                    }
                    else if (settingType == 1)
                    {
                        val1 = new IntFloatValue(settingVal1);
                        val2 = new IntFloatValue(settingVal2);
                        val3 = new IntFloatValue(settingVal3);
                        // var innerInt1 = fileStream.ReadInt32LittleEndian();
                        // var innerInt2 = fileStream.ReadInt32LittleEndian();
                        // судя по всему это значение настройки
                        // var innerInt3 = fileStream.ReadInt32LittleEndian();
                    }
                    else
                    {
                        throw new InvalidOperationException("Settings value type is not float or int");
                    }

                    // Invulnerability
                    // Life state
                    // LogicalID
                    // ClanID
                    // Type
                    // MaxSpeedPercent
                    // MaximumOre
                    // CurrentOre
                    var name = fileStream.ReadLengthPrefixedString();

                    settings.Add(
                        new GameObjectSetting(
                            settingType,
                            val1,
                            val2,
                            val3,
                            name
                        )
                    );
                }

                settingsData = new GameObjectSettings(unused, innerCount, settings);
            }

            gameObjectInfos.Add(new GameObjectInfo(
                type,
                unknownFlags,
                datString,
                owningClanIndex,
                order,
                position,
                rotation,
                scale,
                unknownString2,
                unknownInt4,
                unknownInt5,
                unknownInt6,
                settingsData));

            // end ReadGameObjectData
        }

        // DATA\MAPS\KM_2\land
        // DATA\MAPS\SC_3\land
        var landString = fileStream.ReadLengthPrefixedString();

        int unkInt7 = 0;
        string? missionTechDescription = null;
        if (1 < gameObjectsFeatureSet)
        {
            unkInt7 = fileStream.ReadInt32LittleEndian();

            // ? - байт cd 

            // Mission??????????trm\Is.\Ir
            // Skirmish 1. Full Base, One opponent?????
            // New mission?????????????????M
            missionTechDescription = fileStream.ReadLengthPrefixedString();
        }

        LodeData? lodeData = null;
        if (4 < gameObjectsFeatureSet)
        {
            var unused = fileStream.ReadInt32LittleEndian();

            var lodeCount = fileStream.ReadInt32LittleEndian();

            List<LodeInfo> lodeInfos = [];
            for (var i1 = 0; i1 < lodeCount; i1++)
            {
                var unkLodeVector = new Vector3(
                    fileStream.ReadFloatLittleEndian(),
                    fileStream.ReadFloatLittleEndian(),
                    fileStream.ReadFloatLittleEndian()
                );

                var unkLodeInt1 = fileStream.ReadInt32LittleEndian();
                var unkLodeFlags2 = fileStream.ReadInt32LittleEndian();
                var unkLodeFloat3 = fileStream.ReadFloatLittleEndian();
                var unkLodeInt4 = fileStream.ReadInt32LittleEndian();

                lodeInfos.Add(
                    new LodeInfo(
                        unkLodeVector,
                        unkLodeInt1,
                        unkLodeFlags2,
                        unkLodeFloat3,
                        unkLodeInt4
                    )
                );
            }

            lodeData = new LodeData(unused, lodeCount, lodeInfos);
        }

        return new GameObjectsFileData(
            gameObjectsFeatureSet,
            gameObjectsCount,
            gameObjectInfos,
            landString,
            unkInt7,
            missionTechDescription,
            lodeData
        );
    }
}
