using X86Disassembler.X86.Handlers.Adc;
using X86Disassembler.X86.Handlers.Add;
using X86Disassembler.X86.Handlers.And;
using X86Disassembler.X86.Handlers.ArithmeticUnary;
using X86Disassembler.X86.Handlers.Call;
using X86Disassembler.X86.Handlers.Cmp;
using X86Disassembler.X86.Handlers.Dec;
using X86Disassembler.X86.Handlers.FloatingPoint;
using X86Disassembler.X86.Handlers.Inc;
using X86Disassembler.X86.Handlers.Jump;
using X86Disassembler.X86.Handlers.Lea;
using X86Disassembler.X86.Handlers.Mov;
using X86Disassembler.X86.Handlers.Nop;
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
        _handlers.Add(new Int3Handler(_codeBuffer, _decoder, _length));

        // Register handlers in order of priority (most specific first)
        RegisterArithmeticImmediateHandlers(); // Group 1 instructions (including 0x83)
        RegisterAddHandlers();
        RegisterAndHandlers();
        RegisterArithmeticUnaryHandlers();
        RegisterCmpHandlers();
        RegisterXorHandlers();
        RegisterOrHandlers();
        RegisterTestHandlers();
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
        // NOT handler
        _handlers.Add(new NotRm32Handler(_codeBuffer, _decoder, _length));

        // NEG handler
        _handlers.Add(new NegRm32Handler(_codeBuffer, _decoder, _length));

        // MUL handler
        _handlers.Add(new MulRm32Handler(_codeBuffer, _decoder, _length));

        // IMUL handler
        _handlers.Add(new ImulRm32Handler(_codeBuffer, _decoder, _length));

        // DIV handler
        _handlers.Add(new DivRm32Handler(_codeBuffer, _decoder, _length));

        // IDIV handler
        _handlers.Add(new IdivRm32Handler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all ArithmeticImmediate instruction handlers
    /// </summary>
    private void RegisterArithmeticImmediateHandlers()
    {
        // ADC handlers
        _handlers.Add(new AdcImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AdcImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        // SBB handlers
        _handlers.Add(new SbbImmFromRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SbbImmFromRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        // SUB handlers
        _handlers.Add(new SubImmFromRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Return instruction handlers
    /// </summary>
    private void RegisterReturnHandlers()
    {
        // Add Return handlers
        _handlers.Add(new RetHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new RetImmHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Call instruction handlers
    /// </summary>
    private void RegisterCallHandlers()
    {
        // Add Call handlers
        _handlers.Add(new CallRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CallRm32Handler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Jump instruction handlers
    /// </summary>
    private void RegisterJumpHandlers()
    {
        // JMP handlers
        _handlers.Add(new JmpRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JmpRel8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JgeRel8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new ConditionalJumpHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TwoByteConditionalJumpHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Test instruction handlers
    /// </summary>
    private void RegisterTestHandlers()
    {
        // TEST handlers
        _handlers.Add(new TestImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestImmWithRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestRegMem8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestEaxImmHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Xor instruction handlers
    /// </summary>
    private void RegisterXorHandlers()
    {
        // 16-bit handlers
        _handlers.Add(new XorRm16R16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorR16Rm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm16SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // 32-bit handlers
        _handlers.Add(new XorMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        // 8-bit handlers
        _handlers.Add(new XorRm8R8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorR8Rm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm8Handler(_codeBuffer, _decoder, _length));

        // special treatment with xor-ing eax
        // precise handlers go first
        _handlers.Add(new XorAxImm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorEaxImmHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Or instruction handlers
    /// </summary>
    private void RegisterOrHandlers()
    {
        // Add OR handlers
        _handlers.Add(new OrImmToRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        _handlers.Add(new OrR8Rm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrRm8R8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrR32Rm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrEaxImmHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Lea instruction handlers
    /// </summary>
    private void RegisterLeaHandlers()
    {
        // Add Lea handlers
        _handlers.Add(new LeaR32MHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Cmp instruction handlers
    /// </summary>
    private void RegisterCmpHandlers()
    {
        // Add Cmp handlers
        _handlers.Add(new CmpR32Rm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CmpRm32R32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CmpImmWithRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CmpAlImmHandler(_codeBuffer, _decoder, _length));

        // Add CMP immediate handlers from ArithmeticImmediate namespace
        _handlers.Add(new CmpImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CmpImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Dec instruction handlers
    /// </summary>
    private void RegisterDecHandlers()
    {
        // Add Dec handlers
        _handlers.Add(new DecRegHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Inc instruction handlers
    /// </summary>
    private void RegisterIncHandlers()
    {
        // Add Inc handlers
        _handlers.Add(new IncRegHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Add instruction handlers
    /// </summary>
    private void RegisterAddHandlers()
    {
        // Add ADD handlers
        _handlers.Add(new AddR32Rm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddRm32R32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddEaxImmHandler(_codeBuffer, _decoder, _length));

        // Add ADD immediate handlers from ArithmeticImmediate namespace
        _handlers.Add(new AddImmToRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Data Transfer instruction handlers
    /// </summary>
    private void RegisterDataTransferHandlers()
    {
        // Add MOV handlers
        _handlers.Add(new MovRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRegImm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRegImm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovEaxMoffsHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovMoffsEaxHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRm32Imm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRm8Imm8Handler(_codeBuffer, _decoder, _length));

        // Add PUSH handlers
        _handlers.Add(new PushRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new PushImm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new PushImm8Handler(_codeBuffer, _decoder, _length));

        // Add POP handlers
        _handlers.Add(new PopRegHandler(_codeBuffer, _decoder, _length));

        // Add XCHG handlers
        _handlers.Add(new XchgEaxRegHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all Floating Point instruction handlers
    /// </summary>
    private void RegisterFloatingPointHandlers()
    {
        // Add Floating Point handlers
        _handlers.Add(new FnstswHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new Float32OperationHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new LoadStoreControlHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new Int32OperationHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new LoadStoreInt32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new Float64OperationHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new LoadStoreFloat64Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new Int16OperationHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new LoadStoreInt16Handler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all String instruction handlers
    /// </summary>
    private void RegisterStringHandlers()
    {
        // Add String instruction handler that handles both regular and REP/REPNE prefixed string instructions
        _handlers.Add(new StringInstructionHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all MOV instruction handlers
    /// </summary>
    private void RegisterMovHandlers()
    {
        // Add MOV handlers
        _handlers.Add(new MovRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRegImm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRegImm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovEaxMoffsHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovMoffsEaxHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRm32Imm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MovRm8Imm8Handler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all PUSH instruction handlers
    /// </summary>
    private void RegisterPushHandlers()
    {
        // Add PUSH handlers
        _handlers.Add(new PushRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new PushImm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new PushImm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new PushRm32Handler(_codeBuffer, _decoder, _length)); // Add handler for PUSH r/m32 (FF /6)
    }

    /// <summary>
    /// Registers all POP instruction handlers
    /// </summary>
    private void RegisterPopHandlers()
    {
        // Add POP handlers
        _handlers.Add(new PopRegHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all And instruction handlers
    /// </summary>
    private void RegisterAndHandlers()
    {
        // Add AND handlers
        _handlers.Add(new AndImmToRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        _handlers.Add(new AndR8Rm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndRm8R8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndR32Rm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndEaxImmHandler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all SUB instruction handlers
    /// </summary>
    private void RegisterSubHandlers()
    {
        // Register SUB handlers

        // 32-bit handlers
        _handlers.Add(new SubRm32R32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubR32Rm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_codeBuffer, _decoder, _length));

        // 16-bit handlers
        _handlers.Add(new SubRm16R16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubR16Rm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubAxImm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm16Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm16SignExtendedHandler(_codeBuffer, _decoder, _length));

        // 8-bit handlers
        _handlers.Add(new SubRm8R8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubR8Rm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubAlImm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm8Handler(_codeBuffer, _decoder, _length));
    }

    /// <summary>
    /// Registers all NOP instruction handlers
    /// </summary>
    private void RegisterNopHandlers()
    {
        // Register NOP handlers
        _handlers.Add(new NopHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TwoByteNopHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new MultiByteNopHandler(_codeBuffer, _decoder, _length));
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