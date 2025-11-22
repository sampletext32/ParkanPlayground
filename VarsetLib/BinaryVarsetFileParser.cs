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
            var valueLength = fs.ReadInt32LittleEndian();

            var valueType = (BinaryVarsetValueType)fs.ReadInt32LittleEndian();

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

            if(string2.Length != 0)
            {
                _ = 5;
            }
            
            if (valueType is BinaryVarsetValueType.Bool)
            {
                items.Add(new BinaryVarsetItem<bool>(
                    valueLength,
                    valueType,
                    magic3.AsInt != 0,
                    name,
                    string2,
                    magic4.AsInt != 0,
                    magic5.AsInt != 0,
                    magic6.AsInt != 0,
                    magic7.AsInt != 0));
            }
            else if (valueType is BinaryVarsetValueType.Dword)
            {
                items.Add(new BinaryVarsetItem<uint>(
                    valueLength,
                    valueType,
                    (uint)magic3.AsInt,
                    name,
                    string2,
                    (uint)magic4.AsInt,
                    (uint)magic5.AsInt,
                    (uint)magic6.AsInt,
                    (uint)magic7.AsInt));
            }
            else if (valueType is BinaryVarsetValueType.Float)
            {
                items.Add(new BinaryVarsetItem<float>(
                    valueLength,
                    valueType,
                    magic3.AsFloat,
                    name,
                    string2,
                    magic4.AsFloat,
                    magic5.AsFloat,
                    magic6.AsFloat,
                    magic7.AsFloat));
            }
            else
            {
                throw new InvalidOperationException($"Unknown value type {valueType}");
            }
        }

        return new BinaryVarsetFile(count, items);
    }
}
