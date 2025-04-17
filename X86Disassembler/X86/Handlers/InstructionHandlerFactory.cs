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
        RegisterDataTransferHandlers(); // MOV, MOVZX, MOVSX
        RegisterJumpHandlers();      // JMP instructions
        RegisterCallHandlers();      // CALL instructions
        RegisterReturnHandlers();    // RET instructions
        RegisterDecHandlers();       // DEC instructions
        RegisterIncHandlers(); // INC/DEC handlers after Group 1 handlers
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
    }

    /// <summary>
    /// Registers all SBB instruction handlers
    /// </summary>
    private void RegisterSbbHandlers()
    {
        // SBB immediate handlers
        _handlers.Add(new SbbImmFromRm32Handler(_decoder));         // SBB r/m32, imm32 (opcode 81 /3)
        _handlers.Add(new SbbImmFromRm32SignExtendedHandler(_decoder)); // SBB r/m32, imm8 (opcode 83 /3)
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
    /// Registers all Data Transfer instruction handlers
    /// </summary>
    private void RegisterDataTransferHandlers()
    {
        // Add MOV handlers
        _handlers.Add(new MovRegMemHandler(_decoder)); // MOV r32, r/m32 (opcode 8B)
        _handlers.Add(new MovMemRegHandler(_decoder)); // MOV r/m32, r32 (opcode 89)
        _handlers.Add(new MovRegImm32Handler(_decoder)); // MOV r32, imm32 (opcode B8 + register)
        _handlers.Add(new MovRegImm8Handler(_decoder)); // MOV r32, imm8 (opcode B0 + register)
        _handlers.Add(new MovEaxMoffsHandler(_decoder)); // MOV EAX, moffs32 (opcode A1)
        _handlers.Add(new MovMoffsEaxHandler(_decoder)); // MOV moffs32, EAX (opcode A3)
        _handlers.Add(new MovRm32Imm32Handler(_decoder)); // MOV r/m32, imm32 (opcode C7 /0)
        _handlers.Add(new MovRm8Imm8Handler(_decoder)); // MOV r/m8, imm8 (opcode C6 /0)

        // Add XCHG handlers
        _handlers.Add(new XchgEaxRegHandler(_decoder)); // XCHG EAX, r32 (opcode 90 + register)
    }

    /// <summary>
    /// Registers all Floating Point instruction handlers
    /// </summary>
    private void RegisterFloatingPointHandlers()
    {
        // Add Floating Point handlers
        _handlers.Add(new FnstswHandler(_decoder)); // FSTSW (opcode DF /7)
        _handlers.Add(new Float32OperationHandler(_decoder)); // Floating Point operations on 32-bit values
        _handlers.Add(new LoadStoreControlHandler(_decoder)); // Load and store control words (opcode D9 /0)
        _handlers.Add(new Int32OperationHandler(_decoder)); // Integer operations on 32-bit values
        _handlers.Add(new LoadStoreInt32Handler(_decoder)); // Load and store 32-bit values
        _handlers.Add(new Float64OperationHandler(_decoder)); // Floating Point operations on 64-bit values
        _handlers.Add(new LoadStoreFloat64Handler(_decoder)); // Load and store 64-bit values
        _handlers.Add(new Int16OperationHandler(_decoder)); // Integer operations on 16-bit values
        _handlers.Add(new LoadStoreInt16Handler(_decoder)); // Load and store 16-bit values
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
    /// Registers all shift and rotate instruction handlers
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