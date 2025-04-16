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
    /// Gets the size of the operand in bits (8, 16, 32)
    /// </summary>
    public int Size { get; protected set; }
    
    /// <summary>
    /// Sets the size of the operand in bits
    /// </summary>
    /// <param name="size">The new size in bits (8, 16, 32, or 64)</param>
    /// <returns>The operand instance for method chaining</returns>
    public virtual Operand WithSize(int size)
    {
        Size = size;
        return this;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public abstract override string ToString();
}
