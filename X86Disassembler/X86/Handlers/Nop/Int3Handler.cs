namespace X86Disassembler.X86.Handlers.Nop;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for INT3 instruction (0xCC)
/// </summary>
public class Int3Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Int3Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Int3Handler(InstructionDecoder decoder) 
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
        return opcode == 0xCC;
    }
    
    /// <summary>
    /// Decodes an INT3 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Int;
        
        // INT3 has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
