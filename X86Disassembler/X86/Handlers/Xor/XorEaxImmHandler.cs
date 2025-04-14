namespace X86Disassembler.X86.Handlers.Xor;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for XOR EAX, imm32 instruction (0x35)
/// </summary>
public class XorEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorEaxImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorEaxImmHandler(InstructionDecoder decoder) 
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
        return opcode == 0x35;
    }
    
    /// <summary>
    /// Decodes a XOR EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;
        
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read the immediate value using the decoder
        uint imm32 = Decoder.ReadUInt32();
        
        // Create the register operand for EAX
        var eaxOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A);
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            eaxOperand,
            immOperand
        ];
        
        return true;
    }
}
