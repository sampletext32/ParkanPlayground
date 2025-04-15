using System.Text.Json.Serialization;
using CsvHelper.Configuration;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests;

public class TestFromFileEntry
{
    public string RawBytes { get; set; } = string.Empty;
    public List<TestFromFileInstruction> Instructions { get; set; } = new();

    public TestFromFileEntry()
    {
    }

    public TestFromFileEntry(string rawBytes, List<TestFromFileInstruction> instructions)
    {
        RawBytes = rawBytes;
        Instructions = instructions;
    }

    public override string ToString()
    {
        return $"{RawBytes}. {string.Join(";", Instructions)}";
    }
}

public class TestFromFileInstruction
{
    // Keep the old properties for CSV deserialization
    public string[] Operands { get; set; } = [];

    // Mnemonic
    [JsonConverter(typeof(JsonStringEnumConverter<InstructionType>))]
    public InstructionType Type { get; set; }

    // Parameterless constructor required by CsvHelper
    public TestFromFileInstruction()
    {
    }

    public override string ToString()
    {
        return $"{Type} {string.Join(",", Operands)}";
    }
}

public sealed class TestFromFileEntryMap : ClassMap<TestFromFileEntry>
{
    public TestFromFileEntryMap()
    {
        Map(m => m.RawBytes)
            .Name("RawBytes");
        Map(m => m.Instructions)
            .Name("Instructions")
            .TypeConverter<CsvJsonConverter<List<TestFromFileInstruction>>>();
    }
}