using X86Disassembler.X86.Handlers.Jump;
using X86Disassembler.X86.Handlers.Test;

namespace X86Disassembler.X86.Handlers;

using X86Disassembler.X86.Handlers.Group1;
using X86Disassembler.X86.Handlers.Group3;

/// <summary>
/// Factory for creating instruction handlers
/// </summary>
public class InstructionHandlerFactory
{
    private readonly byte[] _codeBuffer;
    private readonly InstructionDecoder _decoder;
    private readonly int _length;
    private readonly List<IInstructionHandler> _handlers = [];
    
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
        
        // Register all instruction handlers
        RegisterHandlers();
    }
    
    /// <summary>
    /// Registers all instruction handlers
    /// </summary>
    private void RegisterHandlers()
    {
        // Register Group3 handlers first to ensure they take precedence
        // over generic handlers for the same opcodes
        RegisterGroup3Handlers();
        
        // Register Group1 handlers
        RegisterGroup1Handlers();
        
        // Register specific instruction handlers
        _handlers.Add(new RetHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new RetImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CallRel32Handler(_codeBuffer, _decoder, _length));
        
        // XOR handlers
        _handlers.Add(new XorRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorMemRegHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorEaxImmHandler(_codeBuffer, _decoder, _length));

        _handlers.Add(new FnstswHandler(_codeBuffer, _decoder, _length));

        // TEST handlers
        _handlers.Add(new TestImmWithRm32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestImmWithRm8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestRegMem8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestEaxImmHandler(_codeBuffer, _decoder, _length));
        
        // JMP handlers
        _handlers.Add(new JmpRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JmpRel8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new ConditionalJumpHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TwoByteConditionalJumpHandler(_codeBuffer, _decoder, _length));
        
        // Register group handlers for instructions that share similar decoding logic
        _handlers.Add(new FloatingPointHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new DataTransferHandler(_codeBuffer, _decoder, _length));
    }
    
    /// <summary>
    /// Registers the Group1 handlers
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
    /// Registers the Group3 handlers
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
    /// Gets a handler that can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to decode</param>
    /// <returns>A handler that can decode the opcode, or null if no handler is found</returns>
    public IInstructionHandler? GetHandler(byte opcode)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(opcode))
            {
                return handler;
            }
        }
        
        return null;
    }
}
