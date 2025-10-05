using Common;

namespace CpDatLib;

public class CpDatParser
{
    public static CpDatParseResult Parse(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Parse(fs);
    }

    public static CpDatParseResult Parse(Stream fs)
    {
        Span<byte> f0f1 = stackalloc byte[4];

        if (fs.Length < 8)
            return new CpDatParseResult(null, "File too small to be a valid \"cp\" .dat file.");

        fs.ReadExactly(f0f1);

        if (f0f1[0] != 0xf1 || f0f1[1] != 0xf0)
        {
            return new CpDatParseResult(null, "File does not start with expected header bytes f1_f0");
        }

        var schemeType = (SchemeType)fs.ReadInt32LittleEndian();

        var entryLength =
            0x6c + 4; // нам нужно прочитать 0x6c (108) байт - это root, и ещё 4 байта - кол-во вложенных объектов
        if ((fs.Length - 8) % entryLength != 0)
        {
            return new CpDatParseResult(null, "File size is not valid according to expected entry length.");
        }

        CpDatEntry root = ReadEntryRecursive(fs);

        var scheme = new CpDatScheme(schemeType, root);

        return new CpDatParseResult(scheme, null);
    }

    private static CpDatEntry ReadEntryRecursive(Stream fs)
    {
        var str1 = fs.ReadNullTerminatedString();

        fs.Seek(32 - str1.Length - 1, SeekOrigin.Current); // -1 ignore null terminator

        var str2 = fs.ReadNullTerminatedString();

        fs.Seek(32 - str2.Length - 1, SeekOrigin.Current); // -1 ignore null terminator
        var magic1 = fs.ReadInt32LittleEndian();
        var magic2 = fs.ReadInt32LittleEndian();

        var descriptionString = fs.ReadNullTerminated1251String();

        fs.Seek(32 - descriptionString.Length - 1, SeekOrigin.Current); // -1 ignore null terminator
        var type = (DatEntryType)fs.ReadInt32LittleEndian();

        // игра не читает количество внутрь схемы, вместо этого она сразу рекурсией читает нужно количество вложенных объектов
        var childCount = fs.ReadInt32LittleEndian();

        List<CpDatEntry> children = new List<CpDatEntry>(childCount);

        for (var i = 0; i < childCount; i++)
        {
            var child = ReadEntryRecursive(fs);
            children.Add(child);
        }

        return new CpDatEntry(str1, str2, magic1, magic2, descriptionString, type, childCount, Children: children);
    }
}