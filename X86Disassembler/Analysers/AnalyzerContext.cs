namespace X86Disassembler.Analysers;

/// <summary>
/// Central context for all analysis data related to a disassembled function
/// </summary>
public class AnalyzerContext
{
    /// <summary>
    /// The function being analyzed
    /// </summary>
    public AsmFunction Function { get; }
    
    /// <summary>
    /// Dictionary mapping block addresses to instruction blocks
    /// </summary>
    public Dictionary<ulong, InstructionBlock> BlocksByAddress { get; } = [];
    
    /// <summary>
    /// Dictionary mapping loop header addresses to loops
    /// </summary>
    public Dictionary<ulong, Loop> LoopsByHeaderAddress { get; } = [];
    
    /// <summary>
    /// Dictionary mapping block addresses to the loops that contain them
    /// </summary>
    public Dictionary<ulong, List<Loop>> LoopsByBlockAddress { get; } = [];
    
    /// <summary>
    /// Dictionary for storing arbitrary analysis data by address
    /// </summary>
    public Dictionary<ulong, Dictionary<string, object>> AnalysisDataByAddress { get; } = [];
    
    /// <summary>
    /// Creates a new analyzer context for the given function
    /// </summary>
    /// <param name="function">The function to analyze</param>
    public AnalyzerContext(AsmFunction function)
    {
        Function = function;
        
        // Initialize the block dictionary
        foreach (var block in function.Blocks)
        {
            BlocksByAddress[block.Address] = block;
        }
    }
    
    /// <summary>
    /// Represents a loop in the control flow graph
    /// </summary>
    public class Loop
    {
        /// <summary>
        /// The header block of the loop (the entry point into the loop)
        /// </summary>
        public InstructionBlock Header { get; set; } = null!;
        
        /// <summary>
        /// The blocks that are part of this loop
        /// </summary>
        public List<InstructionBlock> Blocks { get; set; } = [];
        
        /// <summary>
        /// The back edge that completes the loop (from a block back to the header)
        /// </summary>
        public (InstructionBlock From, InstructionBlock To) BackEdge { get; set; }
        
        /// <summary>
        /// The exit blocks of the loop (blocks that have successors outside the loop)
        /// </summary>
        public List<InstructionBlock> ExitBlocks { get; set; } = [];
    }
    
    /// <summary>
    /// Stores analysis data for a specific address
    /// </summary>
    /// <param name="address">The address to store data for</param>
    /// <param name="key">The key for the data</param>
    /// <param name="value">The data to store</param>
    public void StoreAnalysisData(ulong address, string key, object value)
    {
        if (!AnalysisDataByAddress.TryGetValue(address, out var dataDict))
        {
            dataDict = [];
            AnalysisDataByAddress[address] = dataDict;
        }
        
        dataDict[key] = value;
    }
    
    /// <summary>
    /// Retrieves analysis data for a specific address
    /// </summary>
    /// <param name="address">The address to retrieve data for</param>
    /// <param name="key">The key for the data</param>
    /// <returns>The stored data, or null if not found</returns>
    public object? GetAnalysisData(ulong address, string key)
    {
        if (AnalysisDataByAddress.TryGetValue(address, out var dataDict) && 
            dataDict.TryGetValue(key, out var value))
        {
            return value;
        }
        
        return null;
    }
    
    /// <summary>
    /// Retrieves typed analysis data for a specific address
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve</typeparam>
    /// <param name="address">The address to retrieve data for</param>
    /// <param name="key">The key for the data</param>
    /// <returns>The stored data, or default(T) if not found or wrong type</returns>
    public T? GetAnalysisData<T>(ulong address, string key)
    {
        var data = GetAnalysisData(address, key);
        if (data is T typedData)
        {
            return typedData;
        }
        
        return default;
    }
}
