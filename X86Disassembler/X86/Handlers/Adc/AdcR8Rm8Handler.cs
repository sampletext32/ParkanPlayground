using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Adc;

/// <summary>
/// Handler for ADC r8, r/m8 instruction (0x12)
/// </summary>
public class AdcR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcR8Rm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x12;
    }

    /// <summary>
    /// Decodes an ADC r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Adc;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For ADC r8, r/m8 (0x12):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM8();

        // Create the register operand for the reg field
        var destinationOperand = OperandFactory.CreateRegisterOperand8(reg);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
