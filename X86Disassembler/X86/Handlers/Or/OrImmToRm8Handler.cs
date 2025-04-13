namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m8, imm8 instruction (0x80 /1)
/// </summary>
public class OrImmToRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrImmToRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrImmToRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x80)
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
    /// Decodes an OR r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "or";

        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // For direct register addressing (mod == 3), use 8-bit register names
        if (mod == 3)
        {
            // Use 8-bit register names for direct register addressing
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }

        position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();

        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm8:X2}";

        return true;
    }
}