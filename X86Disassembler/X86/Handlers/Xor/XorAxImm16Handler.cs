namespace X86Disassembler.X86.Handlers.Xor;

using Operands;

/// <summary>
/// Handler for XOR AX, imm16 instruction (0x35 with 0x66 prefix)
/// </summary>
public class XorAxImm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorAxImm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorAxImm16Handler(InstructionDecoder decoder) 
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
        // Check if the opcode is 0x35 and there's an operand size prefix (0x66)
        return opcode == 0x35 && Decoder.HasOperandSizePrefix();
    }
    
    /// <summary>
    /// Decodes a XOR AX, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;

        if (!Decoder.CanReadUShort())
        {
            return false;
        }
        
        // Read the immediate value using the decoder
        ushort imm16 = Decoder.ReadUInt16();
        
        // Create the register operand for AX
        var axOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 16);
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm16, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            axOperand,
            immOperand
        ];
        
        return true;
    }
}
