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
}

public class TestFromFileInstruction
{
    // Keep the old properties for CSV deserialization
    public string Mnemonic { get; set; } = string.Empty;
    public string Operands { get; set; } = string.Empty;
    
    // Add new properties for comparison with actual Instruction objects
    public InstructionType Type => ConvertMnemonicToType(Mnemonic);
    
    // Parameterless constructor required by CsvHelper
    public TestFromFileInstruction()
    {
    }

    public TestFromFileInstruction(string mnemonic, string operands)
    {
        Mnemonic = mnemonic;
        Operands = operands;
    }
    
    // Helper method to convert mnemonic string to InstructionType
    private InstructionType ConvertMnemonicToType(string mnemonic)
    {
        // Convert mnemonic to InstructionType
        return mnemonic.ToLowerInvariant() switch
        {
            "add" => InstructionType.Add,
            "adc" => InstructionType.Adc,
            "and" => InstructionType.And,
            "call" => InstructionType.Call,
            "cmp" => InstructionType.Cmp,
            "dec" => InstructionType.Dec,
            "inc" => InstructionType.Inc,
            "int3" => InstructionType.Int,
            "jmp" => InstructionType.Jmp,
            "jz" => InstructionType.Jz,
            "jnz" => InstructionType.Jnz,
            "jge" => InstructionType.Jge,
            "lea" => InstructionType.Lea,
            "mov" => InstructionType.Mov,
            "nop" => InstructionType.Nop,
            "or" => InstructionType.Or,
            "pop" => InstructionType.Pop,
            "push" => InstructionType.Push,
            "ret" => InstructionType.Ret,
            "sbb" => InstructionType.Sbb,
            "sub" => InstructionType.Sub,
            "test" => InstructionType.Test,
            "xchg" => InstructionType.Xchg,
            "xor" => InstructionType.Xor,
            _ => InstructionType.Unknown
        };
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