namespace X86Disassembler.X86;

/// <summary>
/// Represents the index values for x86 general-purpose registers.
/// These values correspond to the encoding used in ModR/M and SIB bytes
/// for register operand identification in x86 instructions.
/// </summary>
public enum RegisterIndex
{
    /// <summary>A register (EAX/AX/AL depending on operand size)</summary>
    A = 0,
    
    /// <summary>B register (EBX/BX/BL depending on operand size)</summary>
    B = 1,
    
    /// <summary>C register (ECX/CX/CL depending on operand size)</summary>
    C = 2,
    
    /// <summary>D register (EDX/DX/DL depending on operand size)</summary>
    D = 3,
    
    /// <summary>Source Index register (ESI/SI)</summary>
    Si = 4,
    
    /// <summary>Destination Index register (EDI/DI)</summary>
    Di = 5,
    
    /// <summary>Stack Pointer register (ESP/SP)</summary>
    Sp = 6,
    
    /// <summary>Base Pointer register (EBP/BP)</summary>
    Bp = 7,
}