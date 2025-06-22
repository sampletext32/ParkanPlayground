namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a far pointer memory operand (m16:32) in an x86 instruction
/// </summary>
public class FarPointerOperand : MemoryOperand
{
    /// <summary>
    /// Gets the base register (if any)
    /// </summary>
    public RegisterIndex? BaseRegister { get; }
    
    /// <summary>
    /// Gets the index register (if any)
    /// </summary>
    public RegisterIndex? IndexRegister { get; }
    
    /// <summary>
    /// Gets the scale factor (if using an index register)
    /// </summary>
    public int Scale { get; }
    
    /// <summary>
    /// Gets the displacement value (if any)
    /// </summary>
    public long Displacement { get; }
    
    /// <summary>
    /// Gets the direct memory address (if any)
    /// </summary>
    public long? Address { get; }
    
    /// <summary>
    /// Initializes a new instance of the FarPointerOperand class for base register memory operands
    /// </summary>
    /// <param name="baseRegister">The base register</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public FarPointerOperand(RegisterIndex baseRegister, string? segmentOverride = null)
        : base(48, segmentOverride)
    {
        Type = OperandType.MemoryBaseReg;
        BaseRegister = baseRegister;
        IndexRegister = null;
        Scale = 1;
        Displacement = 0;
        Address = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the FarPointerOperand class for displacement memory operands
    /// </summary>
    /// <param name="baseRegister">The base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public FarPointerOperand(RegisterIndex baseRegister, long displacement, string? segmentOverride = null)
        : base(48, segmentOverride)
    {
        Type = OperandType.MemoryBaseRegPlusOffset;
        BaseRegister = baseRegister;
        IndexRegister = null;
        Scale = 1;
        Displacement = displacement;
        Address = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the FarPointerOperand class for scaled index memory operands
    /// </summary>
    /// <param name="indexRegister">The index register</param>
    /// <param name="scale">The scale factor</param>
    /// <param name="baseRegister">The optional base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public FarPointerOperand(RegisterIndex indexRegister, int scale, RegisterIndex? baseRegister = null, long displacement = 0, string? segmentOverride = null)
        : base(48, segmentOverride)
    {
        Type = OperandType.MemoryIndexed;
        BaseRegister = baseRegister;
        IndexRegister = indexRegister;
        Scale = scale;
        Displacement = displacement;
        Address = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the FarPointerOperand class for direct memory operands
    /// </summary>
    /// <param name="address">The memory address</param>
    /// <param name="segmentOverride">Optional segment override</param>
    public FarPointerOperand(long address, string? segmentOverride = null)
        : base(48, segmentOverride)
    {
        Type = OperandType.MemoryDirect;
        BaseRegister = null;
        IndexRegister = null;
        Scale = 1;
        Displacement = 0;
        Address = address;
    }
    
    /// <summary>
    /// Creates a FarPointerOperand from an existing memory operand
    /// </summary>
    /// <param name="memoryOperand">The memory operand to convert</param>
    /// <returns>A new FarPointerOperand with the same properties</returns>
    public static FarPointerOperand FromMemoryOperand(MemoryOperand memoryOperand)
    {
        // Create the appropriate type of FarPointerOperand based on the source operand type
        if (memoryOperand is BaseRegisterMemoryOperand baseRegMemOperand)
        {
            return new FarPointerOperand(baseRegMemOperand.BaseRegister, memoryOperand.SegmentOverride);
        }
        else if (memoryOperand is DisplacementMemoryOperand dispMemOperand)
        {
            return new FarPointerOperand(dispMemOperand.BaseRegister, dispMemOperand.Displacement, memoryOperand.SegmentOverride);
        }
        else if (memoryOperand is DirectMemoryOperand directMemOperand)
        {
            return new FarPointerOperand(directMemOperand.Address, memoryOperand.SegmentOverride);
        }
        else if (memoryOperand is ScaledIndexMemoryOperand sibMemOperand)
        {
            return new FarPointerOperand(sibMemOperand.IndexRegister, sibMemOperand.Scale, sibMemOperand.BaseRegister, sibMemOperand.Displacement, memoryOperand.SegmentOverride);
        }
        
        // Default case - shouldn't happen if all memory operand types are handled above
        throw new System.ArgumentException("Unsupported memory operand type", nameof(memoryOperand));
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        string prefix = "fword ptr ";
        
        // Add segment override if present
        if (SegmentOverride != null)
        {
            prefix = $"{prefix}{SegmentOverride}:";
        }
        
        // Format based on operand type
        return Type switch
        {
            OperandType.MemoryBaseReg => $"{prefix}[{RegisterMapper.GetRegisterName(BaseRegister!.Value, 32)}]",
            
            OperandType.MemoryBaseRegPlusOffset => $"{prefix}[{RegisterMapper.GetRegisterName(BaseRegister!.Value, 32)}+0x{Displacement:X}]",
            
            OperandType.MemoryDirect => $"{prefix}[0x{Address!.Value:X}]",
            
            OperandType.MemoryIndexed => FormatSIBString(prefix),
            
            _ => $"{prefix}[unknown]"
        };
    }
    
    /// <summary>
    /// Formats the string representation for SIB addressing mode
    /// </summary>
    private string FormatSIBString(string prefix)
    {
        string result = prefix + "[";
        
        // Add base register if present
        if (BaseRegister.HasValue)
        {
            result += RegisterMapper.GetRegisterName(BaseRegister.Value, 32);
        }
        
        // Add index register with scale if present
        if (IndexRegister.HasValue)
        {
            // Add + if we already have a base register
            if (BaseRegister.HasValue)
            {
                result += "+";
            }
            
            result += RegisterMapper.GetRegisterName(IndexRegister.Value, 32);
            
            // Add scale if not 1
            if (Scale > 1)
            {
                result += $"*{Scale}";
            }
        }
        
        // Add displacement if non-zero
        if (Displacement != 0)
        {
            // Format as signed value
            if (Displacement > 0)
            {
                result += $"+0x{Displacement:X}";
            }
            else
            {
                result += $"-0x{-Displacement:X}";
            }
        }
        
        result += "]";
        return result;
    }
}
