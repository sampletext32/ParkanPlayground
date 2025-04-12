namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Factory for creating instruction handlers
/// </summary>
public class InstructionHandlerFactory
{
    private readonly byte[] _codeBuffer;
    private readonly InstructionDecoder _decoder;
    private readonly int _length;
    private readonly List<IInstructionHandler> _handlers = new List<IInstructionHandler>();
    
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
        // Register specific instruction handlers
        _handlers.Add(new RetHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new RetImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JmpRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new JmpRel8Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new CallRel32Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new XorRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestRegMemHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestAlImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TestEaxImmHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new FnstswHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new ConditionalJumpHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new TwoByteConditionalJumpHandler(_codeBuffer, _decoder, _length));
        
        // Register group handlers for instructions that share similar decoding logic
        _handlers.Add(new Group1Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new Group3Handler(_codeBuffer, _decoder, _length));
        _handlers.Add(new FloatingPointHandler(_codeBuffer, _decoder, _length));
        _handlers.Add(new DataTransferHandler(_codeBuffer, _decoder, _length));
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
