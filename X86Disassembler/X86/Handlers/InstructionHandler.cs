namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Abstract base class for instruction handlers
/// </summary>
public abstract class InstructionHandler : IInstructionHandler
{
    // The instruction decoder that owns this handler
    protected readonly InstructionDecoder Decoder;

    // ModRM decoder for handling addressing modes
    protected readonly ModRMDecoder ModRMDecoder;

    /// <summary>
    /// Initializes a new instance of the InstructionHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    protected InstructionHandler(InstructionDecoder decoder)
    {
        Decoder = decoder;
        ModRMDecoder = new ModRMDecoder(decoder);
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