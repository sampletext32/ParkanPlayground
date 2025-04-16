namespace X86Disassembler.X86.Handlers.Ret;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for RET instruction (0xC3)
/// </summary>
public class RetHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RetHandler(InstructionDecoder decoder) 
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
        return opcode == 0xC3;
    }
    
    /// <summary>
    /// Decodes a RET instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Ret;
        
        // No operands for RET
        instruction.StructuredOperands = [];
        
        return true;
    }
}
