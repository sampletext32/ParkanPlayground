namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a memory operand with a base register and displacement in an x86 instruction (e.g., [eax+0x4])
/// </summary>
public class DisplacementMemoryOperand : MemoryOperand
{
    /// <summary>
    /// Gets or sets the base register
    /// </summary>
    public RegisterIndex BaseRegister { get; set; }
    
    /// <summary>
    /// Gets or sets the displacement value
    /// </summary>
    public long Displacement { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the DisplacementMemoryOperand class
    /// </summary>
    /// <param name="baseRegister">The base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public DisplacementMemoryOperand(RegisterIndex baseRegister, long displacement, int size = 32, string? segmentOverride = null)
        : base(size, segmentOverride)
    {
        Type = OperandType.MemoryBaseRegPlusOffset;
        BaseRegister = baseRegister;
        Displacement = displacement;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        string sign = Displacement >= 0 ? "+" : "";
        var registerName = ModRMDecoder.GetRegisterName(BaseRegister, 32);
        
        // Format small displacements (< 256) with at least 2 digits
        string formattedDisplacement = Math.Abs(Displacement) < 256 
            ? $"0x{Math.Abs(Displacement):X2}" 
            : $"0x{Math.Abs(Displacement):X}";
        
        return $"{GetSizePrefix()}[{registerName}{sign}{formattedDisplacement}]";
    }
}
