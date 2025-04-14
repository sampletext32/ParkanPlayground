namespace X86Disassembler.X86;

/// <summary>
/// Represents the type of x86 instruction
/// </summary>
public enum InstructionType
{
    // Data movement
    Move,
    Mov = Move,  // Alias for Move to match the mnemonic used in handlers
    Push,
    Pop,
    Xchg,
    
    // Arithmetic
    Add,
    Sub,
    Mul,
    IMul,
    Div,
    IDiv,
    Inc,
    Dec,
    Neg,
    Adc,        // Add with carry
    Sbb,        // Subtract with borrow
    
    // Logical
    And,
    Or,
    Xor,
    Not,
    Test,
    Cmp,
    Shl,        // Shift left
    Shr,        // Shift right logical
    Sar,        // Shift right arithmetic
    Rol,        // Rotate left
    Ror,        // Rotate right
    Rcl,        // Rotate left through carry
    Rcr,        // Rotate right through carry
    Bt,         // Bit test
    Bts,        // Bit test and set
    Btr,        // Bit test and reset
    Btc,        // Bit test and complement
    Bsf,        // Bit scan forward
    Bsr,        // Bit scan reverse
    
    // Control flow
    Jmp,
    Je,         // Jump if equal
    Jne,        // Jump if not equal
    Jg,         // Jump if greater
    Jge,        // Jump if greater or equal
    Jl,         // Jump if less
    Jle,        // Jump if less or equal
    Ja,         // Jump if above (unsigned)
    Jae,        // Jump if above or equal (unsigned)
    Jb,         // Jump if below (unsigned)
    Jbe,        // Jump if below or equal (unsigned)
    Jz,         // Jump if zero
    Jnz,        // Jump if not zero
    Jo,         // Jump if overflow
    Jno,        // Jump if not overflow
    Js,         // Jump if sign
    Jns,        // Jump if not sign
    Jcxz,       // Jump if CX zero
    Jecxz,      // Jump if ECX zero
    Loop,       // Loop
    Loope,      // Loop if equal
    Loopne,     // Loop if not equal
    Call,
    Ret,
    Int,        // Interrupt
    Into,       // Interrupt if overflow
    Iret,       // Interrupt return
    
    // String operations
    MovsB,
    MovsW,
    MovsD,
    Movs = MovsD, // Alias for MovsD
    CmpsB,
    CmpsW,
    CmpsD,
    StosB,
    StosW,
    StosD,
    Stos = StosB, // Alias for StosB
    ScasB,      // Scan string byte
    ScasW,      // Scan string word
    ScasD,      // Scan string dword
    Scas = ScasB, // Alias for ScasB
    LodsB,      // Load string byte
    LodsW,      // Load string word
    LodsD,      // Load string dword
    Lods = LodsD, // Alias for LodsD
    
    // REP prefixed instructions
    Rep,        // REP prefix
    RepE,       // REPE/REPZ prefix
    RepNE,      // REPNE/REPNZ prefix
    RepneScas = RepNE, // Alias for RepNE
    RepMovsB,   // REP MOVSB
    RepMovsW,   // REP MOVSW
    RepMovsD,   // REP MOVSD
    RepMovs = RepMovsD, // Alias for RepMovsD
    RepeCmpsB,  // REPE CMPSB
    RepeCmpsW,  // REPE CMPSW
    RepeCmpsD,  // REPE CMPSD
    RepneStosB, // REPNE STOSB
    RepneStosW, // REPNE STOSW
    RepneStosD, // REPNE STOSD
    RepScasB,   // REP SCASB
    RepScasW,   // REP SCASW
    RepScasD,   // REP SCASD
    RepLodsB,   // REP LODSB
    RepLodsW,   // REP LODSW
    RepLodsD,   // REP LODSD
    
    // Floating point
    Fld,        // Load floating point value
    Fst,        // Store floating point value
    Fstp,       // Store floating point value and pop
    Fadd,       // Add floating point
    Fiadd,      // Add integer to floating point
    Fild,       // Load integer to floating point
    Fsub,       // Subtract floating point
    Fsubr,      // Subtract floating point reversed
    Fmul,       // Multiply floating point
    Fdiv,       // Divide floating point
    Fdivr,      // Divide floating point reversed
    Fcom,       // Compare floating point
    Fcomp,      // Compare floating point and pop
    Fcompp,     // Compare floating point and pop twice
    Fnstsw,     // Store FPU status word
    Fnstcw,     // Store FPU control word
    Fldcw,      // Load FPU control word
    Fxch,       // Exchange floating point registers
    Fchs,       // Change sign of floating point value
    Fabs,       // Absolute value of floating point
    Ftst,       // Test floating point
    Fxam,       // Examine floating point
    
    // System instructions
    Hlt,        // Halt
    Cpuid,      // CPU identification
    Rdtsc,      // Read time-stamp counter
    
    // Other
    Lea,        // Load effective address
    Nop,        // No operation
    Cdq,        // Convert doubleword to quadword
    Cwde,       // Convert word to doubleword
    Cwd,        // Convert word to doubleword
    Cbw,        // Convert byte to word
    Movsx,      // Move with sign-extend
    Movzx,      // Move with zero-extend
    Setcc,      // Set byte on condition
    Cmov,       // Conditional move
    
    // Unknown
    Unknown
}
