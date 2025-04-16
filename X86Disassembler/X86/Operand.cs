namespace X86Disassembler.X86;

/// <summary>
/// Base class for all x86 instruction operands
/// </summary>
public abstract class Operand
{
    /// <summary>
    /// Gets or sets the type of this operand
    /// </summary>
    public OperandType Type { get; protected set; }
    
    /// <summary>
    /// Gets or sets the size of the operand in bits (8, 16, 32)
    /// </summary>
    public int Size { get; set; }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public abstract override string ToString();
}
