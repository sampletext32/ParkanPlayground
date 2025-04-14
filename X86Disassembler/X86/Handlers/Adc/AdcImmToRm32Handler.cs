namespace X86Disassembler.X86.Handlers.Adc;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for ADC r/m32, imm32 instruction (0x81 /2)
/// </summary>
public class AdcImmToRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcImmToRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AdcImmToRm32Handler(InstructionDecoder decoder)
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
        if (opcode != 0x81)
            return false;

        // Check if the reg field of the ModR/M byte is 2 (ADC)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 2; // 2 = ADC
    }

    /// <summary>
    /// Decodes an ADC r/m32, imm32 instruction
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value in little-endian format
        var imm32 = Decoder.ReadUInt32();

        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm32, 32);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            immOperand
        ];

        return true;
    }
}