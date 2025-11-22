using Common;

namespace VarsetLib;

public class BinaryVarsetFileParser
{
    public static BinaryVarsetFile Parse(string path)
    {
        using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Parse(fs);
    }

    public static BinaryVarsetFile Parse(Stream fs)
    {
        var count = fs.ReadInt32LittleEndian();
        
        var items = new List<BinaryVarsetItem>(count);

        Span<byte> buf4 = stackalloc byte[4];
        for (int i = 0; i < count; i++)
        {
            var magic1 = fs.ReadInt32LittleEndian();
            fs.ReadExactly(buf4);
            var magic2 = new IntFloatValue(buf4);

            fs.ReadExactly(buf4);
            var magic3 = new IntFloatValue(buf4);
            var name = fs.ReadLengthPrefixedString();
            var string2 = fs.ReadLengthPrefixedString();
            
            fs.ReadExactly(buf4);
            var magic4 = new IntFloatValue(buf4);
            fs.ReadExactly(buf4);
            var magic5 = new IntFloatValue(buf4);
            fs.ReadExactly(buf4);
            var magic6 = new IntFloatValue(buf4);
            fs.ReadExactly(buf4);
            var magic7 = new IntFloatValue(buf4);
            
            items.Add(new BinaryVarsetItem(magic1, magic2, magic3, name, string2, magic4, magic5, magic6, magic7));
        }

        return new BinaryVarsetFile(count, items);
    }
}
