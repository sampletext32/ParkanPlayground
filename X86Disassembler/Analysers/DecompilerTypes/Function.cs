using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers.DecompilerTypes;

/// <summary>
/// Represents a function in decompiled code
/// </summary>
public class Function
{
    /// <summary>
    /// The name of the function
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The address of the function
    /// </summary>
    public ulong Address { get; set; }
    
    /// <summary>
    /// The return type of the function
    /// </summary>
    public DataType ReturnType { get; set; } = DataType.Void;
    
    /// <summary>
    /// The parameters of the function
    /// </summary>
    public List<Variable> Parameters { get; set; } = [];
    
    /// <summary>
    /// Local variables in this function
    /// </summary>
    public List<Variable> LocalVariables { get; } = [];
    
    /// <summary>
    /// Variables stored in registers
    /// </summary>
    public List<Variable> RegisterVariables { get; } = [];
    
    /// <summary>
    /// The calling convention used by the function
    /// </summary>
    public CallingConvention CallingConvention { get; set; } = CallingConvention.Cdecl;
    
    /// <summary>
    /// The assembly function representation
    /// </summary>
    public AsmFunction AsmFunction { get; set; }
    
    /// <summary>
    /// Creates a new function with the specified name and address
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <param name="address">The address of the function</param>
    /// <param name="asmFunction">The assembly function representation</param>
    public Function(string name, ulong address, AsmFunction asmFunction)
    {
        Name = name;
        Address = address;
        AsmFunction = asmFunction;
    }
    
    /// <summary>
    /// Analyzes the function to identify variables
    /// </summary>
    public void AnalyzeVariables()
    {
        // Create a variable analyzer
        var variableAnalyzer = new VariableAnalyzer(AsmFunction.Context);
        
        // Analyze stack variables
        variableAnalyzer.AnalyzeStackVariables(this);
    }
    

    

    

    
    /// <summary>
    /// Returns a string representation of the function signature
    /// </summary>
    public string GetSignature()
    {
        string paramList = string.Join(", ", Parameters.Select(p => $"{p.Type} {p.Name}"));
        return $"{ReturnType} {Name}({paramList})";
    }
    
    /// <summary>
    /// Returns a string representation of the function
    /// </summary>
    public override string ToString()
    {
        return GetSignature();
    }
}
