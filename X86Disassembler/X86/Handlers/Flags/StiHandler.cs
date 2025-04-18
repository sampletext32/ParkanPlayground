namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for STI (Set Interrupt Flag) instruction (opcode FB)
/// </summary>
public class StiHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the StiHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public StiHandler(InstructionDecoder decoder)
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
        // STI is FB
        return opcode == 0xFB;
    }
    
    /// <summary>
    /// Decodes a STI instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sti;
        
        // STI has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
