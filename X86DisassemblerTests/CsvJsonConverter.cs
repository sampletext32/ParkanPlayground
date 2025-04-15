using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace X86DisassemblerTests;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CsvJsonConverter<T> : DefaultTypeConverter
{
    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
    };
    
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(text);
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        return JsonSerializer.Serialize(value);
    }
}