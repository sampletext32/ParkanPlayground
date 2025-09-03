namespace CpDatLib;

public record CpDatEntry(
    string ArchiveFile,
    string ArchiveEntryName,
    int Magic1,
    int Magic2,
    string Description,
    int Magic3,
    int ChildCount, // игра не хранит это число в объекте, но оно есть в файле
    List<CpDatEntry> Children
);