using X86Disassembler.X86.Handlers.Adc;
using X86Disassembler.X86.Handlers.Add;
using X86Disassembler.X86.Handlers.And;
using X86Disassembler.X86.Handlers.Bit;
using X86Disassembler.X86.Handlers.Call;
using X86Disassembler.X86.Handlers.Cmp;
using X86Disassembler.X86.Handlers.Dec;
using X86Disassembler.X86.Handlers.Div;
using X86Disassembler.X86.Handlers.FloatingPoint;
using X86Disassembler.X86.Handlers.Idiv;
using X86Disassembler.X86.Handlers.Imul;
using X86Disassembler.X86.Handlers.Inc;
using X86Disassembler.X86.Handlers.Jump;
using X86Disassembler.X86.Handlers.Lea;
using X86Disassembler.X86.Handlers.Misc;
using X86Disassembler.X86.Handlers.Mov;
using X86Disassembler.X86.Handlers.Mul;
using X86Disassembler.X86.Handlers.Neg;
using X86Disassembler.X86.Handlers.Nop;
using X86Disassembler.X86.Handlers.Not;
using X86Disassembler.X86.Handlers.Or;
using X86Disassembler.X86.Handlers.Pop;
using X86Disassembler.X86.Handlers.Push;
using X86Disassembler.X86.Handlers.Ret;
using X86Disassembler.X86.Handlers.Sbb;
using X86Disassembler.X86.Handlers.Shift;
using X86Disassembler.X86.Handlers.String;
using X86Disassembler.X86.Handlers.Sub;
using X86Disassembler.X86.Handlers.Test;
using X86Disassembler.X86.Handlers.Xchg;
using X86Disassembler.X86.Handlers.Xor;
using X86Disassembler.X86.Handlers.Flags;

namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Factory for creating instruction handlers
/// </summary>
public class InstructionHandlerFactory
{
    private readonly List<IInstructionHandler> _handlers = [];
    private readonly InstructionDecoder _decoder;

    /// <summary>
    /// Initializes a new instance of the InstructionHandlerFactory class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this factory</param>
    public InstructionHandlerFactory(InstructionDecoder decoder)
    {
        _decoder = decoder;

        RegisterAllHandlers();
    }

    /// <summary>
    /// Registers all handlers
    /// </summary>
    private void RegisterAllHandlers()
    {
        // Register specific instruction handlers
        _handlers.Add(new Int3Handler(_decoder));

        // Register handlers in order of priority (most specific first)
        RegisterSbbHandlers();        // SBB instructions
        RegisterAdcHandlers();        // ADC instructions
        RegisterAddHandlers();        // ADD instructions
        RegisterAndHandlers();        // AND instructions
        RegisterOrHandlers();         // OR instructions
        RegisterXorHandlers();        // XOR instructions
        RegisterCmpHandlers();        // CMP instructions
        RegisterTestHandlers();       // TEST instructions
        
        // Register arithmetic unary instructions
        RegisterNotHandlers();       // NOT instructions
        RegisterNegHandlers();       // NEG instructions
        RegisterMulHandlers();       // MUL instructions
        RegisterImulHandlers();      // IMUL instructions
        RegisterDivHandlers();       // DIV instructions
        RegisterIdivHandlers();      // IDIV instructions
        RegisterXchgHandlers();     // XCHG
        RegisterJumpHandlers();      // JMP instructions
        RegisterCallHandlers();      // CALL instructions
        RegisterReturnHandlers();    // RET instructions
        RegisterDecHandlers();       // DEC instructions
        RegisterIncHandlers();      // INC/DEC handlers after Group 1 handlers
        RegisterPushHandlers();      // PUSH instructions
        RegisterPopHandlers();       // POP instructions
        RegisterLeaHandlers();       // LEA instructions
        RegisterFloatingPointHandlers(); // FPU instructions
        RegisterStringHandlers();    // String instructions
        RegisterMovHandlers();       // MOV instructions
        RegisterSubHandlers(); // Register SUB handlers
        RegisterNopHandlers(); // Register NOP handlers
        RegisterBitHandlers(); // Register bit manipulation handlers
        RegisterMiscHandlers(); // Register miscellaneous instructions
        RegisterShiftHandlers(); // Register shift and rotate instructions
        RegisterFlagHandlers(); // Register flag manipulation instructions
    }

    /// <summary>
    /// Registers all SBB instruction handlers
    /// </summary>
    private void RegisterSbbHandlers()
    {
        // SBB register-register and register-memory handlers for 8-bit operands
        _handlers.Add(new SbbRm8R8Handler(_decoder));              // SBB r/m8, r8 (opcode 18)
        _handlers.Add(new SbbR8Rm8Handler(_decoder));              // SBB r8, r/m8 (opcode 1A)

        // SBB register-register and register-memory handlers for 16-bit operands (with 0x66 prefix)
        _handlers.Add(new SbbRm16R16Handler(_decoder));            // SBB r/m16, r16 (opcode 19 with 0x66 prefix)
        _handlers.Add(new SbbR16Rm16Handler(_decoder));            // SBB r16, r/m16 (opcode 1B with 0x66 prefix)

        // SBB register-register and register-memory handlers for 32-bit operands
        _handlers.Add(new SbbRm32R32Handler(_decoder));            // SBB r/m32, r32 (opcode 19)
        _handlers.Add(new SbbR32Rm32Handler(_decoder));            // SBB r32, r/m32 (opcode 1B)

        // SBB immediate handlers for 8-bit operands
        _handlers.Add(new SbbImmFromRm8Handler(_decoder));           // SBB r/m8, imm8 (opcode 80 /3)
        _handlers.Add(new SbbAlImmHandler(_decoder));               // SBB AL, imm8 (opcode 1C)

        // SBB immediate handlers for 16-bit operands (with 0x66 prefix)
        _handlers.Add(new SbbImmFromRm16Handler(_decoder));          // SBB r/m16, imm16 (opcode 81 /3 with 0x66 prefix)
        _handlers.Add(new SbbImmFromRm16SignExtendedHandler(_decoder)); // SBB r/m16, imm8 (opcode 83 /3 with 0x66 prefix)

        // SBB immediate handlers for 32-bit operands
        _handlers.Add(new SbbImmFromRm32Handler(_decoder));         // SBB r/m32, imm32 (opcode 81 /3)
        _handlers.Add(new SbbImmFromRm32SignExtendedHandler(_decoder)); // SBB r/m32, imm8 (opcode 83 /3)

        // SBB accumulator handlers
        _handlers.Add(new SbbAccumulatorImmHandler(_decoder));     // SBB AX/EAX, imm16/32 (opcode 1D)
    }
    
