using Common;

namespace ScrLib;

public class ScrParser
{
    public static ScrFile ReadFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ReadFile(fs);
    }

    public static ScrFile ReadFile(Stream fs)
    {
        var magic = fs.ReadInt32LittleEndian();
        var entryCount = fs.ReadInt32LittleEndian();
        List<ScrEntry> entries = [];

        for (var i = 0; i < entryCount; i++)
        {
            var title = fs.ReadLengthPrefixedString();

            // тут игра дополнительно вычитывает ещё 1 байт, видимо как \0 для char*
            fs.ReadByte();

            var index = fs.ReadInt32LittleEndian();
            var innerCount = fs.ReadInt32LittleEndian();
            List<ScrEntryInner> inners = [];
            for (var i1 = 0; i1 < innerCount; i1++)
            {
                var scriptIndex = fs.ReadInt32LittleEndian();
                var unkInner2 = fs.ReadInt32LittleEndian();
                var unkInner3 = fs.ReadInt32LittleEndian();
                var type = (ScrEntryInnerType)fs.ReadInt32LittleEndian();
                var unkInner5 = fs.ReadInt32LittleEndian();
                var argumentsCount = fs.ReadInt32LittleEndian();
                List<int> arguments = [];

                for (var i2 = 0; i2 < argumentsCount; i2++)
                {
                    arguments.Add(fs.ReadInt32LittleEndian());
                }

                var unkInner7 = fs.ReadInt32LittleEndian();
                inners.Add(new ScrEntryInner(
                    scriptIndex,
                    unkInner2,
                    unkInner3,
                    type,
                    unkInner5,
                    argumentsCount,
                    arguments,
                    unkInner7));
            }

            entries.Add(new ScrEntry(title, index, innerCount, inners));
        }

        return new ScrFile(magic, entryCount, entries);
    }
}
