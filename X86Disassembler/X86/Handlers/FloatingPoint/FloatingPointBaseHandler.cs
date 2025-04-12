namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Base class for floating-point instruction handlers
/// </summary>
public abstract class FloatingPointBaseHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FloatingPointBaseHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    protected FloatingPointBaseHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
    }
}
