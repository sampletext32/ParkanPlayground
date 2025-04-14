namespace X86Disassembler.X86.Handlers.Xor;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for XOR r/m16, r16 instruction (0x31 with 0x66 prefix)
/// </summary>
public class XorRm16R16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorRm16R16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorRm16R16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x31 and there's an operand size prefix (0x66)
        return opcode == 0x31 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a XOR r/m16, r16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();

        // Create the source register operand (16-bit)
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 16);
        
        // For all operands, we need to adjust the size to 16-bit
        // This ensures register operands also get the correct size
        destinationOperand.Size = 16;
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
