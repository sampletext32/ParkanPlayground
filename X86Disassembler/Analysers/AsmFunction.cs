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

    /// <summary>
    /// The entry block of the function
    /// </summary>
    public InstructionBlock? EntryBlock => Blocks.FirstOrDefault(b => b.Address == Address);

    /// <summary>
    /// The exit blocks of the function (blocks that end with a return instruction)
    /// </summary>
    public List<InstructionBlock> ExitBlocks => Blocks.Where(b => 
        b.Instructions.Count > 0 && 
        b.Instructions[^1].Type.IsRet()).ToList();

    /// <summary>
    /// Returns a string representation of the function, including its address and blocks
    /// </summary>
    public override string ToString()
    {
        return $"Function at 0x{Address:X8}\n" + 
               $"Entry Block: 0x{EntryBlock?.Address.ToString("X8") ?? "None"}\n" + 
               $"Exit Blocks: {(ExitBlocks.Count > 0 ? string.Join(", ", ExitBlocks.Select(b => $"0x{b.Address:X8}")) : "None")}\n" + 
               $"Total Blocks: {Blocks.Count}\n" + 
               $"{string.Join("\n", Blocks.Select(x => $"\t{x}"))}";
    }
}