    /// <summary>
    /// Registers all ADC instruction handlers
    /// </summary>
    private void RegisterAdcHandlers()
    {
        // ADC immediate handlers
        _handlers.Add(new AdcImmToRm8Handler(_decoder));           // ADC r/m8, imm8 (opcode 80 /2)
        _handlers.Add(new AdcImmToRm16Handler(_decoder));          // ADC r/m16, imm16 (opcode 81 /2 with 0x66 prefix)
        _handlers.Add(new AdcImmToRm16SignExtendedHandler(_decoder)); // ADC r/m16, imm8 (opcode 83 /2 with 0x66 prefix)
        _handlers.Add(new AdcImmToRm32Handler(_decoder));         // ADC r/m32, imm32 (opcode 81 /2)
        _handlers.Add(new AdcImmToRm32SignExtendedHandler(_decoder)); // ADC r/m32, imm8 (opcode 83 /2)
        _handlers.Add(new AdcAlImmHandler(_decoder));             // ADC AL, imm8 (opcode 14)
        _handlers.Add(new AdcAccumulatorImmHandler(_decoder));     // ADC AX/EAX, imm16/32 (opcode 15)

        // Register-to-register ADC handlers (8-bit)
        _handlers.Add(new AdcR8Rm8Handler(_decoder));      // ADC r8, r/m8 (opcode 12)
        _handlers.Add(new AdcRm8R8Handler(_decoder));      // ADC r/m8, r8 (opcode 10)
        
        // Register-to-register ADC handlers (16-bit)
        _handlers.Add(new AdcR16Rm16Handler(_decoder));    // ADC r16, r/m16 (opcode 13 with 0x66 prefix)
        _handlers.Add(new AdcRm16R16Handler(_decoder));    // ADC r/m16, r16 (opcode 11 with 0x66 prefix)
        
        // Register-to-register ADC handlers (32-bit)
        _handlers.Add(new AdcR32Rm32Handler(_decoder));    // ADC r32, r/m32 (opcode 13)
        _handlers.Add(new AdcRm32R32Handler(_decoder));    // ADC r/m32, r32 (opcode 11)
    }

    /// <summary>
    /// Registers all Return instruction handlers
    /// </summary>
    private void RegisterReturnHandlers()
    {
        // Add Return handlers
        _handlers.Add(new RetHandler(_decoder));
        _handlers.Add(new RetImmHandler(_decoder));

        // Add Far Return handlers
        // 16-bit handlers with operand size prefix (must come first)
        _handlers.Add(new Retf16Handler(_decoder));       // RETF (16-bit) (opcode 0xCB with 0x66 prefix)
        _handlers.Add(new RetfImm16Handler(_decoder));    // RETF imm16 (16-bit) (opcode 0xCA with 0x66 prefix)

        // 32-bit handlers (default)
        _handlers.Add(new RetfHandler(_decoder));         // RETF (32-bit) (opcode 0xCB)
        _handlers.Add(new RetfImmHandler(_decoder));      // RETF imm16 (32-bit) (opcode 0xCA)
    }

    /// <summary>
    /// Registers all Call instruction handlers
    /// </summary>
    private void RegisterCallHandlers()
    {
        // Add Call handlers
        _handlers.Add(new CallRel32Handler(_decoder));        // CALL rel32 (opcode E8)
        _handlers.Add(new CallRm32Handler(_decoder));         // CALL r/m32 (opcode FF /2)
        _handlers.Add(new CallFarPtrHandler(_decoder));       // CALL m16:32 (opcode FF /3) - Far call
    }

    /// <summary>
    /// Registers all Jump instruction handlers
    /// </summary>
    private void RegisterJumpHandlers()
    {
        // JMP handlers for relative jumps
        _handlers.Add(new JmpRel32Handler(_decoder));  // JMP rel32 (opcode E9)
        _handlers.Add(new JmpRel8Handler(_decoder));   // JMP rel8 (opcode EB)
        
        // JMP handler for register/memory operands
        _handlers.Add(new JmpRm32Handler(_decoder));   // JMP r/m32 (opcode FF /4)
        
        // Conditional jump handlers
        _handlers.Add(new JgeRel8Handler(_decoder));   // JGE rel8 (opcode 0F 8D)
        _handlers.Add(new ConditionalJumpHandler(_decoder)); // Short conditional jumps
        _handlers.Add(new TwoByteConditionalJumpHandler(_decoder)); // Long conditional jumps
    }

    /// <summary>
    /// Registers all Test instruction handlers
    /// </summary>
    private void RegisterTestHandlers()
    {
        // TEST handlers
        _handlers.Add(new TestImmWithRm32Handler(_decoder)); // TEST r/m32, imm32 (opcode A9)
        _handlers.Add(new TestImmWithRm8Handler(_decoder)); // TEST r/m8, imm8 (opcode F6 /0)
        _handlers.Add(new TestRegMem8Handler(_decoder)); // TEST r8, r/m8 (opcode 84 /0)
        _handlers.Add(new TestRegMemHandler(_decoder)); // TEST r32, r/m32 (opcode 85 /0)
        _handlers.Add(new TestAlImmHandler(_decoder)); // TEST AL, imm8 (opcode A8)
        _handlers.Add(new TestEaxImmHandler(_decoder)); // TEST EAX, imm32 (opcode A9)
    }

    /// <summary>
    /// Registers all Xor instruction handlers
    /// </summary>
    private void RegisterXorHandlers()
    {
        // 16-bit handlers
        _handlers.Add(new XorRm16R16Handler(_decoder));              // XOR r/m16, r16 (opcode 31)
        _handlers.Add(new XorR16Rm16Handler(_decoder));              // XOR r16, r/m16 (opcode 33)
        _handlers.Add(new XorImmWithRm16Handler(_decoder));          // XOR r/m16, imm16 (opcode 81 /6)
        _handlers.Add(new XorImmWithRm16SignExtendedHandler(_decoder)); // XOR r/m16, imm8 (opcode 83 /6)
        
        // 32-bit handlers
        _handlers.Add(new XorMemRegHandler(_decoder));               // XOR r/m32, r32 (opcode 31)
        _handlers.Add(new XorRegMemHandler(_decoder));               // XOR r32, r/m32 (opcode 33)
        _handlers.Add(new XorImmWithRm32Handler(_decoder));          // XOR r/m32, imm32 (opcode 81 /6)
        _handlers.Add(new XorImmWithRm32SignExtendedHandler(_decoder)); // XOR r/m32, imm8 (opcode 83 /6)

        // 8-bit handlers
        _handlers.Add(new XorRm8R8Handler(_decoder));                // XOR r/m8, r8 (opcode 30)
        _handlers.Add(new XorR8Rm8Handler(_decoder));                // XOR r8, r/m8 (opcode 32)
        _handlers.Add(new XorAlImmHandler(_decoder));                // XOR AL, imm8 (opcode 34)
        _handlers.Add(new XorImmWithRm8Handler(_decoder));           // XOR r/m8, imm8 (opcode 80 /6)

        // special treatment with xor-ing eax
        // precise handlers go first
        _handlers.Add(new XorAxImm16Handler(_decoder));              // XOR AX, imm16 (opcode 35)
        _handlers.Add(new XorEaxImmHandler(_decoder));               // XOR EAX, imm32 (opcode 35)
    }

