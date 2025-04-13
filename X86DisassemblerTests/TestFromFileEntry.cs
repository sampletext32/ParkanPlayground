using CsvHelper.Configuration;

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
}

public class TestFromFileInstruction
{
    public string Mnemonic { get; set; } = string.Empty;
    public string Operands { get; set; } = string.Empty;

    // Parameterless constructor required by CsvHelper
    public TestFromFileInstruction()
    {
    }

    public TestFromFileInstruction(string mnemonic, string operands)
    {
        Mnemonic = mnemonic;
        Operands = operands;
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