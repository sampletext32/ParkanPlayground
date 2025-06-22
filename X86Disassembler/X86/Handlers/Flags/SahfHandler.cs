namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for SAHF (Store AH into Flags) instruction (opcode 9E)
/// </summary>
public class SahfHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SahfHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SahfHandler(InstructionDecoder decoder)
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
        // SAHF is 9E
        return opcode == 0x9E;
    }
    
    /// <summary>
    /// Decodes a SAHF instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sahf;
        
        // SAHF has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