    /// <summary>
    /// Registers all Or instruction handlers
    /// </summary>
    private void RegisterOrHandlers()
    {
        // Add OR immediate handlers
        _handlers.Add(new OrImmToRm8Handler(_decoder));              // OR r/m8, imm8 (opcode 80 /1)
        _handlers.Add(new OrImmToRm32Handler(_decoder));            // OR r/m32, imm32 (opcode 81 /1)
        _handlers.Add(new OrImmToRm32SignExtendedHandler(_decoder)); // OR r/m32, imm8 (opcode 83 /1)

        // Add OR register handlers
        _handlers.Add(new OrR8Rm8Handler(_decoder));                // OR r8, r/m8 (opcode 0A)
        _handlers.Add(new OrRm8R8Handler(_decoder));                // OR r/m8, r8 (opcode 08)
        _handlers.Add(new OrR32Rm32Handler(_decoder));              // OR r32, r/m32 (opcode 0B)
        _handlers.Add(new OrRm32R32Handler(_decoder));              // OR r/m32, r32 (opcode 09)
        
        // Add OR immediate with accumulator handlers
        _handlers.Add(new OrAlImmHandler(_decoder));                // OR AL, imm8 (opcode 0C)
        _handlers.Add(new OrEaxImmHandler(_decoder));               // OR EAX, imm32 (opcode 0D)
    }

    /// <summary>
    /// Registers all Lea instruction handlers
    /// </summary>
    private void RegisterLeaHandlers()
    {
        // Add Lea handlers
        _handlers.Add(new LeaR32MHandler(_decoder)); // LEA r32, m (opcode 8D)
    }

    /// <summary>
    /// Registers all Cmp instruction handlers
    /// </summary>
    private void RegisterCmpHandlers()
    {
        // Add Cmp handlers for 32-bit operands
        _handlers.Add(new CmpR32Rm32Handler(_decoder));  // CMP r32, r/m32 (opcode 3B)
        _handlers.Add(new CmpRm32R32Handler(_decoder));  // CMP r/m32, r32 (opcode 39)
        
        // Add Cmp handlers for 8-bit operands
        _handlers.Add(new CmpRm8R8Handler(_decoder));  // CMP r/m8, r8 (opcode 38)
        _handlers.Add(new CmpR8Rm8Handler(_decoder));  // CMP r8, r/m8 (opcode 3A)
        
        // Add Cmp handlers for immediate operands
        _handlers.Add(new CmpImmWithRm8Handler(_decoder)); // CMP r/m8, imm8 (opcode 80 /7)
        _handlers.Add(new CmpAlImmHandler(_decoder));  // CMP AL, imm8 (opcode 3C)
        _handlers.Add(new CmpEaxImmHandler(_decoder)); // CMP EAX, imm32 (opcode 3D)

        // Add CMP immediate handlers from ArithmeticImmediate namespace
        _handlers.Add(new CmpImmWithRm32Handler(_decoder)); // CMP r/m32, imm32 (opcode 81 /7)
        _handlers.Add(new CmpImmWithRm32SignExtendedHandler(_decoder)); // CMP r/m32, imm8 (opcode 83 /7)
    }

    /// <summary>
    /// Registers all Dec instruction handlers
    /// </summary>
    private void RegisterDecHandlers()
    {
        // Add Dec handlers
        _handlers.Add(new DecRegHandler(_decoder)); // DEC r/m8 (opcode FE)
        
        // _handlers.Add(new DecMem8Handler(_decoder)); // DEC r/m16 (opcode FF /1) and DEC r/m32 (opcode FF /1)
    }

    /// <summary>
    /// Registers all Inc instruction handlers
    /// </summary>
    private void RegisterIncHandlers()
    {
        // Add Inc handlers
        _handlers.Add(new IncRegHandler(_decoder)); // INC r/m8 (opcode FE)

        // _handlers.Add(new IncMem8Handler(_decoder)); // INC r/m16 (opcode FF /0) and INC r/m32 (opcode FF /0)
    }

    /// <summary>
    /// Registers all Add instruction handlers
    /// </summary>
    private void RegisterAddHandlers()
    {
        // Add ADD register-to-register handlers (32-bit)
        _handlers.Add(new AddR32Rm32Handler(_decoder));    // ADD r32, r/m32 (opcode 03)
        _handlers.Add(new AddRm32R32Handler(_decoder));    // ADD r/m32, r32 (opcode 01)
        _handlers.Add(new AddEaxImmHandler(_decoder));     // ADD EAX, imm32 (opcode 05 without 0x66 prefix)
        _handlers.Add(new AddAxImmHandler(_decoder));      // ADD AX, imm16 (opcode 05 with 0x66 prefix)
        
        // Add ADD register-to-register handlers (16-bit)
        _handlers.Add(new AddR16Rm16Handler(_decoder));    // ADD r16, r/m16 (opcode 03 with 0x66 prefix)
        _handlers.Add(new AddRm16R16Handler(_decoder));    // ADD r/m16, r16 (opcode 01 with 0x66 prefix)
        
        // Add ADD register-to-register handlers (8-bit)
        _handlers.Add(new AddRm8R8Handler(_decoder));      // ADD r/m8, r8 (opcode 00)
        _handlers.Add(new AddR8Rm8Handler(_decoder));      // ADD r8, r/m8 (opcode 02)
        _handlers.Add(new AddAlImmHandler(_decoder));      // ADD AL, imm8 (opcode 04)

        // Add ADD immediate handlers
        _handlers.Add(new AddImmToRm8Handler(_decoder));           // ADD r/m8, imm8 (opcode 80 /0)
        _handlers.Add(new AddImmToRm16Handler(_decoder));          // ADD r/m16, imm16 (opcode 81 /0 with 0x66 prefix)
        _handlers.Add(new AddImmToRm16SignExtendedHandler(_decoder)); // ADD r/m16, imm8 (opcode 83 /0 with 0x66 prefix)
        _handlers.Add(new AddImmToRm32Handler(_decoder));          // ADD r/m32, imm32 (opcode 81 /0)
        _handlers.Add(new AddImmToRm32SignExtendedHandler(_decoder)); // ADD r/m32, imm8 (opcode 83 /0)
    }

    /// <summary>
    /// Registers all XCHG instruction handlers
    /// </summary>
    private void RegisterXchgHandlers()
    {
        // Add XCHG handlers
        _handlers.Add(new XchgEaxRegHandler(_decoder)); // XCHG EAX, r32 (opcode 90 + register)
    }

