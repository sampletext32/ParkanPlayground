namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m32, imm32 instruction (0x81 /1)
/// </summary>
public class OrImmToRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrImmToRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrImmToRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x81)
            return false;

        // Check if the reg field of the ModR/M byte is 1 (OR)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 1; // 1 = OR
    }

    /// <summary>
    /// Decodes an OR r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "or";

        if (Decoder.GetPosition() >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        int position = Decoder.GetPosition();

        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }

        uint imm32 = Decoder.ReadUInt32();

        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm32:X8}";

        return true;
    }
}