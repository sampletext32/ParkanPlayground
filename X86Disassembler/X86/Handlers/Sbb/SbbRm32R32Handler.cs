using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sbb;

/// <summary>
/// Handler for SBB r/m32, r32 instruction (0x19)
/// </summary>
public class SbbRm32R32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbRm32R32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SbbRm32R32Handler(InstructionDecoder decoder)
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
        // Only handle opcode 0x19 when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0x19 && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a SBB r/m32, r32 instruction
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
        // For SBB r/m32, r32 (0x19):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The reg field specifies the source register
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM();

        // Create the register operand for the reg field
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
