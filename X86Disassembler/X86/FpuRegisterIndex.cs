namespace X86Disassembler.X86;

/// <summary>
/// Represents the index values for x87 floating-point unit registers.
/// These values correspond to the encoding used in x87 FPU instructions
/// for identifying the specific ST(i) register operands.
/// </summary>
public enum FpuRegisterIndex
{
    /// <summary>FPU register ST(0)</summary>
    ST0 = 0,
    
    /// <summary>FPU register ST(1)</summary>
    ST1 = 1,
    
    /// <summary>FPU register ST(2)</summary>
    ST2 = 2,
    
    /// <summary>FPU register ST(3)</summary>
    ST3 = 3,
    
    /// <summary>FPU register ST(4)</summary>
    ST4 = 4,
    
    /// <summary>FPU register ST(5)</summary>
    ST5 = 5,
    
    /// <summary>FPU register ST(6)</summary>
    ST6 = 6,
    
    /// <summary>FPU register ST(7)</summary>
    ST7 = 7,
}