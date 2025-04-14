namespace X86Disassembler.X86.Handlers.Push;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for PUSH imm32 instruction (0x68)
/// </summary>
public class PushImm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushImm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PushImm32Handler(InstructionDecoder decoder) 
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
        return opcode == 0x68;
    }
    
    /// <summary>
    /// Decodes a PUSH imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Push;

        if(!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            immOperand
        ];

        return true;
    }
}