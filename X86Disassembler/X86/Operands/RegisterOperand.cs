namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents a register operand in an x86 instruction
/// </summary>
public class RegisterOperand : Operand
{
    /// <summary>
    /// Gets or sets the register
    /// </summary>
    public RegisterIndex Register { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the RegisterOperand class
    /// </summary>
    /// <param name="register">The register</param>
    /// <param name="size">The size of the register in bits</param>
    public RegisterOperand(RegisterIndex register, int size = 32)
    {
        Type = OperandType.Register;
        Register = register;
        Size = size;
    }
    
    /// <summary>
    /// Returns a string representation of this operand
    /// </summary>
    public override string ToString()
    {
        return ModRMDecoder.GetRegisterName(Register, Size);
    }
}
