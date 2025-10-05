namespace CpDatLib;

public record CpDatEntry(
    string ArchiveFile,
    string ArchiveEntryName,
    int Magic1,
    int Magic2,
    string Description,
    DatEntryType Type,
    int ChildCount, // игра не хранит это число в объекте, но оно есть в файле
    List<CpDatEntry> Children
);

// Magic3 seems to be a type
// 0 - chassis
// 1 - turret (у зданий почему-то дефлектор тоже 1), может быть потому, что дефлектор вращается так же как башня у юнитов
// 2 - armour
// 3 - part
// 4 - cannon
// 5 - ammo

public enum DatEntryType
{
    Unspecified = -1,
    Chassis = 0,
    Turret = 1,
    Armour = 2,
    Part = 3,
    Cannon = 4,
    Ammo = 5,
}