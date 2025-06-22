namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for IRET/IRETD instruction (0xCF)
/// </summary>
public class IretHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IretHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public IretHandler(InstructionDecoder decoder)
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
        // IRET/IRETD is encoded as 0xCF
        return opcode == 0xCF;
    }

    /// <summary>
    /// Decodes an IRET/IRETD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Iret;
        
        // IRET/IRETD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
