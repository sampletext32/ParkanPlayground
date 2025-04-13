namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, imm32 instruction (0x81 /7)
/// </summary>
public class CmpImmWithRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpImmWithRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "cmp";

        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }

        // Read the immediate value in little-endian format
        var imm = Decoder.ReadUInt32();

        // Format the immediate value as expected by the tests (0x12345678)
        // Note: The bytes are reversed to match the expected format in the tests
        string immStr = $"0x{imm:X8}";

        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";

        return true;
    }
}