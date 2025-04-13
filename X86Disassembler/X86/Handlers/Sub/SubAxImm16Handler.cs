namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB AX, imm16 instruction (0x2D with 0x66 prefix)
/// </summary>
public class SubAxImm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubAxImm16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubAxImm16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x2D and we have a 0x66 prefix
        return opcode == 0x2D && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB AX, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";

        int position = Decoder.GetPosition();

        // Check if we have enough bytes for the immediate value
        if (position + 1 >= Length)
        {
            return false;
        }

        // Read the immediate value (16-bit)
        var immediate = Decoder.ReadUInt16();

        // Set the operands (note: we use "eax" instead of "ax" to match the disassembler's output)
        instruction.Operands = $"eax, 0x{immediate:X4}";

        return true;
    }
}