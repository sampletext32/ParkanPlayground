using X86Disassembler.Analysers.DecompilerTypes;
using X86Disassembler.PE;
using X86Disassembler.X86;

namespace X86Disassembler.Analysers;

/// <summary>
/// Main engine for decompiling x86 code
/// </summary>
public class DecompilerEngine
{
    /// <summary>
    /// The PE file being analyzed
    /// </summary>
    private readonly PeFile _peFile;
    
    /// <summary>
    /// Dictionary of analyzed functions by address
    /// </summary>
    private readonly Dictionary<ulong, Function> _functions = [];
    
    /// <summary>
    /// Dictionary of exported function names by address
    /// </summary>
    private readonly Dictionary<ulong, string> _exportedFunctions = [];
    
    /// <summary>
    /// Creates a new decompiler engine for the specified PE file
    /// </summary>
    /// <param name="peFile">The PE file to decompile</param>
    public DecompilerEngine(PeFile peFile)
    {
        _peFile = peFile;
        
        // Initialize the exported functions dictionary
        foreach (var export in peFile.ExportedFunctions)
        {
            _exportedFunctions[export.AddressRva] = export.Name;
        }
    }
    
    /// <summary>
    /// Decompiles a function at the specified address
    /// </summary>
    /// <param name="address">The address of the function to decompile</param>
    /// <returns>The decompiled function</returns>
    public Function DecompileFunction(ulong address)
    {
        // Check if we've already analyzed this function
        if (_functions.TryGetValue(address, out var existingFunction))
        {
            return existingFunction;
        }
        
        // Find the code section containing this address
        var codeSection = _peFile.SectionHeaders.Find(s => 
            s.ContainsCode() && 
            address >= s.VirtualAddress && 
            address < s.VirtualAddress + s.VirtualSize);
        
        if (codeSection == null)
        {
            throw new InvalidOperationException($"No code section found containing address 0x{address:X8}");
        }
        
        // Get the section data
        int sectionIndex = _peFile.SectionHeaders.IndexOf(codeSection);
        byte[] codeBytes = _peFile.GetSectionData(sectionIndex);
        
        // Create a disassembler for the code section
        var disassembler = new BlockDisassembler(codeBytes, codeSection.VirtualAddress);
        
        // Disassemble the function
        var asmFunction = disassembler.DisassembleFromAddress((uint)address);
        
        // Create an analyzer context
        var context = new AnalyzerContext(asmFunction);
        
        // Run the analyzers
        var loopAnalyzer = new LoopAnalyzer();
        loopAnalyzer.AnalyzeLoops(context);
        
        var dataFlowAnalyzer = new DataFlowAnalyzer();
        dataFlowAnalyzer.AnalyzeDataFlow(context);
        
        // Get the function name from exports if available
        string functionName = _exportedFunctions.TryGetValue(address, out var name) 
            ? name 
            : $"func_{address:X8}";
        
        // Analyze the function
        var functionAnalyzer = new FunctionAnalyzer(context);
        var function = functionAnalyzer.AnalyzeFunction(address, functionName);
        
        // Analyze control flow structures
        var controlFlowAnalyzer = new ControlFlowAnalyzer(context);
        controlFlowAnalyzer.AnalyzeControlFlow(function);
        

        
        // Store the function in our cache
        _functions[address] = function;
        
        return function;
    }
    
    /// <summary>
    /// Generates C-like pseudocode for a decompiled function
    /// </summary>
    /// <param name="function">The function to generate pseudocode for</param>
    /// <returns>The generated pseudocode</returns>
    public string GeneratePseudocode(Function function)
    {
        // Create a pseudocode generator
        var generator = new PseudocodeGenerator();
        
        // Generate the pseudocode
        return generator.GeneratePseudocode(function);
    }
    
    /// <summary>
    /// Decompiles all exported functions in the PE file
    /// </summary>
    /// <returns>A dictionary of decompiled functions by address</returns>
    public Dictionary<ulong, Function> DecompileAllExportedFunctions()
    {
        foreach (var export in _peFile.ExportedFunctions)
        {
            // Skip forwarded exports
            if (export.IsForwarder)
            {
                continue;
            }
            
            try
            {
                DecompileFunction(export.AddressRva);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decompiling function {export.Name} at 0x{export.AddressRva:X8}: {ex.Message}");
            }
        }
        
        return _functions;
    }
}


