namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a relative offset operand in an x86 instruction (used for jumps and calls)
/// </summary>
public class RelativeOffsetOperand : Operand
{
    /// <summary>
    /// Gets or sets the target address
    /// </summary>
    public uint TargetAddress { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the RelativeOffsetOperand class
    /// </summary>
    /// <param name="targetAddress">The target address</param>
    /// <param name="size">The size of the offset in bits (8 or 32)</param>
    public RelativeOffsetOperand(uint targetAddress, int size = 32)
    {
        Type = OperandType.RelativeOffset;
        TargetAddress = targetAddress;
        Size = size;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        return $"0x{TargetAddress:X8}";
    }
}
