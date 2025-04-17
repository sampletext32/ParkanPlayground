namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for WAIT/FWAIT instruction (0x9B)
/// </summary>
public class WaitHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the WaitHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public WaitHandler(InstructionDecoder decoder)
        : base(decoder)
    {
    }

    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // WAIT/FWAIT is encoded as 0x9B
        return opcode == 0x9B;
    }

    /// <summary>
    /// Decodes a WAIT/FWAIT instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Wait;
        
        // WAIT/FWAIT has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
