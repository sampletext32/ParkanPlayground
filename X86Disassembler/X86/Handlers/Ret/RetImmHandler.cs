namespace X86Disassembler.X86.Handlers.Ret;

using Operands;

/// <summary>
/// Handler for RET instruction with immediate operand (0xC2)
/// </summary>
public class RetImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RetImmHandler(InstructionDecoder decoder) 
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
        // Only handle opcode 0xC2 when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0xC2 && !Decoder.HasOperandSizePrefix();
    }
    
    /// <summary>
    /// Decodes a RET instruction with immediate operand
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Ret;
        
        if (!Decoder.CanReadUShort())
        {
            return false;
        }
        
        // Read the immediate value
        ushort imm16 = Decoder.ReadUInt16();
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm16, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            immOperand
        ];
        
        return true;
    }
}
