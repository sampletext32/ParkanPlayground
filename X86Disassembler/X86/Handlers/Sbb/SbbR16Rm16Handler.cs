using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sbb;

/// <summary>
/// Handler for SBB r16, r/m16 instruction (0x1B with 0x66 prefix)
/// </summary>
public class SbbR16Rm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbR16Rm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SbbR16Rm16Handler(InstructionDecoder decoder)
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
        // Only handle opcode 0x1B when the operand size prefix IS present
        return opcode == 0x1B && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a SBB r16, r/m16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sbb;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For SBB r16, r/m16 (0x1B with 0x66 prefix):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM16();

        // Create the register operand for the reg field (16-bit)
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
