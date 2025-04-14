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
    /// Gets the size prefix string for display (e.g., "byte ptr", "word ptr", "dword ptr")
    /// </summary>
    /// <returns>The size prefix string</returns>
    protected string GetSizePrefix()
    {
        string sizePrefix = Size switch
        {
            8 => "byte ptr ",
            16 => "word ptr ",
            32 => "dword ptr ",
            64 => "qword ptr ",
            _ => ""
        };
        
        // If we have a segment override, include it in the format "dword ptr es:[reg]"
        if (SegmentOverride != null)
        {
            return $"{sizePrefix}{SegmentOverride}:";
        }
        
        return sizePrefix;
    }
}
