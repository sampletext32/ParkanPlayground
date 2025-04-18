namespace X86Disassembler.Analysers;

public class AsmFunction
{
    public ulong Address { get; set; }

    public List<InstructionBlock> Blocks { get; set; }

    public override string ToString()
    {
        return $"Function at {Address:X8}\n{string.Join("\n", Blocks.Select(x => $"\t{x}"))}";
    }
}