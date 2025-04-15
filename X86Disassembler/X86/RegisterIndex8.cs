namespace X86Disassembler.X86;

/// <summary>
/// Represents the index values for x86 8-bit registers.
/// These values correspond to the encoding used in ModR/M bytes
/// for 8-bit register operand identification in x86 instructions.
/// </summary>
public enum RegisterIndex8
{
    /// <summary>AL register (low byte of EAX)</summary>
    AL = 0,
    
    /// <summary>CL register (low byte of ECX)</summary>
    CL = 1,
    
    /// <summary>DL register (low byte of EDX)</summary>
    DL = 2,
    
    /// <summary>BL register (low byte of EBX)</summary>
    BL = 3,
    
    /// <summary>AH register (high byte of EAX)</summary>
    AH = 4,
    
    /// <summary>CH register (high byte of ECX)</summary>
    CH = 5,
    
    /// <summary>DH register (high byte of EDX)</summary>
    DH = 6,
    
    /// <summary>BH register (high byte of EBX)</summary>
    BH = 7
}
