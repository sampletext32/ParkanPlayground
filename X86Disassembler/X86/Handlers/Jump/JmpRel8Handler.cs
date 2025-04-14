namespace X86Disassembler.X86.Handlers.Jump;

using Operands;

/// <summary>
/// Handler for JMP rel8 instruction (0xEB)
/// </summary>
public class JmpRel8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the JmpRel8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public JmpRel8Handler(InstructionDecoder decoder)
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
        return opcode == 0xEB;
    }
    
    /// <summary>
    /// Decodes a JMP rel8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Jmp;
        
        // Check if we can read the offset byte
        if (!Decoder.CanReadByte())
        {
            return true;
        }

        sbyte offset = (sbyte)Decoder.ReadByte();
        
        // Calculate target address (instruction address + instruction length + offset)
        ulong targetAddress = instruction.Address + 2UL + (uint)offset;
        
        // Create the target address operand
        var targetOperand = OperandFactory.CreateRelativeOffsetOperand((uint)targetAddress, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            targetOperand
        ];
        
        return true;
    }
}
