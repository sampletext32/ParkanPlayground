namespace X86Disassembler.X86.Handlers.Push;

using Operands;

/// <summary>
/// Handler for PUSH imm16 instruction with operand size prefix (0x66 0x68)
/// </summary>
public class PushImm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushImm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PushImm16Handler(InstructionDecoder decoder) 
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
        // Check for operand size prefix (66h) followed by PUSH imm (68h)
        if (opcode != 0x68)
        {
            return false;
        }
        
        // Check if we have an operand size prefix
        return Decoder.HasOperandSizePrefix();
    }
    
    /// <summary>
    /// Decodes a PUSH imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Push;

        // Check if we have enough bytes for the 16-bit immediate
        if(!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the 16-bit immediate value
        ushort imm16 = Decoder.ReadUInt16();
        
        // Create an immediate operand with 16-bit size
        var immOperand = new ImmediateOperand(imm16, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            immOperand
        ];

        return true;
    }
}
