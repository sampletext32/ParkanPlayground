namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for CLD (Clear Direction Flag) instruction (opcode FC)
/// </summary>
public class CldHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CldHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CldHandler(InstructionDecoder decoder)
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
        // CLD is FC
        return opcode == 0xFC;
    }
    
    /// <summary>
    /// Decodes a CLD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cld;
        
        // CLD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
