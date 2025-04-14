namespace X86Disassembler.X86.Operands;

/// <summary>
/// Factory class for creating operand objects
/// </summary>
public static class OperandFactory
{
    /// <summary>
    /// Creates a register operand
    /// </summary>
    /// <param name="register">The register</param>
    /// <param name="size">The size of the register in bits</param>
    /// <returns>A register operand</returns>
    public static RegisterOperand CreateRegisterOperand(RegisterIndex register, int size = 32)
    {
        return new RegisterOperand(register, size);
    }
    
    /// <summary>
    /// Creates an immediate value operand
    /// </summary>
    /// <param name="value">The immediate value</param>
    /// <param name="size">The size of the value in bits</param>
    /// <returns>An immediate value operand</returns>
    public static ImmediateOperand CreateImmediateOperand(uint value, int size = 32)
    {
        return new ImmediateOperand(value, size);
    }
    
    /// <summary>
    /// Creates a direct memory operand
    /// </summary>
    /// <param name="address">The memory address</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    /// <returns>A direct memory operand</returns>
    public static DirectMemoryOperand CreateDirectMemoryOperand(long address, int size = 32, string? segmentOverride = null)
    {
        return new DirectMemoryOperand(address, size, segmentOverride);
    }
    
    /// <summary>
    /// Creates a base register memory operand
    /// </summary>
    /// <param name="baseRegister">The base register</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    /// <returns>A base register memory operand</returns>
    public static BaseRegisterMemoryOperand CreateBaseRegisterMemoryOperand(RegisterIndex baseRegister, int size = 32, string? segmentOverride = null)
    {
        return new BaseRegisterMemoryOperand(baseRegister, size, segmentOverride);
    }
    
    /// <summary>
    /// Creates a displacement memory operand
    /// </summary>
    /// <param name="baseRegister">The base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    /// <returns>A displacement memory operand</returns>
    public static DisplacementMemoryOperand CreateDisplacementMemoryOperand(RegisterIndex baseRegister, long displacement, int size = 32, string? segmentOverride = null)
    {
        return new DisplacementMemoryOperand(baseRegister, displacement, size, segmentOverride);
    }
    
    /// <summary>
    /// Creates a scaled index memory operand
    /// </summary>
    /// <param name="indexRegister">The index register</param>
    /// <param name="scale">The scale factor (1, 2, 4, or 8)</param>
    /// <param name="baseRegister">The optional base register</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="size">The size of the memory access in bits</param>
    /// <param name="segmentOverride">Optional segment override</param>
    /// <returns>A scaled index memory operand</returns>
    public static ScaledIndexMemoryOperand CreateScaledIndexMemoryOperand(RegisterIndex indexRegister, int scale, RegisterIndex? baseRegister = null, 
                                                                        long displacement = 0, int size = 32, string? segmentOverride = null)
    {
        return new ScaledIndexMemoryOperand(indexRegister, scale, baseRegister, displacement, size, segmentOverride);
    }
    
    /// <summary>
    /// Creates a relative offset operand
    /// </summary>
    /// <param name="targetAddress">The target address</param>
    /// <param name="size">The size of the offset in bits (8 or 32)</param>
    /// <returns>A relative offset operand</returns>
    public static RelativeOffsetOperand CreateRelativeOffsetOperand(uint targetAddress, int size = 32)
    {
        return new RelativeOffsetOperand(targetAddress, size);
    }

    /// <summary>
    /// Creates an FPU register operand
    /// </summary>
    /// <param name="registerIndex">The FPU register index (RegisterIndex.A to RegisterIndex.Di)</param>
    /// <returns>An FPU register operand</returns>
    public static FPURegisterOperand CreateFPURegisterOperand(FpuRegisterIndex registerIndex)
    {
        return new FPURegisterOperand(registerIndex);
    }
}
