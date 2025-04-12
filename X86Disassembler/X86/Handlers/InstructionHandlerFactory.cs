using X86Disassembler.X86.Handlers.Call;
using X86Disassembler.X86.Handlers.FloatingPoint;
using X86Disassembler.X86.Handlers.Group1;
using X86Disassembler.X86.Handlers.Group3;
using X86Disassembler.X86.Handlers.Jump;
using X86Disassembler.X86.Handlers.Mov;
using X86Disassembler.X86.Handlers.Pop;
using X86Disassembler.X86.Handlers.Push;
using X86Disassembler.X86.Handlers.Ret;
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
        
        RegisterHandlers();
    }
    
    /// <summary>
    /// Registers all instruction handlers
    /// </summary>
    private void RegisterHandlers()
    {
        // Register group handlers
        RegisterGroup3Handlers();
        RegisterGroup1Handlers();
        
        // Register specific instruction handlers
        _handlers.Add(new Int3Handler(_codeBuffer, _decoder, _length));

        // Register Return handlers
        RegisterReturnHandlers();
        
        // Register Call handlers
        RegisterCallHandlers();
        
        // Register Jump handlers
        RegisterJumpHandlers();
        
        // Register Test handlers
        RegisterTestHandlers();
        
        // Register Xor handlers
        RegisterXorHandlers();
        
        // Register Data Transfer handlers
        RegisterDataTransferHandlers();
        
        // Register floating point handlers
        RegisterFloatingPointHandlers();
    }
    
    /// <summary>
    /// Registers all Group1 instruction handlers
    /// </summary>
    private void RegisterGroup1Handlers()
    {
        // ADD handlers
        _handlers.Add(new AddImmToRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AddImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // OR handlers
        _handlers.Add(new OrImmToRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new OrImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // ADC handlers
        _handlers.Add(new AdcImmToRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AdcImmToRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // SBB handlers
        _handlers.Add(new SbbImmFromRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SbbImmFromRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // AND handlers
        _handlers.Add(new AndImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new AndImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // SUB handlers
        _handlers.Add(new SubImmFromRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new SubImmFromRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // XOR handlers
        _handlers.Add(new XorImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
        
        // CMP handlers
        _handlers.Add(new CmpImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CmpImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
    }
    
    /// <summary>
    /// Registers all Group3 instruction handlers
    /// </summary>
    private void RegisterGroup3Handlers()
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
    }
    
    /// <summary>
    /// Registers all Jump instruction handlers
    /// </summary>
    private void RegisterJumpHandlers()
    {
        // JMP handlers
        _handlers.Add(new JmpRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JmpRel8Handler(_codeBuffer, _decoder, _length));
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
        // Add Xor handlers
        _handlers.Add(new XorAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorEaxImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorImmWithRm32SignExtendedHandler(_codeBuffer, _decoder, _length));
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
    /// Gets the handler that can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to decode</param>
    /// <returns>The handler that can decode the opcode, or null if no handler can decode it</returns>
    public IInstructionHandler? GetHandler(byte opcode)
    {
        return _handlers.FirstOrDefault(h => h.CanHandle(opcode));
    }
}
