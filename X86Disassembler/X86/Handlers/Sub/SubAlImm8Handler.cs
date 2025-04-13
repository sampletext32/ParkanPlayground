namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB AL, imm8 instruction (0x2C)
/// </summary>
public class SubAlImm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubAlImm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubAlImm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x2C;
    }

    /// <summary>
    /// Decodes a SUB AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the immediate byte
        byte imm8 = Decoder.ReadByte();

        // Set the instruction information
        instruction.Mnemonic = "sub";
        instruction.Operands = $"al, 0x{imm8:X2}";

        return true;
    }
}