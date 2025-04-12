namespace X86Disassembler.X86;

/// <summary>
/// Represents the different types of x86 instructions
/// </summary>
public enum InstructionType
{
    /// <summary>
    /// Unknown or unrecognized instruction
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Data transfer instructions (e.g., MOV, PUSH, POP, XCHG)
    /// </summary>
    DataTransfer,
    
    /// <summary>
    /// Arithmetic instructions (e.g., ADD, SUB, MUL, DIV)
    /// </summary>
    Arithmetic,
    
    /// <summary>
    /// Logical instructions (e.g., AND, OR, XOR, NOT)
    /// </summary>
    Logical,
    
    /// <summary>
    /// Shift and rotate instructions (e.g., SHL, SHR, ROL, ROR)
    /// </summary>
    ShiftRotate,
    
    /// <summary>
    /// Control flow instructions (e.g., JMP, CALL, RET)
    /// </summary>
    ControlFlow,
    
    /// <summary>
    /// Conditional jump instructions (e.g., JE, JNE, JG, JL)
    /// </summary>
    ConditionalJump,
    
    /// <summary>
    /// String instructions (e.g., MOVS, CMPS, SCAS)
    /// </summary>
    String,
    
    /// <summary>
    /// I/O instructions (e.g., IN, OUT)
    /// </summary>
    IO,
    
    /// <summary>
    /// Flag control instructions (e.g., STC, CLC, CMC)
    /// </summary>
    FlagControl,
    
    /// <summary>
    /// Processor control instructions (e.g., HLT, WAIT)
    /// </summary>
    ProcessorControl,
    
    /// <summary>
    /// Floating-point instructions (e.g., FADD, FSUB, FMUL)
    /// </summary>
    FloatingPoint,
    
    /// <summary>
    /// SIMD instructions (e.g., MMX, SSE, AVX)
    /// </summary>
    SIMD
}
