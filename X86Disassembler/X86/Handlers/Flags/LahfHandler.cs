namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for LAHF (Load Flags into AH) instruction (opcode 9F)
/// </summary>
public class LahfHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the LahfHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LahfHandler(InstructionDecoder decoder)
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
        // LAHF is 9F
        return opcode == 0x9F;
    }
    
    /// <summary>
    /// Decodes a LAHF instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Lahf;
        
        // LAHF has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
