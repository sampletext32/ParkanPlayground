namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for HLT instruction (0xF4)
/// </summary>
public class HltHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the HltHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public HltHandler(InstructionDecoder decoder)
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
        // HLT is encoded as 0xF4
        return opcode == 0xF4;
    }

    /// <summary>
    /// Decodes a HLT instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Hlt;
        
        // HLT has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
