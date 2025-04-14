namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a memory operand with scale, index, and base in an x86 instruction (e.g., [eax+ecx*4+0x8])
/// </summary>
public class ScaledIndexMemoryOperand : MemoryOperand
{
    /// <summary>
    /// Gets or sets the base register
    /// </summary>
    public RegisterIndex? BaseRegister { get; set; }
    
    /// <summary>
    /// Gets or sets the index register
    /// </summary>
    public RegisterIndex IndexRegister { get; set; }
    
    /// <summary>
    /// Gets or sets the scale factor (1, 2, 4, or 8)
    /// </summary>
    public int Scale { get; set; }
    
    /// <summary>
    /// Gets or sets the displacement value
    /// </summary>
    public long Displacement { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ScaledIndexMemoryOperand class
    /// </summary>
    /// <param name="indexRegister">The index register</param>
    /// <param name="scale">The scale factor (1, 2, 4, or 8)</param>
    /// <param name="baseRegister">The optional base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public ScaledIndexMemoryOperand(RegisterIndex indexRegister, int scale, RegisterIndex? baseRegister = null, 
                                   long displacement = 0, int size = 32, string? segmentOverride = null)
        : base(size, segmentOverride)
    {
        Type = OperandType.MemoryIndexed;
        IndexRegister = indexRegister;
        Scale = scale;
        BaseRegister = baseRegister;
        Displacement = displacement;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        string baseRegPart = BaseRegister != null ? $"{ModRMDecoder.GetRegisterName(BaseRegister.Value, 32)}+" : "";
        string indexPart = $"{ModRMDecoder.GetRegisterName(IndexRegister, 32)}*{Scale}";
        string dispPart = "";
        
        if (Displacement != 0)
        {
            string sign = Displacement > 0 ? "+" : "";
            dispPart = $"{sign}0x{Math.Abs(Displacement):X}";
        }
        
        return $"{GetSegmentPrefix()}{GetSizePrefix()}[{baseRegPart}{indexPart}{dispPart}]";
    }
}
