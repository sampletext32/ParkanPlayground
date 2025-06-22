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
        
        string format;

        if (maskedValue == 0)
        {
            format = "X2";
        }
        else if (maskedValue <= 0xFF)
        {
            format = "X2";
        }
        else if (maskedValue <= 0xFFFF)
        {
            format = "X4";
        }
        else
        {
            format = "X8";
        }

        return $"0x{maskedValue.ToString(format)}";
    }
}
