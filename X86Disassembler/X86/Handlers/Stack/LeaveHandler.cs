namespace X86Disassembler.X86.Handlers.Stack;

/// <summary>
/// Handler for LEAVE instruction (0xC9)
/// High-level procedure exit that releases the stack frame set up by a previous ENTER instruction
/// </summary>
public class LeaveHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the LeaveHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LeaveHandler(InstructionDecoder decoder)
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
        // LEAVE is 0xC9
        return opcode == 0xC9;
    }
    
    /// <summary>
    /// Decodes a LEAVE instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Leave;
        
        // LEAVE has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
