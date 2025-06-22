using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r16, r/m16 instruction (0x33 with 0x66 prefix)
/// </summary>
public class XorR16Rm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorR16Rm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorR16Rm16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x33 and there's an operand size prefix (0x66)
        return opcode == 0x33 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a XOR r16, r/m16 instruction
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
        // For XOR r16, r/m16 (0x33 with 0x66 prefix):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM16();

        // Note: The operand size is already set to 16-bit by the ReadModRM16 method

        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
