namespace X86Disassembler.X86.Handlers.Adc;

using Operands;

/// <summary>
/// Handler for ADC r/m16, imm8 (sign-extended) instruction (0x83 /2 with 0x66 prefix)
/// </summary>
public class AdcImmToRm16SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcImmToRm16SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcImmToRm16SignExtendedHandler(InstructionDecoder decoder) 
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
        // ADC r/m16, imm8 (sign-extended) is encoded as 0x83 /2 with 0x66 prefix
        if (opcode != 0x83)
        {
            return false;
        }

        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the reg field of the ModR/M byte is 2 (ADC)
        var reg = ModRMDecoder.PeakModRMReg();

        // Only handle when the operand size prefix is present
        return reg == 2 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a ADC r/m16, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Adc;

        // For ADC r/m16, imm8 (sign-extended) (0x83 /2 with 0x66 prefix):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand (sign-extended from 8 to 16 bits)
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM16();

        // Note: The operand size is already set to 16-bit by the ReadModRM16 method

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value (sign-extended from 8 to 16 bits)
        short imm16 = (sbyte)Decoder.ReadByte();

        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand((ushort)imm16, 16);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
