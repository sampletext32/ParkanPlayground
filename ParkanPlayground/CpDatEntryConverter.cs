using Common;
using MissionTmaLib.Parsing;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// Игра называет этот объект "схемой"
/// </summary>
/// <remarks>
/// В игре файл .dat читается в ArealMap.dll/CreateObjectFromScheme
/// </remarks>
/// <code>
///
/// struct Scheme
/// {
///     char[32] str1; // имя архива
///     char[32] str2; // имя объекта в архиве
///     undefined4 magic1;
///     undefined4 magic2;
///     char[32] str3; // описание объекта
///     undefined4 magic3;
/// }
///
/// </code>
public class CpDatEntryConverter
{
    const string gameRoot = "C:\\Program Files (x86)\\Nikita\\Iron Strategy";
    const string missionTmaPath = $"{gameRoot}\\MISSIONS\\Campaign\\Campaign.01\\Mission.01\\data.tma";
    const string staticRlbPath = $"{gameRoot}\\static.rlb";
    const string objectsRlbPath = $"{gameRoot}\\objects.rlb";

    // Схема такая:
    // Файл обязан начинаться с 0xf1 0xf0 ("cp\0\0") - типа заголовок
    // Далее 4 байта - тип объекта, который содержится в схеме (их я выдернул из .var файла)
    // Далее 0x6c (108) байт - root объект
    
    public void Convert()
    {
        var tma = MissionTmaParser.ReadFile(missionTmaPath);
        var staticRlbResult = NResParser.ReadFile(staticRlbPath);
        var objectsRlbResult = NResParser.ReadFile(objectsRlbPath);

        var mission = tma.Mission!;
        var sRlb = staticRlbResult.Archive!;
        var oRlb = objectsRlbResult.Archive!;

        Span<byte> f0f1 = stackalloc byte[4];
        foreach (var gameObject in mission.GameObjectsData.GameObjectInfos)
        {
            var gameObjectDatPath = gameObject.DatString;

            if (gameObjectDatPath.Contains('\\'))
            {
                // если это путь, то надо искать его в папке
                string datFullPath = $"{gameRoot}\\{gameObjectDatPath}";

                using FileStream fs = new FileStream(datFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                fs.ReadExactly(f0f1);

                if (f0f1[0] != 0xf1 || f0f1[1] != 0xf0)
                {
                    _ = 5;
                }

                var fileFlags = (CpEntryType)fs.ReadInt32LittleEndian();

                var entryLength = 0x6c + 4; // нам нужно прочитать 0x6c (108) байт - это root, и ещё 4 байта - кол-во вложенных объектов
                if ((fs.Length - 8) % entryLength != 0)
                {
                    _ = 5;
                }

                DatEntry entry = ReadEntryRecursive(fs);

                // var objects = entries.Select(x => oRlb.Files.FirstOrDefault(y => y.FileName == x.ArchiveEntryName))
                //     .ToList();

                _ = 5;
            }
            else
            {
                // это статический объект, который будет в objects.rlb
                var sEntry = oRlb.Files.FirstOrDefault(x => x.FileName == gameObjectDatPath);

                _ = 5;
            }
        }
    }

    private DatEntry ReadEntryRecursive(FileStream fs)
    {
        var str1 = fs.ReadNullTerminatedString();

        fs.Seek(32 - str1.Length - 1, SeekOrigin.Current);

        var str2 = fs.ReadNullTerminatedString();

        fs.Seek(32 - str2.Length - 1, SeekOrigin.Current);
        var magic1 = fs.ReadInt32LittleEndian();
        var magic2 = fs.ReadInt32LittleEndian();

        var descriptionString = fs.ReadNullTerminatedString();

        fs.Seek(32 - descriptionString.Length - 1, SeekOrigin.Current);
        var magic3 = fs.ReadInt32LittleEndian();

        // игра не читает количество внутрь схемы, вместо этого она сразу рекурсией читает нужно количество вложенных объектов
        var childCount = fs.ReadInt32LittleEndian();
        
        List<DatEntry> children = new List<DatEntry>();
        
        for (var i = 0; i < childCount; i++)
        {
            var child = ReadEntryRecursive(fs);
            children.Add(child);
        }

        return new DatEntry(str1, str2, magic1, magic2, descriptionString, magic3, childCount, Children: children);
    }
    
    public record DatEntry(
        string ArchiveFile,
        string ArchiveEntryName,
        int Magic1,
        int Magic2,
        string Description,
        int Magic3,
        int ChildCount, // игра не хранит это число в объекте, но оно есть в файле
        List<DatEntry> Children
    );

    enum CpEntryType : uint
    {
        ClassBuilding = 0x80000000,
        ClassRobot = 0x01000000,
        ClassAnimal = 0x20000000,

        BunkerSmall = 0x80010000,
        BunkerMedium = 0x80020000,
        BunkerLarge = 0x80040000,
        Generator = 0x80000002,
        Mine = 0x80000004,
        Storage = 0x80000008,
        Plant = 0x80000010,
        Hangar = 0x80000040,
        TowerMedium = 0x80100000,
        TowerLarge = 0x80200000,
        MainTeleport = 0x80000200,
        Institute = 0x80000400,
        Bridge = 0x80001000,
        Ruine = 0x80002000,

        RobotTransport = 0x01002000,
        RobotBuilder = 0x01004000,
        RobotBattleunit = 0x01008000,
        RobotHq = 0x01010000,
        RobotHero = 0x01020000,
    }
}