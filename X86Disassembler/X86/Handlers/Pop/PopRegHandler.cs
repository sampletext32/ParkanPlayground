namespace X86Disassembler.X86.Handlers.Pop;

using Operands;

/// <summary>
/// Handler for POP r32 instruction (0x58-0x5F)
/// </summary>
public class PopRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PopRegHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PopRegHandler(InstructionDecoder decoder) 
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
        return opcode >= 0x58 && opcode <= 0x5F;
    }
    
    /// <summary>
    /// Decodes a POP r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Pop;
        
        // Register is encoded in the low 3 bits of the opcode
        RegisterIndex reg = (RegisterIndex)(opcode & 0x07);
        
        // Create the register operand
        var regOperand = OperandFactory.CreateRegisterOperand(reg);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            regOperand
        ];
        
        return true;
    }
}
