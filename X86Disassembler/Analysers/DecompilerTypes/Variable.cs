namespace X86Disassembler.Analysers.DecompilerTypes;

/// <summary>
/// Represents a variable in decompiled code
/// </summary>
public class Variable
{
    /// <summary>
    /// The type of storage for a variable
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// Variable stored on the stack (local variable)
        /// </summary>
        Stack,
        
        /// <summary>
        /// Variable stored in a register
        /// </summary>
        Register,
        
        /// <summary>
        /// Variable stored in global memory
        /// </summary>
        Global,
        
        /// <summary>
        /// Function parameter passed on the stack
        /// </summary>
        Parameter,
        
        /// <summary>
        /// Function parameter passed in a register
        /// </summary>
        RegisterParameter
    }
    
    /// <summary>
    /// The name of the variable
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of the variable
    /// </summary>
    public DataType Type { get; set; } = DataType.Unknown;
    
    /// <summary>
    /// The storage location of the variable
    /// </summary>
    public StorageType Storage { get; set; }
    
    /// <summary>
    /// The offset from the base pointer (for stack variables)
    /// </summary>
    public int? StackOffset { get; set; }
    
    /// <summary>
    /// The register that holds this variable (for register variables)
    /// </summary>
    public X86.RegisterIndex? Register { get; set; }
    
    /// <summary>
    /// The memory address (for global variables)
    /// </summary>
    public ulong? Address { get; set; }
    
    /// <summary>
    /// The size of the variable in bytes
    /// </summary>
    public int Size { get; set; }
    
    /// <summary>
    /// Whether this variable is a function parameter
    /// </summary>
    public bool IsParameter { get; set; }
    
    /// <summary>
    /// The parameter index (if this is a parameter)
    /// </summary>
    public int? ParameterIndex { get; set; }
    
    /// <summary>
    /// Creates a new variable with the specified name and type
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="type">The type of the variable</param>
    public Variable(string name, DataType type)
    {
        Name = name;
        Type = type;
    }
    
    /// <summary>
    /// Returns a string representation of the variable
    /// </summary>
    public override string ToString()
    {
        return $"{Type} {Name}";
    }
}
