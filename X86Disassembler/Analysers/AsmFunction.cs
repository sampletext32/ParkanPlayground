namespace X86Disassembler.Analysers;

/// <summary>
/// Represents a disassembled function with its control flow graph
/// </summary>
public class AsmFunction
{
    /// <summary>
    /// The starting address of the function
    /// </summary>
    public ulong Address { get; set; }

    /// <summary>
    /// The list of basic blocks that make up the function
    /// </summary>
    public List<InstructionBlock> Blocks { get; set; } = [];

    public override string ToString()
    {
        return $"{Address:X8}\n{string.Join("\n", Blocks)}";
    }
}