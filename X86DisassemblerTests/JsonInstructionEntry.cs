namespace X86DisassemblerTests;

public class JsonInstructionEntry
{
    public string RawBytes { get; set; } = "";
    public List<JsonDisassembledInstruction> Disassembled { get; set; } = [];
}

public class JsonDisassembledInstruction
{
    public string Mnemonic { get; set; } = "";
    public string Operands { get; set; } = "";
}