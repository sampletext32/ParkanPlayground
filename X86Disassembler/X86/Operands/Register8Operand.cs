namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents an 8-bit register operand in an x86 instruction
/// </summary>
public class Register8Operand : Operand
{
    /// <summary>
    /// Gets or sets the 8-bit register
    /// </summary>
    public RegisterIndex8 Register { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the Register8Operand class
    /// </summary>
    /// <param name="register">The 8-bit register</param>
    public Register8Operand(RegisterIndex8 register)
    {
        Type = OperandType.Register;
        Register = register;
        Size = 8; // Always 8 bits for this operand type
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        return RegisterMapper.GetRegisterName(Register);
    }
}
