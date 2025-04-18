namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for CMC (Complement Carry Flag) instruction (opcode F5)
/// </summary>
public class CmcHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmcHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmcHandler(InstructionDecoder decoder)
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
        // CMC is F5
        return opcode == 0xF5;
    }
    
    /// <summary>
    /// Decodes a CMC instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cmc;
        
        // CMC has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
