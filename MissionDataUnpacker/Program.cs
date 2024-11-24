using System.Buffers.Binary;
using System.Text;

// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Autodemo.00\\data.tma";
// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Tutorial.01\\data.tma";
var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\CAMPAIGN\\CAMPAIGN.01\\Mission.03\\data.tma";
// var missionFilePath = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\Single.01\\data.tma";
var fs = new FileStream(missionFilePath, FileMode.Open);

var arealData = LoadAreals(fs);

var clans = LoadClans(fs);

_ = 5;
// LoadGameObjects(fs);

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

    List<ClanTreeInfo> infos = [];
    for (var i = 0; i < treeInfoCount; i++)
    {
        var clanTreeInfo = new ClanTreeInfo();

        clanTreeInfo.ClanName = fileStream.ReadLengthPrefixedString();
        clanTreeInfo.UnkInt1 = fileStream.ReadInt32LittleEndian();
        clanTreeInfo.X = fileStream.ReadFloatLittleEndian();
        clanTreeInfo.Y = fileStream.ReadFloatLittleEndian();
        clanTreeInfo.ClanType = fileStream.ReadInt32LittleEndian();
            
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

        var buffer = new byte[len];

        fs.ReadExactly(buffer, 0, len);

        return Encoding.ASCII.GetString(buffer, 0, len);
    }
}

public record ArealsFileData(int UnusedHeader, int ArealCount, List<ArealInfo> ArealInfos);

public record ArealInfo(int Index, int CoordsCount, List<Vector3> Coords);

public record Vector3(float X, float Y, float Z);

// ----

public record ClansFileData(int ClanFeatureSet, int TreeCount, List<ClanTreeInfo> TreeInfos);

public class ClanTreeInfo
{
    public string ClanName { get; set; }
    public int UnkInt1 { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    
    /// <summary>
    /// 1 - игрок, 2 AI, 3 - нейтральный
    /// </summary>
    public int ClanType { get; set; }
    public string UnkString2 { get; set; }
    public int UnknownClanPartCount { get; set; }
    public List<UnknownClanTreeInfoPart> UnknownParts { get; set; }
    public string ResearchNResPath { get; set; }
    public int UnkInt3 { get; set; }
    public int AlliesMapCount { get; set; }
    
    /// <summary>
    /// мапа союзников (ключ - имя клана, значение - число, всегда либо 0 либо 1)
    /// </summary>
    public Dictionary<string, int> AlliesMap { get; set; }
}

public record UnknownClanTreeInfoPart(int UnkInt1, Vector3 UnkVector, float UnkInt2, float UnkInt3);