    /// <summary>
    /// Registers all Floating Point instruction handlers
    /// </summary>
    private void RegisterFloatingPointHandlers()
    {
        // Add specialized Floating Point handlers
        
        // D8 opcode handlers (float32 operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FaddFloat32Handler(_decoder));  // FADD float32 (D8 /0)
        _handlers.Add(new FloatingPoint.Arithmetic.FmulFloat32Handler(_decoder));  // FMUL float32 (D8 /1)
        _handlers.Add(new FloatingPoint.Comparison.FcomFloat32Handler(_decoder));  // FCOM float32 (D8 /2)
        _handlers.Add(new FloatingPoint.Comparison.FcompFloat32Handler(_decoder)); // FCOMP float32 (D8 /3)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubFloat32Handler(_decoder));  // FSUB float32 (D8 /4)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubrFloat32Handler(_decoder)); // FSUBR float32 (D8 /5)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivFloat32Handler(_decoder));  // FDIV float32 (D8 /6)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivrFloat32Handler(_decoder)); // FDIVR float32 (D8 /7)
        
        // D9 opcode handlers (load/store float32 and control operations)
        _handlers.Add(new FloatingPoint.LoadStore.FldFloat32Handler(_decoder));   // FLD float32 (D9 /0)
        _handlers.Add(new FloatingPoint.LoadStore.FstFloat32Handler(_decoder));   // FST float32 (D9 /2)
        _handlers.Add(new FloatingPoint.LoadStore.FstpFloat32Handler(_decoder));  // FSTP float32 (D9 /3)
        _handlers.Add(new FloatingPoint.Control.FldenvHandler(_decoder));         // FLDENV (D9 /4)
        _handlers.Add(new FloatingPoint.Control.FldcwHandler(_decoder));          // FLDCW (D9 /5)
        _handlers.Add(new FloatingPoint.Control.FnstenvHandler(_decoder));        // FNSTENV (D9 /6)
        _handlers.Add(new FloatingPoint.Control.FnstcwHandler(_decoder));         // FNSTCW (D9 /7)
        
        // DA opcode handlers (int32 operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FiaddInt32Handler(_decoder));   // FIADD int32 (DA /0)
        _handlers.Add(new FloatingPoint.Arithmetic.FimulInt32Handler(_decoder));   // FIMUL int32 (DA /1)
        _handlers.Add(new FloatingPoint.Comparison.FicomInt32Handler(_decoder));   // FICOM int32 (DA /2)
        _handlers.Add(new FloatingPoint.Comparison.FicompInt32Handler(_decoder));  // FICOMP int32 (DA /3)
        _handlers.Add(new FloatingPoint.Arithmetic.FisubInt32Handler(_decoder));   // FISUB int32 (DA /4)
        _handlers.Add(new FloatingPoint.Arithmetic.FisubrInt32Handler(_decoder));  // FISUBR int32 (DA /5)
        _handlers.Add(new FloatingPoint.Arithmetic.FidivInt32Handler(_decoder));   // FIDIV int32 (DA /6)
        _handlers.Add(new FloatingPoint.Arithmetic.FidivrInt32Handler(_decoder));  // FIDIVR int32 (DA /7)
        
        // DD opcode handlers (load/store float64 operations)
        _handlers.Add(new FloatingPoint.LoadStore.FldFloat64Handler(_decoder));   // FLD float64 (DD /0)
        _handlers.Add(new FloatingPoint.LoadStore.FstFloat64Handler(_decoder));   // FST float64 (DD /2)
        _handlers.Add(new FloatingPoint.LoadStore.FstpFloat64Handler(_decoder));  // FSTP float64 (DD /3)
        
        // Register-register operations
        _handlers.Add(new FloatingPoint.Control.FxchHandler(_decoder));           // FXCH (D9 C8-CF)
        
        // Special floating point instructions
        _handlers.Add(new FloatingPoint.Arithmetic.FchsHandler(_decoder));        // FCHS (D9 E0)
        _handlers.Add(new FloatingPoint.Arithmetic.FabsHandler(_decoder));        // FABS (D9 E1)
        _handlers.Add(new FloatingPoint.Comparison.FtstHandler(_decoder));        // FTST (D9 E4)
        _handlers.Add(new FloatingPoint.Control.FxamHandler(_decoder));           // FXAM (D9 E5)
        
        // Transcendental functions
        _handlers.Add(new FloatingPoint.Transcendental.F2xm1Handler(_decoder));   // F2XM1 (D9 F0)
        _handlers.Add(new FloatingPoint.Transcendental.Fyl2xHandler(_decoder));   // FYL2X (D9 F1)
        _handlers.Add(new FloatingPoint.Transcendental.FptanHandler(_decoder));   // FPTAN (D9 F2)
        _handlers.Add(new FloatingPoint.Transcendental.FpatanHandler(_decoder));  // FPATAN (D9 F3)
        _handlers.Add(new FloatingPoint.Arithmetic.FxtractHandler(_decoder));    // FXTRACT (D9 F4)
        _handlers.Add(new FloatingPoint.Arithmetic.Fprem1Handler(_decoder));     // FPREM1 (D9 F5)
        
        // Other floating point handlers
        _handlers.Add(new FloatingPoint.Control.FnstswHandler(_decoder));         // FNSTSW AX (DF E0)
        _handlers.Add(new FloatingPoint.Control.FstswHandler(_decoder));           // FSTSW AX (9B DF E0)
        _handlers.Add(new FloatingPoint.Control.FstswMemHandler(_decoder));        // FSTSW m2byte (9B DD /7)
        
        // DB opcode handlers (int32 operations and extended precision)
        _handlers.Add(new FloatingPoint.LoadStore.FildInt32Handler(_decoder));     // FILD int32 (DB /0)
        _handlers.Add(new FloatingPoint.LoadStore.FistInt32Handler(_decoder));     // FIST int32 (DB /2)
        _handlers.Add(new FloatingPoint.LoadStore.FistpInt32Handler(_decoder));    // FISTP int32 (DB /3)
        _handlers.Add(new FloatingPoint.LoadStore.FldExtendedHandler(_decoder));   // FLD extended-precision (DB /5)
        _handlers.Add(new FloatingPoint.LoadStore.FstpExtendedHandler(_decoder));  // FSTP extended-precision (DB /7)
        
        // DB opcode handlers (conditional move instructions)
        _handlers.Add(new FloatingPoint.Conditional.FcmovnbHandler(_decoder));     // FCMOVNB (DB C0-C7)
        _handlers.Add(new FloatingPoint.Conditional.FcmovneHandler(_decoder));     // FCMOVNE (DB C8-CF)
        _handlers.Add(new FloatingPoint.Conditional.FcmovnbeHandler(_decoder));    // FCMOVNBE (DB D0-D7)
        _handlers.Add(new FloatingPoint.Conditional.FcmovnuHandler(_decoder));     // FCMOVNU (DB D8-DF)
        
        // DB opcode handlers (control instructions)
        _handlers.Add(new FloatingPoint.Control.FnclexHandler(_decoder));          // FNCLEX (DB E2)
        _handlers.Add(new FloatingPoint.Control.FclexHandler(_decoder));           // FCLEX (9B DB E2)
        _handlers.Add(new FloatingPoint.Control.FninitHandler(_decoder));          // FNINIT (DB E3)
        _handlers.Add(new FloatingPoint.Control.FinitHandler(_decoder));           // FINIT (9B DB E3)
        
        // DB opcode handlers (comparison instructions)
        _handlers.Add(new FloatingPoint.Comparison.FucomiHandler(_decoder));       // FUCOMI (DB E8-EF)
        _handlers.Add(new FloatingPoint.Comparison.FcomiHandler(_decoder));        // FCOMI (DB F0-F7)

        // D8 opcode handlers (register operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FaddStStiHandler(_decoder));         // FADD ST, ST(i) (D8 C0-C7)
        _handlers.Add(new FloatingPoint.Arithmetic.FmulStStiHandler(_decoder));         // FMUL ST, ST(i) (D8 C8-CF)
        _handlers.Add(new FloatingPoint.Comparison.FcomStHandler(_decoder));           // FCOM ST(i) (D8 D0-D7)
        _handlers.Add(new FloatingPoint.Comparison.FcompStHandler(_decoder));          // FCOMP ST(i) (D8 D8-DF)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubStStiHandler(_decoder));         // FSUB ST, ST(i) (D8 E0-E7)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubrStStiHandler(_decoder));        // FSUBR ST, ST(i) (D8 E8-EF)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivStStiHandler(_decoder));         // FDIV ST, ST(i) (D8 F0-F7)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivrStStiHandler(_decoder));        // FDIVR ST, ST(i) (D8 F8-FF)
        
        // DC opcode handlers (memory operations - float64)
        _handlers.Add(new FloatingPoint.Arithmetic.FaddFloat64Handler(_decoder));      // FADD float64 (DC /0)
        _handlers.Add(new FloatingPoint.Arithmetic.FmulFloat64Handler(_decoder));      // FMUL float64 (DC /1)
        _handlers.Add(new FloatingPoint.Comparison.FcomFloat64Handler(_decoder));      // FCOM float64 (DC /2)
        _handlers.Add(new FloatingPoint.Comparison.FcompFloat64Handler(_decoder));     // FCOMP float64 (DC /3)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubFloat64Handler(_decoder));      // FSUB float64 (DC /4)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubrFloat64Handler(_decoder));     // FSUBR float64 (DC /5)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivFloat64Handler(_decoder));      // FDIV float64 (DC /6)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivrFloat64Handler(_decoder));     // FDIVR float64 (DC /7)
        
        // DC opcode handlers (register operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FaddStiStHandler(_decoder));        // FADD ST(i), ST (DC C0-C7)
        _handlers.Add(new FloatingPoint.Arithmetic.FmulStiStHandler(_decoder));        // FMUL ST(i), ST (DC C8-CF)
        _handlers.Add(new FloatingPoint.Comparison.FcomRegisterHandler(_decoder));     // FCOM ST(i), ST(0) (DC D0-D7)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubStiStHandler(_decoder));        // FSUB ST(i), ST (DC E0-E7)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubrStiStHandler(_decoder));       // FSUB ST(i), ST (DC E8-EF)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivrStiStHandler(_decoder));        // FDIVR ST(i), ST (DC F0-F7)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivStiStHandler(_decoder));       // FDIV ST(i), ST (DC F8-FF)
        _handlers.Add(new FloatingPoint.Comparison.FcompRegisterHandler(_decoder));    // FCOMP ST(i), ST(0) (DC D8-DF)
        
        // DD opcode handlers (register operations)
        _handlers.Add(new FloatingPoint.Control.FfreeHandler(_decoder));           // FFREE ST(i) (DD C0-C7)
        _handlers.Add(new FloatingPoint.LoadStore.FstRegisterHandler(_decoder));    // FST ST(i) (DD D0-D7)
        _handlers.Add(new FloatingPoint.LoadStore.FstpRegisterHandler(_decoder));   // FSTP ST(i) (DD D8-DF)
        _handlers.Add(new FloatingPoint.Comparison.FucomHandler(_decoder));        // FUCOM ST(i) (DD E0-E7)
        _handlers.Add(new FloatingPoint.Comparison.FucompHandler(_decoder));       // FUCOMP ST(i) (DD E8-EF)
        
        // DD opcode handlers (memory operations)
        _handlers.Add(new FloatingPoint.Control.FrstorHandler(_decoder));          // FRSTOR (DD /4)
        _handlers.Add(new FloatingPoint.Control.FnsaveHandler(_decoder));          // FNSAVE (DD /6)
        _handlers.Add(new FloatingPoint.Control.FnstswMemoryHandler(_decoder));    // FNSTSW memory (DD /7)
        
        // DE opcode handlers (memory operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FiaddInt16Handler(_decoder));    // FIADD int16 (DE /0)
        _handlers.Add(new FloatingPoint.Arithmetic.FimulInt16Handler(_decoder));    // FIMUL int16 (DE /1)
        _handlers.Add(new FloatingPoint.Comparison.FicomInt16Handler(_decoder));    // FICOM int16 (DE /2)
        _handlers.Add(new FloatingPoint.Comparison.FicompInt16Handler(_decoder));   // FICOMP int16 (DE /3)
        _handlers.Add(new FloatingPoint.Arithmetic.FisubInt16Handler(_decoder));    // FISUB int16 (DE /4)
        _handlers.Add(new FloatingPoint.Arithmetic.FisubrInt16Handler(_decoder));   // FISUBR int16 (DE /5)
        _handlers.Add(new FloatingPoint.Arithmetic.FidivInt16Handler(_decoder));    // FIDIV int16 (DE /6)
        _handlers.Add(new FloatingPoint.Arithmetic.FidivrInt16Handler(_decoder));   // FIDIVR int16 (DE /7)
        
        // DE opcode handlers (register operations)
        _handlers.Add(new FloatingPoint.Arithmetic.FaddpStiStHandler(_decoder));     // FADDP ST(i), ST (DE C0-C7)
        _handlers.Add(new FloatingPoint.Arithmetic.FmulpStiStHandler(_decoder));     // FMULP ST(i), ST (DE C8-CF)
        _handlers.Add(new FloatingPoint.Comparison.FcomppHandler(_decoder));        // FCOMPP (DE D9)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubpStiStHandler(_decoder));     // FSUBP ST(i), ST (DE E0-E7)
        _handlers.Add(new FloatingPoint.Arithmetic.FsubrpStiStHandler(_decoder));    // FSUBRP ST(i), ST (DE E8-EF)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivrpStiStHandler(_decoder));     // FDIVP ST(i), ST (DE F0-F7)
        _handlers.Add(new FloatingPoint.Arithmetic.FdivpStiStHandler(_decoder));    // FDIVRP ST(i), ST (DE F8-FF)
        
        // DF opcode handlers (memory operations)
        _handlers.Add(new FloatingPoint.LoadStore.FildInt16Handler(_decoder));     // FILD int16 (DF /0)
        _handlers.Add(new FloatingPoint.LoadStore.FisttpInt16Handler(_decoder));   // FISTTP int16 (DF /1)
        _handlers.Add(new FloatingPoint.LoadStore.FistInt16Handler(_decoder));     // FIST int16 (DF /2)
        _handlers.Add(new FloatingPoint.LoadStore.FistpInt16Handler(_decoder));    // FISTP int16 (DF /3)
        _handlers.Add(new FloatingPoint.LoadStore.FbldHandler(_decoder));          // FBLD packed BCD (DF /4)
        _handlers.Add(new FloatingPoint.LoadStore.FildInt64Handler(_decoder));     // FILD int64 (DF /5)
        _handlers.Add(new FloatingPoint.LoadStore.FbstpHandler(_decoder));         // FBSTP packed BCD (DF /6)
        _handlers.Add(new FloatingPoint.LoadStore.FistpInt64Handler(_decoder));    // FISTP int64 (DF /7)
        
        // DF opcode handlers (register operations)
        _handlers.Add(new FloatingPoint.Control.FfreepHandler(_decoder));          // FFREEP ST(i) (DF C0-C7)
        _handlers.Add(new FloatingPoint.LoadStore.FxchDfHandler(_decoder));        // FXCH (DF C8)
        _handlers.Add(new FloatingPoint.LoadStore.FstpDfHandler(_decoder));        // FSTP ST(1) (DF D0, DF D8)
        _handlers.Add(new FloatingPoint.Comparison.FucomipHandler(_decoder));      // FUCOMIP ST(0), ST(i) (DF E8-EF)
        _handlers.Add(new FloatingPoint.Comparison.FcomipHandler(_decoder));       // FCOMIP ST(0), ST(i) (DF F0-F7)
    }

