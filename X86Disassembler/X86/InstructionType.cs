namespace X86Disassembler.X86;

/// <summary>
/// Represents the type of x86 instruction
/// </summary>
public enum InstructionType
{
    // Data movement
    Mov,
    Push,
    Pop,
    Xchg,
    Lea,        // Load Effective Address
    
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
    Jmp,        // Jump unconditionally
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
    Jp,         // Jump if parity (even)
    Jnp,        // Jump if not parity (odd)
    Jcxz,       // Jump if CX zero
    Jecxz,      // Jump if ECX zero
    Loop,       // Loop
    Loope,      // Loop if equal
    Loopne,     // Loop if not equal
    Call,       // Call procedure
    Ret,        // Near return from procedure
    Retf,       // Far return from procedure
    Int,        // Interrupt
    Int3,       // Breakpoint interrupt
    Into,       // Interrupt if overflow
    Iret,       // Interrupt return
    
    // String operations
    MovsB,      // Move string byte
    MovsW,      // Move string word
    MovsD,      // Move string dword
    // Movs = MovsD, // Alias for MovsD - removed alias to avoid switch expression issues
    CmpsB,      // Compare string byte
    CmpsW,      // Compare string word
    CmpsD,      // Compare string dword
    StosB,      // Store string byte
    StosW,      // Store string word
    StosD,      // Store string dword
    // Stos = StosB, // Alias for StosB - removed alias to avoid switch expression issues
    ScasB,      // Scan string byte
    ScasW,      // Scan string word
    ScasD,      // Scan string dword
    // Scas = ScasB, // Alias for ScasB - removed alias to avoid switch expression issues
    LodsB,      // Load string byte
    LodsW,      // Load string word
    LodsD,      // Load string dword
    // Lods = LodsD, // Alias for LodsD - removed alias to avoid switch expression issues
    
    // REP prefixed instructions
    Rep,        // REP prefix
    RepE,       // REPE/REPZ prefix
    RepNE,      // REPNE/REPNZ prefix
    // RepneScas = RepNE, // Alias for RepNE - removed alias to avoid switch expression issues
    RepMovsB,   // REP MOVSB
    RepMovsW,   // REP MOVSW
    RepMovsD,   // REP MOVSD
    // RepMovs = RepMovsD, // Alias for RepMovsD - removed alias to avoid switch expression issues
    RepStosB,   // REP STOSB
    RepStosW,   // REP STOSW
    RepStosD,   // REP STOSD
    RepeCmpsB,  // REPE CMPSB
    RepeCmpsW,  // REPE CMPSW
    RepeCmpsD,  // REPE CMPSD
    RepneStosB, // REPNE STOSB
    RepneStosW, // REPNE STOSW
    RepneStosD, // REPNE STOSD
    RepScasB,   // REP SCASB
    RepScasW,   // REP SCASW
    RepScasD,   // REP SCASD
    RepneScasB, // REPNE SCASB
    RepneScasW, // REPNE SCASW
    RepneScasD, // REPNE SCASD
    RepLodsB,   // REP LODSB
    RepLodsW,   // REP LODSW
    RepLodsD,   // REP LODSD
    RepneCmpsB, // REPNE CMPSB
    RepneCmpsD, // REPNE CMPSD
    RepneCmpsW,  // REPNE CMPSW
    
