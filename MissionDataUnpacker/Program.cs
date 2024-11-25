using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Autodemo.00\\data.tma";
// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Tutorial.01\\data.tma";
var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\CAMPAIGN\\CAMPAIGN.01\\Mission.02\\data.tma";
// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Single.01\\data.tma";
var fs = new FileStream(missionFilePath, FileMode.Open);

var arealData = LoadAreals(fs);

var clansData = LoadClans(fs);

var gameObjectsData = LoadGameObjects(fs);

_ = 5;

ArealsFileData LoadAreals(FileStream fileStream)
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

ClansFileData? LoadClans(FileStream fileStream)
{
    var clanFeatureSet = fileStream.ReadInt32LittleEndian();

    if (clanFeatureSet is <= 0 or >= 7) return null;

    var treeInfoCount = fileStream.ReadInt32LittleEndian();

    List<ClanInfo> infos = [];
    for (var i = 0; i < treeInfoCount; i++)
    {
        var clanTreeInfo = new ClanInfo();

        clanTreeInfo.ClanName = fileStream.ReadLengthPrefixedString();
        clanTreeInfo.UnkInt1 = fileStream.ReadInt32LittleEndian();
        clanTreeInfo.X = fileStream.ReadFloatLittleEndian();
        clanTreeInfo.Y = fileStream.ReadFloatLittleEndian();
        clanTreeInfo.ClanType = (ClanType)fileStream.ReadInt32LittleEndian();

        if (1 < clanFeatureSet)
        {
            // MISSIONS\SCRIPTS\default
            // MISSIONS\SCRIPTS\tut1_pl
            // MISSIONS\SCRIPTS\tut1_en
            clanTreeInfo.UnkString2 = fileStream.ReadLengthPrefixedString();
        }

        if (2 < clanFeatureSet)
        {
            clanTreeInfo.UnknownClanPartCount = fileStream.ReadInt32LittleEndian();

            // тут игра читает число, затем 12 байт и ещё 2 числа

            List<UnknownClanTreeInfoPart> unknownClanTreeInfoParts = [];
            for (var i1 = 0; i1 < clanTreeInfo.UnknownClanPartCount; i1++)
            {
                unknownClanTreeInfoParts.Add(
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

            clanTreeInfo.UnknownParts = unknownClanTreeInfoParts;
        }

        if (3 < clanFeatureSet)
        {
            // MISSIONS\SCRIPTS\auto.trf
            // MISSIONS\SCRIPTS\data.trf
            // указатель на NRes файл с данными
            // может быть пустым, например у Ntrl в туториале
            clanTreeInfo.ResearchNResPath = fileStream.ReadLengthPrefixedString();
        }

        if (4 < clanFeatureSet)
        {
            clanTreeInfo.UnkInt3 = fileStream.ReadInt32LittleEndian();
        }

        if (5 < clanFeatureSet)
        {
            clanTreeInfo.AlliesMapCount = fileStream.ReadInt32LittleEndian();

            // тут какая-то мапа 
            // в демо миссии тут 
            // player -> 1
            // player2 -> 0

            // в туториале 
            // Plr -> 1
            // Trgt -> 1
            // Enm -> 0
            // Ntrl -> 1
            Dictionary<string, int> map = [];
            for (var i1 = 0; i1 < clanTreeInfo.AlliesMapCount; i1++)
            {
                var keyIdString = fileStream.ReadLengthPrefixedString();
                // это число всегда либо 0 либо 1
                var unkNumber = fileStream.ReadInt32LittleEndian();

                map[keyIdString] = unkNumber;
            }

            clanTreeInfo.AlliesMap = map;
        }

        infos.Add(clanTreeInfo);
    }

    var clanInfo = new ClansFileData(clanFeatureSet, treeInfoCount, infos);

    return clanInfo;
}

GameObjectsFileData LoadGameObjects(FileStream fileStream)
{
    var gameObjectsFeatureSet = fileStream.ReadInt32LittleEndian();

    var gameObjectsCount = fileStream.ReadInt32LittleEndian();

    Span<byte> settingVal1 = stackalloc byte[4];
    Span<byte> settingVal2 = stackalloc byte[4];
    Span<byte> settingVal3 = stackalloc byte[4];

    List<GameObjectInfo> gameObjectInfos = [];

    for (var i = 0; i < gameObjectsCount; i++)
    {
        var gameObjectInfo = new GameObjectInfo();
        // ReadGameObjectData
        gameObjectInfo.Type = (GameObjectType)fileStream.ReadInt32LittleEndian();
        gameObjectInfo.UnknownFlags = fileStream.ReadInt32LittleEndian();

        // UNITS\UNITS\HERO\hero_t.dat
        gameObjectInfo.DatString = fileStream.ReadLengthPrefixedString();

        if (2 < gameObjectsFeatureSet)
        {
            gameObjectInfo.OwningClanIndex = fileStream.ReadInt32LittleEndian();
        }

        if (3 < gameObjectsFeatureSet)
        {
            gameObjectInfo.UnknownInt3 = fileStream.ReadInt32LittleEndian();
        }

        // читает 12 байт
        gameObjectInfo.Position = new Vector3(
            fileStream.ReadFloatLittleEndian(),
            fileStream.ReadFloatLittleEndian(),
            fileStream.ReadFloatLittleEndian()
        );

        // ещё раз читает 12 байт
        gameObjectInfo.Rotation = new Vector3(
            fileStream.ReadFloatLittleEndian(),
            fileStream.ReadFloatLittleEndian(),
            fileStream.ReadFloatLittleEndian()
        );

        if (gameObjectsFeatureSet < 10)
        {
            // если фичесет меньше 10, то игра забивает вектор единицами
            gameObjectInfo.Scale = new Vector3(1, 1, 1);
        }
        else
        {
            // в противном случае читает ещё вектор из файла
            gameObjectInfo.Scale = new Vector3(
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian(),
                fileStream.ReadFloatLittleEndian()
            );
        }

        if (6 < gameObjectsFeatureSet)
        {
            // у HERO пустая строка
            gameObjectInfo.UnknownString2 = fileStream.ReadLengthPrefixedString();
        }

        if (7 < gameObjectsFeatureSet)
        {
            gameObjectInfo.UnknownInt4 = fileStream.ReadInt32LittleEndian();
        }

        if (8 < gameObjectsFeatureSet)
        {
            gameObjectInfo.UnknownInt5 = fileStream.ReadInt32LittleEndian();
            gameObjectInfo.UnknownInt6 = fileStream.ReadInt32LittleEndian();
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

                settings.Add(new GameObjectSetting(settingType, val1, val2, val3, name));
            }

            gameObjectInfo.Settings = new GameObjectSettings(unused, innerCount, settings);
        }
        
        gameObjectInfos.Add(gameObjectInfo);

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

public static class Extensions
{
    public static int ReadInt32LittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadInt32LittleEndian(buf);
    }

    public static float ReadFloatLittleEndian(this FileStream fs)
    {
        Span<byte> buf = stackalloc byte[4];
        fs.ReadExactly(buf);

        return BinaryPrimitives.ReadSingleLittleEndian(buf);
    }

    public static string ReadLengthPrefixedString(this FileStream fs)
    {
        var len = fs.ReadInt32LittleEndian();

        if (len == 0)
        {
            return "";
        }

        var buffer = new byte[len];

        fs.ReadExactly(buffer, 0, len);

        return Encoding.ASCII.GetString(buffer, 0, len);
    }
}

public record ArealsFileData(int UnusedHeader, int ArealCount, List<ArealInfo> ArealInfos);

public record ArealInfo(int Index, int CoordsCount, List<Vector3> Coords);

public record Vector3(float X, float Y, float Z);

// ----

public record ClansFileData(int ClanFeatureSet, int ClanCount, List<ClanInfo> ClanInfos);

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

    public string UnkString2 { get; set; }
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

public record UnknownClanTreeInfoPart(int UnkInt1, Vector3 UnkVector, float UnkInt2, float UnkInt3);

[DebuggerDisplay("AsInt = {AsInt}, AsFloat = {AsFloat}")]
public class IntFloatValue(Span<byte> span)
{
    public int AsInt { get; set; } = MemoryMarshal.Read<int>(span);
    public float AsFloat { get; set; } = MemoryMarshal.Read<float>(span);
}

public record GameObjectsFileData(int GameObjectsFeatureSet, int GameObjectsCount, List<GameObjectInfo> GameObjectInfos, string LandString, int UnknownInt, string? MissionTechDescription, LodeData? LodeData);

public class GameObjectInfo
{
    // 0 - здание, 1 - бот, 2 - окружение
    public GameObjectType Type { get; set; }

    public int UnknownFlags { get; set; }

    public string DatString { get; set; }

    /// <summary>
    /// Индекс клана, которому принадлежит объект
    /// </summary>
    /// <remarks>
    /// <para>
    /// Некоторые объекты окружения иногда почему-то принадлежат клану отличному от -1
    /// </para>
    /// <para>
    /// Может быть -1, если объект никому не принадлежит, я такое встречал только у объектов окружения
    /// </para>
    /// </remarks>
    public int OwningClanIndex { get; set; }

    public int UnknownInt3 { get; set; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public string UnknownString2 { get; set; }

    public int UnknownInt4 { get; set; }
    public int UnknownInt5 { get; set; }
    public int UnknownInt6 { get; set; }

    public GameObjectSettings Settings { get; set; }
}

public record GameObjectSettings(int Unused, int SettingsCount, List<GameObjectSetting> Settings);

public record GameObjectSetting(int SettingType, IntFloatValue Unk1, IntFloatValue Unk2, IntFloatValue Unk3, string Name);

public record LodeData(int Unused, int LodeCount, List<LodeInfo> Lodes);

public record LodeInfo(Vector3 UnknownVector, int UnknownInt1, int UnknownInt2, float UnknownFloat, int UnknownInt3);

public enum GameObjectType
{
    Building = 0,
    Warbot = 1,
    Tree = 2,
    Stone = 3
}

public enum ClanType
{
    Environment = 0,
    Player = 1,
    AI = 2,
    Neutral = 3
}