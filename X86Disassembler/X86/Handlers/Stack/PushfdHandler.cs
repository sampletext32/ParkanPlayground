namespace X86Disassembler.X86.Handlers.Stack;

/// <summary>
/// Handler for PUSHFD instruction (0x9C)
/// Pushes the EFLAGS register onto the stack
/// </summary>
public class PushfdHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushfdHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PushfdHandler(InstructionDecoder decoder)
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
        // PUSHFD is 0x9C
        return opcode == 0x9C;
    }
    
    /// <summary>
    /// Decodes a PUSHFD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Pushfd;
        
        // PUSHFD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
