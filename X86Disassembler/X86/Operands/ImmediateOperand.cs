namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents an immediate value operand in an x86 instruction
/// </summary>
public class ImmediateOperand : Operand
{
    /// <summary>
    /// Gets or sets the immediate value
    /// </summary>
    public ulong Value { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ImmediateOperand class
    /// </summary>
    /// <param name="value">The immediate value</param>
    /// <param name="size">The size of the value in bits</param>
    public ImmediateOperand(long value, int size = 32)
    {
        Type = OperandType.ImmediateValue;
        
        // For negative values in 32-bit mode, convert to unsigned 32-bit representation
        if (value < 0 && size == 32)
        {
            Value = (ulong)(uint)value; // Sign-extend to 32 bits, then store as unsigned
        }
        else
        {
            Value = (ulong)value;
        }
        
        Size = size;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        // Mask the value based on its size
        ulong maskedValue = Size switch
        {
            8 => Value & 0xFF,
            16 => Value & 0xFFFF,
            32 => Value & 0xFFFFFFFF,
            _ => Value
        };
        
        // For 8-bit immediate values, always use at least 2 digits
        if (Size == 8)
        {
            return $"0x{maskedValue:X2}";
        }
        
        // For 16-bit immediate values, format depends on the value
        if (Size == 16)
        {
            // For small values (< 256), show without leading zeros
            if (maskedValue < 256)
            {
                return $"0x{maskedValue:X}";
            }
            
            // For larger values, use at least 4 digits
            return $"0x{maskedValue:X4}";
        }
        
        // For 32-bit immediate values, format depends on the instruction context
        if (Size == 32)
        {
            // For small values (0), always show as 0x00
            if (maskedValue == 0)
            {
                return "0x00";
            }
            
            // For other small values (< 256), show as 0xNN
            if (maskedValue < 256)
            {
                return $"0x{maskedValue:X2}";
            }
        }
        
        // For larger 32-bit values, show the full 32-bit representation
        return $"0x{maskedValue:X8}";
    }
}
