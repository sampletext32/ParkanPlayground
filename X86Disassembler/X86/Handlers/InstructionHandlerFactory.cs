using X86Disassembler.X86.Handlers.Adc;
using X86Disassembler.X86.Handlers.Add;
using X86Disassembler.X86.Handlers.And;
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
    private readonly byte[] _codeBuffer;
    private readonly InstructionDecoder _decoder;
    private readonly int _length;

    /// <summary>
    /// Initializes a new instance of the InstructionHandlerFactory class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this factory</param>
    /// <param name="length">The length of the buffer</param>
    public InstructionHandlerFactory(byte[] codeBuffer, InstructionDecoder decoder, int length)
    {
        _codeBuffer = codeBuffer;
        _decoder = decoder;
        _length = length;

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
        RegisterArithmeticImmediateHandlers(); // Group 1 instructions (including 0x83)
        RegisterAddHandlers();        // ADD instructions
        RegisterAndHandlers();        // AND instructions
        RegisterOrHandlers();         // OR instructions
        RegisterXorHandlers();        // XOR instructions
        RegisterCmpHandlers();        // CMP instructions
        RegisterTestHandlers();       // TEST instructions
        
        // Register arithmetic unary instructions
        RegisterArithmeticUnaryHandlers(); // Empty, kept for consistency
        RegisterNotHandlers();       // NOT instructions
        RegisterNegHandlers();       // NEG instructions
        RegisterMulHandlers();       // MUL instructions
        RegisterImulHandlers();      // IMUL instructions
        RegisterDivHandlers();       // DIV instructions
        RegisterIdivHandlers();      // IDIV instructions
        RegisterDataTransferHandlers();
        RegisterJumpHandlers();
        RegisterCallHandlers();
        RegisterReturnHandlers();
        RegisterDecHandlers();
        RegisterIncHandlers(); // INC/DEC handlers after Group 1 handlers
        RegisterPushHandlers();
        RegisterPopHandlers();
        RegisterLeaHandlers();
        RegisterFloatingPointHandlers();
        RegisterStringHandlers();
        RegisterMovHandlers();
        RegisterSubHandlers(); // Register SUB handlers
        RegisterNopHandlers(); // Register NOP handlers
    }

    /// <summary>
    /// Registers all ArithmeticUnary instruction handlers
    /// </summary>
    private void RegisterArithmeticUnaryHandlers()
    {
        // This method is kept for consistency, but all handlers have been moved to their own namespaces
    }

    /// <summary>
    /// Registers all ArithmeticImmediate instruction handlers
    /// </summary>
    private void RegisterArithmeticImmediateHandlers()
    {
        // ADC handlers
        _handlers.Add(new AdcImmToRm32Handler(_decoder));         // ADC r/m32, imm32 (opcode 81 /2)
        _handlers.Add(new AdcImmToRm32SignExtendedHandler(_decoder)); // ADC r/m32, imm8 (opcode 83 /2)

        // SBB handlers
        _handlers.Add(new SbbImmFromRm32Handler(_decoder));         // SBB r/m32, imm32 (opcode 81 /3)
        _handlers.Add(new SbbImmFromRm32SignExtendedHandler(_decoder)); // SBB r/m32, imm8 (opcode 83 /3)

        // SUB handlers
        _handlers.Add(new SubImmFromRm32Handler(_decoder));         // SUB r/m32, imm32 (opcode 81 /5)
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_decoder)); // SUB r/m32, imm8 (opcode 83 /5)
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
        _handlers.Add(new CallRel32Handler(_decoder));
        _handlers.Add(new CallRm32Handler(_decoder));
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
        _handlers.Add(new JgeRel8Handler(_decoder));
        _handlers.Add(new ConditionalJumpHandler(_decoder));
        _handlers.Add(new TwoByteConditionalJumpHandler(_decoder));
    }

    /// <summary>
    /// Registers all Test instruction handlers
    /// </summary>
    private void RegisterTestHandlers()
    {
        // TEST handlers
        _handlers.Add(new TestImmWithRm32Handler(_decoder));
        _handlers.Add(new TestImmWithRm8Handler(_decoder));
        _handlers.Add(new TestRegMem8Handler(_decoder));
        _handlers.Add(new TestRegMemHandler(_decoder));
        _handlers.Add(new TestAlImmHandler(_decoder));
        _handlers.Add(new TestEaxImmHandler(_decoder));
    }

    /// <summary>
    /// Registers all Xor instruction handlers
    /// </summary>
    private void RegisterXorHandlers()
    {
        // 16-bit handlers
        _handlers.Add(new XorRm16R16Handler(_decoder));
        _handlers.Add(new XorR16Rm16Handler(_decoder));
        _handlers.Add(new XorImmWithRm16Handler(_decoder));
        _handlers.Add(new XorImmWithRm16SignExtendedHandler(_decoder));
        
        // 32-bit handlers
        _handlers.Add(new XorMemRegHandler(_decoder));
        _handlers.Add(new XorRegMemHandler(_decoder));
        _handlers.Add(new XorImmWithRm32Handler(_decoder));
        _handlers.Add(new XorImmWithRm32SignExtendedHandler(_decoder));

        // 8-bit handlers
        _handlers.Add(new XorRm8R8Handler(_decoder));
        _handlers.Add(new XorR8Rm8Handler(_decoder));
        _handlers.Add(new XorAlImmHandler(_decoder));
        _handlers.Add(new XorImmWithRm8Handler(_decoder));

        // special treatment with xor-ing eax
        // precise handlers go first
        _handlers.Add(new XorAxImm16Handler(_decoder));
        _handlers.Add(new XorEaxImmHandler(_decoder));
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
        _handlers.Add(new LeaR32MHandler(_decoder));
    }

    /// <summary>
    /// Registers all Cmp instruction handlers
    /// </summary>
    private void RegisterCmpHandlers()
    {
        // Add Cmp handlers for 32-bit operands
        _handlers.Add(new CmpR32Rm32Handler(_decoder));
        _handlers.Add(new CmpRm32R32Handler(_decoder));
        
        // Add Cmp handlers for 8-bit operands
        _handlers.Add(new CmpRm8R8Handler(_decoder));  // CMP r/m8, r8 (opcode 38)
        _handlers.Add(new CmpR8Rm8Handler(_decoder));  // CMP r8, r/m8 (opcode 3A)
        
        // Add Cmp handlers for immediate operands
        _handlers.Add(new CmpImmWithRm8Handler(_decoder));
        _handlers.Add(new CmpAlImmHandler(_decoder));  // CMP AL, imm8 (opcode 3C)
        _handlers.Add(new CmpEaxImmHandler(_decoder)); // CMP EAX, imm32 (opcode 3D)

        // Add CMP immediate handlers from ArithmeticImmediate namespace
        _handlers.Add(new CmpImmWithRm32Handler(_decoder));
        _handlers.Add(new CmpImmWithRm32SignExtendedHandler(_decoder));
    }

    /// <summary>
    /// Registers all Dec instruction handlers
    /// </summary>
    private void RegisterDecHandlers()
    {
        // Add Dec handlers
        _handlers.Add(new DecRegHandler(_decoder));
    }

    /// <summary>
    /// Registers all Inc instruction handlers
    /// </summary>
    private void RegisterIncHandlers()
    {
        // Add Inc handlers
        _handlers.Add(new IncRegHandler(_decoder));
    }

    /// <summary>
    /// Registers all Add instruction handlers
    /// </summary>
    private void RegisterAddHandlers()
    {
        // Add ADD handlers
        _handlers.Add(new AddR32Rm32Handler(_decoder));
        _handlers.Add(new AddRm32R32Handler(_decoder));
        _handlers.Add(new AddEaxImmHandler(_decoder));
        
        // Add 8-bit ADD handlers
        _handlers.Add(new AddRm8R8Handler(_decoder)); // ADD r/m8, r8 (opcode 00)
        _handlers.Add(new AddR8Rm8Handler(_decoder)); // ADD r8, r/m8 (opcode 02)
        _handlers.Add(new AddAlImmHandler(_decoder)); // ADD AL, imm8 (opcode 04)

        // Add ADD immediate handlers from ArithmeticImmediate namespace
        _handlers.Add(new AddImmToRm8Handler(_decoder));
        _handlers.Add(new AddImmToRm32Handler(_decoder));
        _handlers.Add(new AddImmToRm32SignExtendedHandler(_decoder));
    }

    /// <summary>
    /// Registers all Data Transfer instruction handlers
    /// </summary>
    private void RegisterDataTransferHandlers()
    {
        // Add MOV handlers
        _handlers.Add(new MovRegMemHandler(_decoder));
        _handlers.Add(new MovMemRegHandler(_decoder));
        _handlers.Add(new MovRegImm32Handler(_decoder));
        _handlers.Add(new MovRegImm8Handler(_decoder));
        _handlers.Add(new MovEaxMoffsHandler(_decoder));
        _handlers.Add(new MovMoffsEaxHandler(_decoder));
        _handlers.Add(new MovRm32Imm32Handler(_decoder));
        _handlers.Add(new MovRm8Imm8Handler(_decoder));

        // Add PUSH handlers
        _handlers.Add(new PushRegHandler(_decoder));
        _handlers.Add(new PushImm32Handler(_decoder));
        _handlers.Add(new PushImm8Handler(_decoder));

        // Add POP handlers
        _handlers.Add(new PopRegHandler(_decoder));

        // Add XCHG handlers
        _handlers.Add(new XchgEaxRegHandler(_decoder));
    }

    /// <summary>
    /// Registers all Floating Point instruction handlers
    /// </summary>
    private void RegisterFloatingPointHandlers()
    {
        // Add Floating Point handlers
        _handlers.Add(new FnstswHandler(_decoder));
        _handlers.Add(new Float32OperationHandler(_decoder));
        _handlers.Add(new LoadStoreControlHandler(_decoder));
        _handlers.Add(new Int32OperationHandler(_decoder));
        _handlers.Add(new LoadStoreInt32Handler(_decoder));
        _handlers.Add(new Float64OperationHandler(_decoder));
        _handlers.Add(new LoadStoreFloat64Handler(_decoder));
        _handlers.Add(new Int16OperationHandler(_decoder));
        _handlers.Add(new LoadStoreInt16Handler(_decoder));
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
    }

    /// <summary>
    /// Registers all And instruction handlers
    /// </summary>
    private void RegisterAndHandlers()
    {
        // Add AND immediate handlers
        _handlers.Add(new AndImmToRm8Handler(_decoder));              // AND r/m8, imm8 (opcode 80 /4)
        _handlers.Add(new AndImmWithRm32Handler(_decoder));           // AND r/m32, imm32 (opcode 81 /4)
        _handlers.Add(new AndImmToRm32Handler(_decoder));            // AND r/m32, imm32 (opcode 81 /4)
        _handlers.Add(new AndImmToRm32SignExtendedHandler(_decoder)); // AND r/m32, imm8 (opcode 83 /4)

        // Add AND register handlers
        _handlers.Add(new AndR8Rm8Handler(_decoder));                // AND r8, r/m8 (opcode 22)
        _handlers.Add(new AndRm8R8Handler(_decoder));                // AND r/m8, r8 (opcode 20)
        _handlers.Add(new AndR32Rm32Handler(_decoder));              // AND r32, r/m32 (opcode 23)
        _handlers.Add(new AndMemRegHandler(_decoder));               // AND r/m32, r32 (opcode 21)
        
        // Add AND immediate with accumulator handlers
        _handlers.Add(new AndAlImmHandler(_decoder));                // AND AL, imm8 (opcode 24)
        _handlers.Add(new AndEaxImmHandler(_decoder));               // AND EAX, imm32 (opcode 25)
    }

    /// <summary>
    /// Registers all SUB instruction handlers
    /// </summary>
    private void RegisterSubHandlers()
    {
        // Register SUB handlers

        // 32-bit handlers
        _handlers.Add(new SubRm32R32Handler(_decoder));
        _handlers.Add(new SubR32Rm32Handler(_decoder));
        _handlers.Add(new SubImmFromRm32Handler(_decoder));
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_decoder));

        // 16-bit handlers
        _handlers.Add(new SubRm16R16Handler(_decoder));
        _handlers.Add(new SubR16Rm16Handler(_decoder));
        _handlers.Add(new SubAxImm16Handler(_decoder));
        _handlers.Add(new SubImmFromRm16Handler(_decoder));
        _handlers.Add(new SubImmFromRm16SignExtendedHandler(_decoder));

        // 8-bit handlers
        _handlers.Add(new SubRm8R8Handler(_decoder));
        _handlers.Add(new SubR8Rm8Handler(_decoder));
        _handlers.Add(new SubAlImm8Handler(_decoder));
        _handlers.Add(new SubImmFromRm8Handler(_decoder));
    }

    /// <summary>
    /// Registers all NOP instruction handlers
    /// </summary>
    private void RegisterNopHandlers()
    {
        // Register NOP handlers
        _handlers.Add(new NopHandler(_decoder));
        _handlers.Add(new TwoByteNopHandler(_decoder));
        _handlers.Add(new MultiByteNopHandler(_decoder));
    }

    /// <summary>
    /// Registers all NEG instruction handlers
    /// </summary>
    private void RegisterNegHandlers()
    {
        // NEG r/m8 handler (F6 /3)
        _handlers.Add(new NegRm8Handler(_decoder));

        // NEG r/m32 handler (F7 /3)
        _handlers.Add(new NegRm32Handler(_decoder));
    }

    /// <summary>
    /// Registers all MUL instruction handlers
    /// </summary>
    private void RegisterMulHandlers()
    {
        // MUL r/m8 handler (F6 /4)
        _handlers.Add(new MulRm8Handler(_decoder));

        // MUL r/m32 handler (F7 /4)
        _handlers.Add(new MulRm32Handler(_decoder));
    }

    /// <summary>
    /// Registers all NOT instruction handlers
    /// </summary>
    private void RegisterNotHandlers()
    {
        // NOT r/m8 handler (F6 /2)
        _handlers.Add(new NotRm8Handler(_decoder));

        // NOT r/m32 handler (F7 /2)
        _handlers.Add(new NotRm32Handler(_decoder));
    }

    /// <summary>
    /// Registers all IMUL instruction handlers
    /// </summary>
    private void RegisterImulHandlers()
    {
        // IMUL r/m8 handler (F6 /5)
        _handlers.Add(new ImulRm8Handler(_decoder));

        // IMUL r/m32 handler (F7 /5)
        _handlers.Add(new ImulRm32Handler(_decoder));

        // IMUL r32, r/m32 handler (0F AF /r)
        _handlers.Add(new ImulR32Rm32Handler(_decoder));

        // IMUL r32, r/m32, imm8 handler (6B /r ib)
        _handlers.Add(new ImulR32Rm32Imm8Handler(_decoder));

        // IMUL r32, r/m32, imm32 handler (69 /r id)
        _handlers.Add(new ImulR32Rm32Imm32Handler(_decoder));
    }

    /// <summary>
    /// Registers all DIV instruction handlers
    /// </summary>
    private void RegisterDivHandlers()
    {
        // DIV r/m8 handler (F6 /6)
        _handlers.Add(new DivRm8Handler(_decoder));

        // DIV r/m32 handler (F7 /6)
        _handlers.Add(new DivRm32Handler(_decoder));
    }

    /// <summary>
    /// Registers all IDIV instruction handlers
    /// </summary>
    private void RegisterIdivHandlers()
    {
        // IDIV r/m8 handler (F6 /7)
        _handlers.Add(new IdivRm8Handler(_decoder));

        // IDIV r/m32 handler (F7 /7)
        _handlers.Add(new IdivRm32Handler(_decoder));
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