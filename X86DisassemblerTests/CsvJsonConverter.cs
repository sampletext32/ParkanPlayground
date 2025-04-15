using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using X86Disassembler.X86;

namespace X86DisassemblerTests;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CsvJsonConverter<T> : DefaultTypeConverter
{
    // Configure JSON options with case-insensitive enum handling
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(text, _options);
    }

    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}