    /// <summary>
    /// Registers all String instruction handlers
    /// </summary>
    private void RegisterStringHandlers()
    {
        // Add String instruction handler that handles both regular and REP/REPNE prefixed string instructions
        _handlers.Add(new StringInstructionHandler(_decoder));
    }

    /// <summary>
    /// Registers all MOV instruction handlers
    /// </summary>
    private void RegisterMovHandlers()
    {
        // Add MOV register/memory handlers
        _handlers.Add(new MovRegMemHandler(_decoder));       // MOV r32, r/m32 (opcode 8B)
        _handlers.Add(new MovMemRegHandler(_decoder));       // MOV r/m32, r32 (opcode 89)
        
        // Add MOV immediate handlers
        _handlers.Add(new MovRegImm32Handler(_decoder));     // MOV r32, imm32 (opcode B8+r)
        _handlers.Add(new MovRegImm8Handler(_decoder));      // MOV r8, imm8 (opcode B0+r)
        _handlers.Add(new MovRm32Imm32Handler(_decoder));    // MOV r/m32, imm32 (opcode C7 /0)
        _handlers.Add(new MovRm8Imm8Handler(_decoder));      // MOV r/m8, imm8 (opcode C6 /0)
        
        // Add MOV memory offset handlers
        _handlers.Add(new MovEaxMoffsHandler(_decoder));     // MOV EAX, moffs32 (opcode A1)
        _handlers.Add(new MovMoffsEaxHandler(_decoder));     // MOV moffs32, EAX (opcode A3)
    }

