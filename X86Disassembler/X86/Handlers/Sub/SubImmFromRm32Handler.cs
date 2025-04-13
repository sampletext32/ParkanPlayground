namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m32, imm32 instruction (0x81 /5)
/// </summary>
public class SubImmFromRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubImmFromRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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

        // Check if the reg field of the ModR/M byte is 5 (SUB)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";

        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte

        // Extract the fields from the ModR/M byte
        // Let the ModRMDecoder handle the ModR/M byte and any additional bytes (SIB, displacement)
        // This will update the decoder position to point after the ModR/M and any additional bytes
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Get the updated position after ModR/M decoding
        position = Decoder.GetPosition();

        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }

        // Read the immediate value in little-endian format
        var imm = Decoder.ReadUInt32();

        // Format the immediate value
        string immStr = $"0x{imm:X8}";

        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";

        return true;
    }
}