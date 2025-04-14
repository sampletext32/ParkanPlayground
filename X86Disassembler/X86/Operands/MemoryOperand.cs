namespace X86Disassembler.X86.Operands;

/// <summary>
/// Base class for all memory operands in an x86 instruction
/// </summary>
public abstract class MemoryOperand : Operand
{
    /// <summary>
    /// Gets or sets the segment override (if any)
    /// </summary>
    public string? SegmentOverride { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the MemoryOperand class
    /// </summary>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    protected MemoryOperand(int size = 32, string? segmentOverride = null)
    {
        Size = size;
        SegmentOverride = segmentOverride;
    }
    
    /// <summary>
    /// Gets the segment prefix string for display
    /// </summary>
    /// <returns>The segment prefix string</returns>
    protected string GetSegmentPrefix()
    {
        return SegmentOverride != null ? $"{SegmentOverride}:" : "";
    }
}
