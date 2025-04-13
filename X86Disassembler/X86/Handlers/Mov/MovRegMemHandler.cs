namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, r/m32 instruction (0x8B) and MOV r8, r/m8 instruction (0x8A)
/// </summary>
public class MovRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegMemHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRegMemHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x8A || opcode == 0x8B;
    }

    /// <summary>
    /// Decodes a MOV r32, r/m32 or MOV r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();

        // Set the mnemonic
        instruction.Mnemonic = "mov";

        if (startPosition >= Length)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] {opcode};
            return true;
        }

        // Determine operand size (0 = 8-bit, 1 = 32-bit)
        bool operandSize32 = (opcode & 0x01) != 0;
        int operandSize = operandSize32
            ? 32
            : 8;

        // Use ModRMDecoder to decode the ModR/M byte
        var (mod, reg, rm, rmOperand) = ModRMDecoder.ReadModRM(); // false for 32-bit operand

        // Get register name based on size
        string regName = ModRMDecoder.GetRegisterName(reg, operandSize);

        // Get the position after decoding the ModR/M byte
        int newPosition = Decoder.GetPosition();

        // Set the operands - register is the destination, r/m is the source (for 0x8B)
        // This matches the correct x86 instruction format: MOV r32, r/m32
        instruction.Operands = $"{regName}, {rmOperand}";

        // Set the raw bytes
        int totalBytes = newPosition - startPosition + 1; // +1 for opcode
        byte[] rawBytes = new byte[totalBytes];
        rawBytes[0] = opcode;
        for (int i = 0; i < totalBytes - 1; i++)
        {
            if (startPosition + i < Length)
            {
                rawBytes[i + 1] = CodeBuffer[startPosition + i];
            }
        }

        instruction.RawBytes = rawBytes;

        return true;
    }
}