    // Floating point
    Fld,        // Load floating point value
    Fst,        // Store floating point value
    Fstp,       // Store floating point value and pop
    Fadd,       // Add floating point
    Faddp,      // Add floating point and pop
    Fiadd,      // Add integer to floating point
    Fild,       // Load integer to floating point
    Fist,       // Store integer
    Fistp,      // Store integer and pop
    Fsub,       // Subtract floating point
    Fsubp,      // Subtract floating point and pop
    Fisub,      // Subtract integer from floating point
    Fsubr,      // Subtract floating point reversed
    Fsubrp,     // Subtract floating point reversed and pop
    Fisubr,     // Subtract floating point from integer (reversed)
    Fmul,       // Multiply floating point
    Fmulp,      // Multiply floating point and pop
    Fimul,      // Multiply integer with floating point
    Fdiv,       // Divide floating point
    Fdivp,      // Divide floating point and pop
    Fidiv,      // Divide integer by floating point
    Fdivr,      // Divide floating point reversed
    Fdivrp,     // Divide floating point reversed and pop
    Fidivr,     // Divide floating point by integer (reversed)
    Fcom,       // Compare floating point
    Ficom,      // Compare integer with floating point
    Fcomp,      // Compare floating point and pop
    Ficomp,     // Compare integer with floating point and pop
    Fcompp,     // Compare floating point and pop twice
    Fcomip,     // Compare floating point and pop, set EFLAGS
    Fcomi,      // Compare floating point, set EFLAGS
    Fucom,      // Unordered compare floating point
    Fucomp,     // Unordered compare floating point and pop
    Fucompp,    // Unordered compare floating point and pop twice
    Fucomip,    // Unordered compare floating point and pop, set EFLAGS
    Fucomi,     // Unordered compare floating point, set EFLAGS
    Ffreep,     // Free floating point register and pop
    Ffree,      // Free floating point register
    Fisttp,     // Store integer with truncation and pop
    Fbld,       // Load BCD
    Fbstp,      // Store BCD and pop
    Fnstsw,     // Store FPU status word without checking for pending unmasked exceptions
    Fstsw,      // Store FPU status word
    Fnstcw,     // Store FPU control word
    Fldcw,      // Load FPU control word
    Fxam,       // Examine floating point value
    Finit,      // Initialize FPU (with FWAIT prefix)
    Fninit,     // Initialize FPU without checking for pending unmasked exceptions
    Fclex,      // Clear floating-point exceptions
    Fnclex,     // Clear floating-point exceptions without checking for pending unmasked exceptions
    Fldenv,     // Load FPU environment
    Fnstenv,    // Store FPU environment
    Frstor,     // Restore FPU state
    Fnsave,     // Save FPU state without checking for pending unmasked exceptions
    Fsave,      // Save FPU state
    
    // Flag control instructions
    Stc,        // Set Carry Flag
    Clc,        // Clear Carry Flag
    Cmc,        // Complement Carry Flag
    Std,        // Set Direction Flag
    Cld,        // Clear Direction Flag
    Sti,        // Set Interrupt Flag
    Cli,        // Clear Interrupt Flag
    Sahf,       // Store AH into Flags
    Lahf,       // Load Flags into AH
    Fxch,       // Exchange floating point registers
    Fchs,       // Change sign of floating point value
    Fabs,       // Absolute value of floating point
    Ftst,       // Test floating point
    F2xm1,      // 2^x - 1
    Fyl2x,      // y * log2(x)
    Fyl2xp1,    // y * log2(x+1)
    Fptan,      // Partial tangent
    Fpatan,     // Partial arctangent
    Fxtract,    // Extract exponent and significand
    Fprem,      // Partial remainder
    Fprem1,     // Partial remainder (IEEE)
    Fdecstp,    // Decrement FPU stack pointer
    Fincstp,    // Increment FPU stack pointer
    Frndint,    // Round to integer
    Fscale,     // Scale by power of 2
    Fsin,       // Sine
    Fcos,       // Cosine
    Fsincos,    // Sine and cosine
    Fsqrt,      // Square root
    Fnop,       // No operation
    Fwait,      // Wait for FPU
    
    // Floating point conditional moves
    Fcmovb,     // FP conditional move if below
    Fcmove,     // FP conditional move if equal
    Fcmovbe,    // FP conditional move if below or equal
    Fcmovu,     // FP conditional move if unordered
    Fcmovnb,    // FP conditional move if not below
    Fcmovne,    // FP conditional move if not equal
    Fcmovnbe,   // FP conditional move if not below or equal
    Fcmovnu,    // FP conditional move if not unordered
    
    // System instructions
    Hlt,        // Halt
    Cpuid,      // CPU identification
    Rdtsc,      // Read time-stamp counter
    Wait,       // Wait for FPU
    Lock,       // Lock prefix
    In,         // Input from port
    Out,        // Output to port
    
    // Stack-related instructions
    Pushad,     // Push all general-purpose registers
    Popad,      // Pop all general-purpose registers
    Pushfd,     // Push EFLAGS register onto the stack
    Popfd,      // Pop stack into EFLAGS register
    Enter,      // Make stack frame for procedure parameters
    Leave,      // High level procedure exit
    
    // Other
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
