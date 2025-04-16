namespace X86Disassembler.X86.Operands;

/// <summary>
/// Represents an FPU register operand (ST(0) to ST(7))
/// </summary>
public class FPURegisterOperand : Operand
{
    /// <summary>
    /// Gets the FPU register index (0-7)
    /// </summary>
    public FpuRegisterIndex RegisterIndex { get; }

    /// <summary>
    /// Initializes a new instance of the FPURegisterOperand class
    /// </summary>
    /// <param name="registerIndex">The FPU register index (RegisterIndex.A to RegisterIndex.Di)</param>
    public FPURegisterOperand(FpuRegisterIndex registerIndex)
    {
        RegisterIndex = registerIndex;
    }

    /// <summary>
    /// Returns a string representation of the FPU register operand
    /// </summary>
    /// <returns>A string representation of the FPU register operand</returns>
    public override string ToString()
    {
        // Convert RegisterIndex to a numerical index (0-7)
        int index = (int)RegisterIndex;
        return $"ST({index})";
    }
}
