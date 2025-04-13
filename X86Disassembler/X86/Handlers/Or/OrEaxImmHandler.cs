namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR EAX, imm32 instruction (0x0D)
/// </summary>
public class OrEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrEaxImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrEaxImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x0D;
    }

    /// <summary>
    /// Decodes an OR EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate dword (little-endian)

        uint imm32 = Decoder.ReadUInt32();

        // Set the mnemonic
        instruction.Mnemonic = "or";

        // Set the operands
        instruction.Operands = $"eax, 0x{imm32:X8}";

        return true;
    }
}
