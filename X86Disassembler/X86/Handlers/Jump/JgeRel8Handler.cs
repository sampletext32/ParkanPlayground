namespace X86Disassembler.X86.Handlers.Jump;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for JGE rel8 instruction (0x7D)
/// </summary>
public class JgeRel8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the JgeRel8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public JgeRel8Handler(InstructionDecoder decoder)
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
        return opcode == 0x7D;
    }
    
    /// <summary>
    /// Decodes a JGE rel8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Jge;
        
        // Check if we can read the offset byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the offset byte
        sbyte offset = (sbyte)Decoder.ReadByte();
        
        // The instruction.Address already includes the base address from the disassembler
        ulong targetAddress = instruction.Address + 2UL + (ulong) offset;
        
        // Create the relative offset operand with the absolute target address
        var targetOperand = OperandFactory.CreateRelativeOffsetOperand((uint)targetAddress, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            targetOperand
        ];
        
        return true;
    }
}
