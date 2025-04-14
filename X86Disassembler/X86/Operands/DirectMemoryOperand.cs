namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a direct memory operand in an x86 instruction (e.g., [0x12345678])
/// </summary>
public class DirectMemoryOperand : MemoryOperand
{
    /// <summary>
    /// Gets or sets the memory address
    /// </summary>
    public long Address { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the DirectMemoryOperand class
    /// </summary>
    /// <param name="address">The memory address</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public DirectMemoryOperand(long address, int size = 32, string? segmentOverride = null)
        : base(size, segmentOverride)
    {
        Type = OperandType.MemoryDirect;
        Address = address;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        return $"{GetSegmentPrefix()}{GetSizePrefix()}[0x{Address:X}]";
    }
}
