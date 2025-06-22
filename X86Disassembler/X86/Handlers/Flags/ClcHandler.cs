namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for CLC (Clear Carry Flag) instruction (opcode F8)
/// </summary>
public class ClcHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ClcHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ClcHandler(InstructionDecoder decoder)
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
        // CLC is F8
        return opcode == 0xF8;
    }
    
    /// <summary>
    /// Decodes a CLC instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Clc;
        
        // CLC has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
