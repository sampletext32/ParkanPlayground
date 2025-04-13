namespace X86Disassembler.X86.Handlers.Adc;

/// <summary>
/// Handler for ADC r/m32, imm8 (sign-extended) instruction (0x83 /2)
/// </summary>
public class AdcImmToRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AdcImmToRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AdcImmToRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
        : base(codeBuffer, decoder, length)
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

        // Check if the reg field of the ModR/M byte is 2 (ADC)
        int position = Decoder.GetPosition();
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 2; // 2 = ADC
    }

    /// <summary>
    /// Decodes an ADC r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "adc";

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value (sign-extended from 8 to 32 bits)
        int imm32 = (sbyte) Decoder.ReadByte();

        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm32:X8}";

        return true;
    }
}