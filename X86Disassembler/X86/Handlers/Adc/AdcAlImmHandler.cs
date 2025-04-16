using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Adc;

/// <summary>
/// Handler for ADC AL, imm8 instruction (0x14)
/// </summary>
public class AdcAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcAlImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcAlImmHandler(InstructionDecoder decoder)
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
        return opcode == 0x14;
    }

    /// <summary>
    /// Decodes an ADC AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Adc;

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate byte
        var imm8 = Decoder.ReadByte();

        // Create the AL register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);

        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
