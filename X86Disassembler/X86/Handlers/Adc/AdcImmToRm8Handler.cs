namespace X86Disassembler.X86.Handlers.Adc;

using Operands;

/// <summary>
/// Handler for ADC r/m8, imm8 instruction (0x80 /2)
/// </summary>
public class AdcImmToRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcImmToRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcImmToRm8Handler(InstructionDecoder decoder)
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
        if (opcode != 0x80)
            return false;

        // Check if the reg field of the ModR/M byte is 2 (ADC)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 2; // 2 = ADC
    }

    /// <summary>
    /// Decodes an ADC r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Adc;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For ADC r/m8, imm8 (0x80 /2):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM8();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();

        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
