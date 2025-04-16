namespace X86Disassembler.X86;

using Operands;

/// <summary>
/// Handles mapping between register indices and register enums
/// </summary>
public static class RegisterMapper
{
    /// <summary>
    /// Maps the register index from the ModR/M byte to the RegisterIndex enum value
    /// </summary>
    /// <param name="modRMRegIndex">The register index from the ModR/M byte (0-7)</param>
    /// <returns>The corresponding RegisterIndex enum value</returns>
    public static RegisterIndex MapModRMToRegisterIndex(int modRMRegIndex)
    {
        // The mapping from ModR/M register index to RegisterIndex enum is:
        // 0 -> A (EAX)
        // 1 -> C (ECX)
        // 2 -> D (EDX)
        // 3 -> B (EBX)
        // 4 -> Sp (ESP)
        // 5 -> Bp (EBP)
        // 6 -> Si (ESI)
        // 7 -> Di (EDI)
        return modRMRegIndex switch
        {
            0 => RegisterIndex.A,  // EAX
            1 => RegisterIndex.C,  // ECX
            2 => RegisterIndex.D,  // EDX
            3 => RegisterIndex.B,  // EBX
            4 => RegisterIndex.Sp, // ESP
            5 => RegisterIndex.Bp, // EBP
            6 => RegisterIndex.Si, // ESI
            7 => RegisterIndex.Di, // EDI
            _ => RegisterIndex.A   // Default to EAX
        };
    }

    /// <summary>
    /// Maps the register index from the ModR/M byte to the RegisterIndex8 enum value
    /// </summary>
    /// <param name="modRMRegIndex">The register index from the ModR/M byte (0-7)</param>
    /// <returns>The corresponding RegisterIndex8 enum value</returns>
    public static RegisterIndex8 MapModRMToRegisterIndex8(int modRMRegIndex)
    {
        // The mapping from ModR/M register index to RegisterIndex8 enum is direct:
        // 0 -> AL, 1 -> CL, 2 -> DL, 3 -> BL, 4 -> AH, 5 -> CH, 6 -> DH, 7 -> BH
        return (RegisterIndex8)modRMRegIndex;
    }
    
    /// <summary>
    /// Maps a RegisterIndex8 enum value to the corresponding RegisterIndex enum value for base registers
    /// </summary>
    /// <param name="regIndex8">The RegisterIndex8 enum value</param>
    /// <returns>The corresponding RegisterIndex enum value</returns>
    public static RegisterIndex MapRegister8ToBaseRegister(RegisterIndex8 regIndex8)
    {
        // Map 8-bit register indices to their corresponding 32-bit register indices
        return regIndex8 switch
        {
            RegisterIndex8.AL => RegisterIndex.A,
            RegisterIndex8.CL => RegisterIndex.C,
            RegisterIndex8.DL => RegisterIndex.D,
            RegisterIndex8.BL => RegisterIndex.B,
            RegisterIndex8.AH => RegisterIndex.A,
            RegisterIndex8.CH => RegisterIndex.C,
            RegisterIndex8.DH => RegisterIndex.D,
            RegisterIndex8.BH => RegisterIndex.B,
            _ => RegisterIndex.A // Default to EAX
        };
    }
    
    /// <summary>
    /// Gets the register name based on the register index and size
    /// </summary>
    /// <param name="regIndex">The register index as RegisterIndex enum</param>
    /// <param name="size">The register size (16, 32, or 64 bits)</param>
    /// <returns>The register name</returns>
    public static string GetRegisterName(RegisterIndex regIndex, int size)
    {
        return size switch
        {
            16 => Constants.RegisterNames16[(int)regIndex],
            32 => Constants.RegisterNames32[(int)regIndex],
            64 => Constants.RegisterNames32[(int)regIndex], // For now, reuse 32-bit names for 64-bit
            _ => "unknown"
        };
    }
    
    /// <summary>
    /// Gets the 8-bit register name based on the RegisterIndex8 enum value
    /// </summary>
    /// <param name="regIndex8">The register index as RegisterIndex8 enum</param>
    /// <returns>The 8-bit register name</returns>
    public static string GetRegisterName(RegisterIndex8 regIndex8)
    {
        return regIndex8.ToString().ToLower();
    }
}
