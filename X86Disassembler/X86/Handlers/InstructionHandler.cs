namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Base class for all instruction handlers
/// </summary>
public abstract class InstructionHandler
{
    // Buffer containing the code to decode
    protected readonly byte[] CodeBuffer;
    
    // The instruction decoder that owns this handler
    protected readonly InstructionDecoder Decoder;
    
    // Length of the buffer
    protected readonly int Length;
    
    // ModRM decoder for handling addressing modes
    protected readonly ModRMDecoder ModRMDecoder;
    
    /// <summary>
    /// Initializes a new instance of the InstructionHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    protected InstructionHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
    {
        CodeBuffer = codeBuffer;
        Decoder = decoder;
        Length = length;
        ModRMDecoder = new ModRMDecoder(codeBuffer, decoder, length);
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public abstract bool CanHandle(byte opcode);
    
    /// <summary>
    /// Decodes an instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public abstract bool Decode(byte opcode, Instruction instruction);
}