    /// <summary>
    /// Registers all PUSH instruction handlers
    /// </summary>
    private void RegisterPushHandlers()
    {
        // Add PUSH register handlers
        _handlers.Add(new PushRegHandler(_decoder));      // PUSH r32 (opcode 50+r)
        _handlers.Add(new PushRm32Handler(_decoder));     // PUSH r/m32 (opcode FF /6)
        
        // Add PUSH immediate handlers
        // Note: Order matters! PushImm16Handler must be registered before PushImm32Handler
        // since both check for opcode 68h but PushImm16Handler also checks for operand size prefix
        _handlers.Add(new PushImm16Handler(_decoder));    // PUSH imm16 with operand size prefix (0x66 0x68)
        _handlers.Add(new PushImm32Handler(_decoder));    // PUSH imm32 (opcode 68)
        _handlers.Add(new PushImm8Handler(_decoder));     // PUSH imm8 (opcode 6A)
    }

    /// <summary>
    /// Registers all POP instruction handlers
    /// </summary>
    private void RegisterPopHandlers()
    {
        // Add POP register handlers
        _handlers.Add(new PopRegHandler(_decoder));       // POP r32 (opcode 58+r)
        _handlers.Add(new PopRm32Handler(_decoder));      // POP r/m32 (opcode 8F /0)
    }

    /// <summary>
    /// Registers all AND instruction handlers
    /// </summary>
    private void RegisterAndHandlers()
    {
        // 16-bit handlers with operand size prefix (must come first)
        _handlers.Add(new AndAxImmHandler(_decoder));                // AND AX, imm16 (opcode 25 with 0x66 prefix)
        _handlers.Add(new AndImmToRm16Handler(_decoder));            // AND r/m16, imm16 (opcode 81 /4 with 0x66 prefix)
        _handlers.Add(new AndImmToRm16SignExtendedHandler(_decoder)); // AND r/m16, imm8 (opcode 83 /4 with 0x66 prefix)
        _handlers.Add(new AndRm16R16Handler(_decoder));              // AND r/m16, r16 (opcode 21 with 0x66 prefix)
        _handlers.Add(new AndR16Rm16Handler(_decoder));              // AND r16, r/m16 (opcode 23 with 0x66 prefix)
        
        // 8-bit handlers
        _handlers.Add(new AndAlImmHandler(_decoder));                // AND AL, imm8 (opcode 24)
        _handlers.Add(new AndR8Rm8Handler(_decoder));                // AND r8, r/m8 (opcode 22)
        _handlers.Add(new AndRm8R8Handler(_decoder));                // AND r/m8, r8 (opcode 20)
        _handlers.Add(new AndImmToRm8Handler(_decoder));              // AND r/m8, imm8 (opcode 80 /4)
        
        // 32-bit handlers
        _handlers.Add(new AndEaxImmHandler(_decoder));               // AND EAX, imm32 (opcode 25 without 0x66 prefix)
        _handlers.Add(new AndR32Rm32Handler(_decoder));              // AND r32, r/m32 (opcode 23)
        _handlers.Add(new AndMemRegHandler(_decoder));               // AND r/m32, r32 (opcode 21)
        _handlers.Add(new AndImmToRm32Handler(_decoder));            // AND r/m32, imm32 (opcode 81 /4)
        _handlers.Add(new AndImmToRm32SignExtendedHandler(_decoder)); // AND r/m32, imm8 (opcode 83 /4)
    }

