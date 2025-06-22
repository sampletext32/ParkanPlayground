namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for LOCK prefix (0xF0)
/// </summary>
public class LockHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the LockHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LockHandler(InstructionDecoder decoder)
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
        // LOCK prefix is encoded as 0xF0
        return opcode == 0xF0;
    }

    /// <summary>
    /// Decodes a LOCK prefix
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Lock;
        
        // LOCK prefix has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
