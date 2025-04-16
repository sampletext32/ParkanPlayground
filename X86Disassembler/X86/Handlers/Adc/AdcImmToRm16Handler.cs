namespace X86Disassembler.X86.Handlers.Adc;

using Operands;

/// <summary>
/// Handler for ADC r/m16, imm16 instruction (0x81 /2 with 0x66 prefix)
/// </summary>
public class AdcImmToRm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcImmToRm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcImmToRm16Handler(InstructionDecoder decoder) 
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
        // ADC r/m16, imm16 is encoded as 0x81 /2 with 0x66 prefix
        if (opcode != 0x81)
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
    /// Decodes a ADC r/m16, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Adc;

        // Read the ModR/M byte, specifying that we're dealing with 16-bit operands
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM16();

        // Note: The operand size is already set to 16-bit by the ReadModRM16 method

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the immediate value
        ushort imm16 = Decoder.ReadUInt16();

        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm16, 16);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
