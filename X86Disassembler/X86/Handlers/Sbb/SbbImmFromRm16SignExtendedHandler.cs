using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sbb;

/// <summary>
/// Handler for SBB r/m16, imm8 (sign-extended) instruction (0x83 /3 with 0x66 prefix)
/// </summary>
public class SbbImmFromRm16SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbImmFromRm16SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SbbImmFromRm16SignExtendedHandler(InstructionDecoder decoder)
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
        if (opcode != 0x83)
            return false;

        // Must have operand size prefix for 16-bit operation
        if (!Decoder.HasOperandSizePrefix())
            return false;

        // Check if the reg field of the ModR/M byte is 3 (SBB)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 3; // 3 = SBB
    }

    /// <summary>
    /// Decodes a SBB r/m16, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sbb;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For SBB r/m16, imm8 (sign-extended) (0x83 /3 with 0x66 prefix):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM16();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Sign-extend to 16 bits
        sbyte imm8 = (sbyte)Decoder.ReadByte();
        
        // Create the immediate operand with sign extension
        var sourceOperand = OperandFactory.CreateImmediateOperand((ushort)imm8, 16);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