    /// <summary>
    /// Registers all SUB instruction handlers
    /// </summary>
    private void RegisterSubHandlers()
    {
        // Register SUB handlers
        
        // 16-bit handlers with operand size prefix (must come first)
        _handlers.Add(new SubAxImm16Handler(_decoder));              // SUB AX, imm16 (opcode 0x66 0x83 /5)
        _handlers.Add(new SubImmFromRm16Handler(_decoder));          // SUB r/m16, imm16 (opcode 0x66 0x81 /5)
        _handlers.Add(new SubImmFromRm16SignExtendedHandler(_decoder)); // SUB r/m16, imm8 (opcode 0x66 0x83 /5)
        _handlers.Add(new SubRm16R16Handler(_decoder));              // SUB r/m16, r16 (opcode 0x66 0x29)
        _handlers.Add(new SubR16Rm16Handler(_decoder));              // SUB r16, r/m16 (opcode 0x66 0x2B)
        
        // 32-bit handlers
        _handlers.Add(new SubRm32R32Handler(_decoder));              // SUB r/m32, r32 (opcode 0x29)
        _handlers.Add(new SubR32Rm32Handler(_decoder));              // SUB r32, r/m32 (opcode 0x2B)
        _handlers.Add(new SubImmFromRm32Handler(_decoder));          // SUB r/m32, imm32 (opcode 0x81 /5)
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_decoder)); // SUB r/m32, imm8 (opcode 0x83 /5)

        // 8-bit handlers
        _handlers.Add(new SubRm8R8Handler(_decoder));                // SUB r/m8, r8 (opcode 0x28)
        _handlers.Add(new SubR8Rm8Handler(_decoder));                // SUB r8, r/m8 (opcode 0x2A)
        _handlers.Add(new SubAlImm8Handler(_decoder));              // SUB AL, imm8 (opcode 0x2C)
        _handlers.Add(new SubImmFromRm8Handler(_decoder));           // SUB r/m8, imm8 (opcode 0x80 /5)
    }

    /// <summary>
    /// Registers all NOP instruction handlers
    /// </summary>
    private void RegisterNopHandlers()
    {
        // Register NOP handlers
        _handlers.Add(new Nop.NopHandler(_decoder));                // NOP (opcode 0x90)
        _handlers.Add(new TwoByteNopHandler(_decoder));         // 2-byte NOP (opcode 0x66 0x90)
        _handlers.Add(new MultiByteNopHandler(_decoder));       // Multi-byte NOP (opcode 0F 1F /0)
    }

    /// <summary>
    /// Registers all miscellaneous instruction handlers
    /// </summary>
    private void RegisterMiscHandlers()
    {
        // Register miscellaneous instruction handlers
        _handlers.Add(new IntImm8Handler(_decoder));        // INT (opcode 0xCD)
        _handlers.Add(new IntoHandler(_decoder));       // INTO (opcode 0xCE)
        _handlers.Add(new IretHandler(_decoder));       // IRET (opcode 0xCF)
        _handlers.Add(new CpuidHandler(_decoder));      // CPUID (opcode 0x0F 0xA2)
        _handlers.Add(new RdtscHandler(_decoder));      // RDTSC (opcode 0x0F 0x31)
        _handlers.Add(new HltHandler(_decoder));        // HLT (opcode 0xF4)
        _handlers.Add(new WaitHandler(_decoder));       // WAIT (opcode 0x9B)
        _handlers.Add(new LockHandler(_decoder));       // LOCK (opcode 0xF0)
        _handlers.Add(new InHandler(_decoder));         // IN (opcodes 0xE4, 0xE5, 0xEC, 0xED)
        _handlers.Add(new OutHandler(_decoder));        // OUT (opcodes 0xE6, 0xE7, 0xEE, 0xEF)
    }

    /// <summary>
    /// Registers all Shift instruction handlers
    /// </summary>
    private void RegisterShiftHandlers()
    {
        // SHL (Shift Left) handlers
        _handlers.Add(new ShlRm8By1Handler(_decoder));       // SHL r/m8, 1 (0xD0 /4)
        _handlers.Add(new ShlRm8ByClHandler(_decoder));      // SHL r/m8, CL (0xD2 /4)
        _handlers.Add(new ShlRm8ByImmHandler(_decoder));     // SHL r/m8, imm8 (0xC0 /4)
        _handlers.Add(new ShlRm32By1Handler(_decoder));      // SHL r/m32, 1 (0xD1 /4)
        _handlers.Add(new ShlRm32ByClHandler(_decoder));     // SHL r/m32, CL (0xD3 /4)
        _handlers.Add(new ShlRm32ByImmHandler(_decoder));    // SHL r/m32, imm8 (0xC1 /4)

        // SHR (Shift Right) handlers
        _handlers.Add(new ShrRm8By1Handler(_decoder));       // SHR r/m8, 1 (0xD0 /5)
        _handlers.Add(new ShrRm8ByClHandler(_decoder));      // SHR r/m8, CL (0xD2 /5)
        _handlers.Add(new ShrRm8ByImmHandler(_decoder));     // SHR r/m8, imm8 (0xC0 /5)
        _handlers.Add(new ShrRm32By1Handler(_decoder));      // SHR r/m32, 1 (0xD1 /5)
        _handlers.Add(new ShrRm32ByClHandler(_decoder));     // SHR r/m32, CL (0xD3 /5)
        _handlers.Add(new ShrRm32ByImmHandler(_decoder));    // SHR r/m32, imm8 (0xC1 /5)

        // SAR (Shift Arithmetic Right) handlers
        _handlers.Add(new SarRm8By1Handler(_decoder));       // SAR r/m8, 1 (0xD0 /7)
        _handlers.Add(new SarRm8ByClHandler(_decoder));      // SAR r/m8, CL (0xD2 /7)
        _handlers.Add(new SarRm8ByImmHandler(_decoder));     // SAR r/m8, imm8 (0xC0 /7)
        _handlers.Add(new SarRm32By1Handler(_decoder));      // SAR r/m32, 1 (0xD1 /7)
        _handlers.Add(new SarRm32ByClHandler(_decoder));     // SAR r/m32, CL (0xD3 /7)
        _handlers.Add(new SarRm32ByImmHandler(_decoder));    // SAR r/m32, imm8 (0xC1 /7)

        // ROL (Rotate Left) handlers
        _handlers.Add(new RolRm8By1Handler(_decoder));       // ROL r/m8, 1 (0xD0 /0)
        _handlers.Add(new RolRm8ByClHandler(_decoder));      // ROL r/m8, CL (0xD2 /0)
        _handlers.Add(new RolRm8ByImmHandler(_decoder));     // ROL r/m8, imm8 (0xC0 /0)
        _handlers.Add(new RolRm32By1Handler(_decoder));      // ROL r/m32, 1 (0xD1 /0)
        _handlers.Add(new RolRm32ByClHandler(_decoder));     // ROL r/m32, CL (0xD3 /0)
        _handlers.Add(new RolRm32ByImmHandler(_decoder));    // ROL r/m32, imm8 (0xC1 /0)

        // ROR (Rotate Right) handlers
        _handlers.Add(new RorRm8By1Handler(_decoder));       // ROR r/m8, 1 (0xD0 /1)
        _handlers.Add(new RorRm8ByClHandler(_decoder));      // ROR r/m8, CL (0xD2 /1)
        _handlers.Add(new RorRm8ByImmHandler(_decoder));     // ROR r/m8, imm8 (0xC0 /1)
        _handlers.Add(new RorRm32By1Handler(_decoder));      // ROR r/m32, 1 (0xD1 /1)
        _handlers.Add(new RorRm32ByClHandler(_decoder));     // ROR r/m32, CL (0xD3 /1)
        _handlers.Add(new RorRm32ByImmHandler(_decoder));    // ROR r/m32, imm8 (0xC1 /1)

        // RCL (Rotate Carry Left) handlers
        _handlers.Add(new RclRm8By1Handler(_decoder));       // RCL r/m8, 1 (0xD0 /2)
        _handlers.Add(new RclRm8ByClHandler(_decoder));      // RCL r/m8, CL (0xD2 /2)
        _handlers.Add(new RclRm8ByImmHandler(_decoder));     // RCL r/m8, imm8 (0xC0 /2)
        _handlers.Add(new RclRm32By1Handler(_decoder));      // RCL r/m32, 1 (0xD1 /2)
        _handlers.Add(new RclRm32ByClHandler(_decoder));     // RCL r/m32, CL (0xD3 /2)
        _handlers.Add(new RclRm32ByImmHandler(_decoder));    // RCL r/m32, imm8 (0xC1 /2)

        // RCR (Rotate Carry Right) handlers
        _handlers.Add(new RcrRm8By1Handler(_decoder));       // RCR r/m8, 1 (0xD0 /3)
        _handlers.Add(new RcrRm8ByClHandler(_decoder));      // RCR r/m8, CL (0xD2 /3)
        _handlers.Add(new RcrRm8ByImmHandler(_decoder));     // RCR r/m8, imm8 (0xC0 /3)
        _handlers.Add(new RcrRm32By1Handler(_decoder));      // RCR r/m32, 1 (0xD1 /3)
        _handlers.Add(new RcrRm32ByClHandler(_decoder));     // RCR r/m32, CL (0xD3 /3)
        _handlers.Add(new RcrRm32ByImmHandler(_decoder));    // RCR r/m32, imm8 (0xC1 /3)
    }
    
    /// <summary>
    /// Registers all Flag manipulation instruction handlers
    /// </summary>
    private void RegisterFlagHandlers()
    {
        // Register flag manipulation handlers
        _handlers.Add(new StcHandler(_decoder));            // STC (Set Carry Flag) - opcode F9
        _handlers.Add(new ClcHandler(_decoder));            // CLC (Clear Carry Flag) - opcode F8
        _handlers.Add(new CmcHandler(_decoder));            // CMC (Complement Carry Flag) - opcode F5
        _handlers.Add(new StdHandler(_decoder));            // STD (Set Direction Flag) - opcode FD
        _handlers.Add(new CldHandler(_decoder));            // CLD (Clear Direction Flag) - opcode FC
        _handlers.Add(new StiHandler(_decoder));            // STI (Set Interrupt Flag) - opcode FB
        _handlers.Add(new CliHandler(_decoder));            // CLI (Clear Interrupt Flag) - opcode FA
        _handlers.Add(new SahfHandler(_decoder));           // SAHF (Store AH into Flags) - opcode 9E
        _handlers.Add(new LahfHandler(_decoder));           // LAHF (Load Flags into AH) - opcode 9F
    }

    /// <summary>
    /// Registers all bit manipulation instruction handlers
    /// </summary>
    private void RegisterBitHandlers()
    {
        // BT (Bit Test) handlers
        _handlers.Add(new BtR32Rm32Handler(_decoder));    // BT r32, r/m32 (0F A3)
        _handlers.Add(new BtRm32ImmHandler(_decoder));    // BT r/m32, imm8 (0F BA /4)
        
        // BTS (Bit Test and Set) handlers
        _handlers.Add(new BtsR32Rm32Handler(_decoder));   // BTS r32, r/m32 (0F AB)
        _handlers.Add(new BtsRm32ImmHandler(_decoder));   // BTS r/m32, imm8 (0F BA /5)
        
        // BTR (Bit Test and Reset) handlers
        _handlers.Add(new BtrR32Rm32Handler(_decoder));   // BTR r32, r/m32 (0F B3)
        _handlers.Add(new BtrRm32ImmHandler(_decoder));   // BTR r/m32, imm8 (0F BA /6)
        
        // BTC (Bit Test and Complement) handlers
        _handlers.Add(new BtcR32Rm32Handler(_decoder));   // BTC r32, r/m32 (0F BB)
        _handlers.Add(new BtcRm32ImmHandler(_decoder));   // BTC r/m32, imm8 (0F BA /7)
        
        // BSF and BSR (Bit Scan) handlers
        _handlers.Add(new BsfR32Rm32Handler(_decoder));   // BSF r32, r/m32 (0F BC)
        _handlers.Add(new BsrR32Rm32Handler(_decoder));   // BSR r32, r/m32 (0F BD)
    }



    /// <summary>
    /// Registers all NEG instruction handlers
    /// </summary>
    private void RegisterNegHandlers()
    {
        _handlers.Add(new NegRm8Handler(_decoder)); // NEG r/m8 handler (F6 /3)
        _handlers.Add(new NegRm32Handler(_decoder)); // NEG r/m32 handler (F7 /3)
    }

    /// <summary>
    /// Registers all MUL instruction handlers
    /// </summary>
    private void RegisterMulHandlers()
    {
        _handlers.Add(new MulRm8Handler(_decoder)); // MUL r/m8 handler (F6 /4)
        _handlers.Add(new MulRm32Handler(_decoder)); // MUL r/m32 handler (F7 /4)
    }

    /// <summary>
    /// Registers all NOT instruction handlers
    /// </summary>
    private void RegisterNotHandlers()
    {
        _handlers.Add(new NotRm8Handler(_decoder)); // NOT r/m8 handler (F6 /2)
        _handlers.Add(new NotRm32Handler(_decoder)); // NOT r/m32 handler (F7 /2)
    }

    /// <summary>
    /// Registers all IMUL instruction handlers
    /// </summary>
    private void RegisterImulHandlers()
    {
        _handlers.Add(new ImulRm8Handler(_decoder)); // IMUL r/m8 handler (F6 /5)
        _handlers.Add(new ImulRm32Handler(_decoder)); // IMUL r/m32 handler (F7 /5)

        _handlers.Add(new ImulR32Rm32Handler(_decoder)); // IMUL r32, r/m32 handler (0F AF /r)
        _handlers.Add(new ImulR32Rm32Imm8Handler(_decoder)); // IMUL r32, r/m32, imm8 handler (6B /r ib)
        _handlers.Add(new ImulR32Rm32Imm32Handler(_decoder)); // IMUL r32, r/m32, imm32 handler (69 /r id)
    }

    /// <summary>
    /// Registers all DIV instruction handlers
    /// </summary>
    private void RegisterDivHandlers()
    {
        _handlers.Add(new DivRm8Handler(_decoder)); // DIV r/m8 handler (F6 /6)
        _handlers.Add(new DivRm32Handler(_decoder)); // DIV r/m32 handler (F7 /6)
    }

    /// <summary>
    /// Registers all IDIV instruction handlers
    /// </summary>
    private void RegisterIdivHandlers()
    {
        _handlers.Add(new IdivRm8Handler(_decoder)); // IDIV r/m8 handler (F6 /7)
        _handlers.Add(new IdivRm32Handler(_decoder)); // IDIV r/m32 handler (F7 /7)
    }

    /// <summary>
    /// Gets the handler that can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to decode</param>
    /// <returns>The handler that can decode the opcode, or null if no handler can decode it</returns>
    public IInstructionHandler? GetHandler(byte opcode)
    {
        // For all other opcodes, use the normal handler selection logic
        return _handlers.FirstOrDefault(h => h.CanHandle(opcode));
    }
}