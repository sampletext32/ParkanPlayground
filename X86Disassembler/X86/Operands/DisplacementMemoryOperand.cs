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
        // Get register name
        var registerName = RegisterMapper.GetRegisterName(BaseRegister, 32);

        // Format the displacement value
        long absDisplacement = Math.Abs(Displacement);
        string sign = Displacement >= 0 ? "+" : "-";
        string format;

        if (absDisplacement == 0)
        {
            format = "X2";
        }
        else if (absDisplacement <= 0xFF)
        {
            format = "X2";
        }
        else if (absDisplacement <= 0xFFFF)
        {
            format = "X4";
        }
        else
        {
            format = "X8";
        }

        string formattedDisplacement = $"0x{absDisplacement.ToString(format)}";
        
        return $"{GetSizePrefix()}[{registerName}{sign}{formattedDisplacement}]";
    }
}
