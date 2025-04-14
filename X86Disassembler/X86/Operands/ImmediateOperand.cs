namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents an immediate value operand in an x86 instruction
/// </summary>
public class ImmediateOperand : Operand
{
    /// <summary>
    /// Gets or sets the immediate value
    /// </summary>
    public long Value { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ImmediateOperand class
    /// </summary>
    /// <param name="value">The immediate value</param>
    /// <param name="size">The size of the value in bits</param>
    public ImmediateOperand(long value, int size = 32)
    {
        Type = OperandType.ImmediateValue;
        Value = value;
        Size = size;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        return $"0x{Value:X}";
    }
}
