using Common;

namespace ScrLib;

public class ScrParser
{
    public static ScrFile ReadFile(string filePath)
    {
        var fs = new FileStream(filePath, FileMode.Open);

        var scrFile = new ScrFile();
        
        scrFile.Magic = fs.ReadInt32LittleEndian();

        scrFile.EntryCount = fs.ReadInt32LittleEndian();
        scrFile.Entries = [];

        for (var i = 0; i < scrFile.EntryCount; i++)
        {
            var entry = new ScrEntry();
            entry.Title = fs.ReadLengthPrefixedString();
    
            // тут игра дополнительно вычитывает ещё 1 байт, видимо как \0 для char*
            fs.ReadByte();

            entry.Index = fs.ReadInt32LittleEndian();
            entry.InnerCount = fs.ReadInt32LittleEndian();
            entry.Inners = [];
            for (var i1 = 0; i1 < entry.InnerCount; i1++)
            {
                var entryInner = new ScrEntryInner();
                entryInner.ScriptIndex = fs.ReadInt32LittleEndian();
                
                entryInner.UnkInner2 = fs.ReadInt32LittleEndian();
                entryInner.UnkInner3 = fs.ReadInt32LittleEndian();
                entryInner.Type = (ScrEntryInnerType)fs.ReadInt32LittleEndian();
                entryInner.UnkInner5 = fs.ReadInt32LittleEndian();
        
                entryInner.ArgumentsCount = fs.ReadInt32LittleEndian();

                entryInner.Arguments = [];

                for (var i2 = 0; i2 < entryInner.ArgumentsCount; i2++)
                {
                    entryInner.Arguments.Add(fs.ReadInt32LittleEndian());
                }

                entryInner.UnkInner7 = fs.ReadInt32LittleEndian();
                entry.Inners.Add(entryInner);
            }
            
            scrFile.Entries.Add(entry);
        }

        return scrFile;
    }
}