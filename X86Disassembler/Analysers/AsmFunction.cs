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
    /// The analyzer context for this function
    /// </summary>
    public AnalyzerContext Context { get; private set; }
    
    /// <summary>
    /// Creates a new AsmFunction instance
    /// </summary>
    public AsmFunction()
    {
        Context = new AnalyzerContext(this);
    }
    
    /// <summary>
    /// Analyzes the function using various analyzers
    /// </summary>
    public void Analyze()
    {
        // Analyze loops
        var loopAnalyzer = new LoopAnalyzer();
        loopAnalyzer.AnalyzeLoops(Context);
        
        // Analyze data flow
        var dataFlowAnalyzer = new DataFlowAnalyzer();
        dataFlowAnalyzer.AnalyzeDataFlow(Context);
    }

    /// <summary>
    /// Returns a string representation of the function, including its address, blocks, and analysis results
    /// </summary>
    public override string ToString()
    {
        string loopsInfo = "";
        if (Context.LoopsByHeaderAddress.Count > 0)
        {
            loopsInfo = $"Loops: {Context.LoopsByHeaderAddress.Count}\n";
            int i = 0;
            foreach (var loop in Context.LoopsByHeaderAddress.Values)
            {
                loopsInfo += $"  Loop {i++}: Header=0x{loop.Header.Address:X8}, " + 
                             $"Blocks={loop.Blocks.Count}, " + 
                             $"Back Edge=(0x{loop.BackEdge.From.Address:X8} -> 0x{loop.BackEdge.To.Address:X8}), " + 
                             $"Exits={loop.ExitBlocks.Count}\n";
            }
        }
        else
        {
            loopsInfo = "Loops: None\n";
        }
        
        return $"Function at 0x{Address:X8}\n" + 
               $"Entry Block: 0x{EntryBlock?.Address.ToString("X8") ?? "None"}\n" + 
               $"Exit Blocks: {(ExitBlocks.Count > 0 ? string.Join(", ", ExitBlocks.Select(b => $"0x{b.Address:X8}")) : "None")}\n" + 
               $"Total Blocks: {Blocks.Count}\n" + 
               loopsInfo + 
               $"{string.Join("\n", Blocks.Select(x => $"\t{x}"))}";
    }
}