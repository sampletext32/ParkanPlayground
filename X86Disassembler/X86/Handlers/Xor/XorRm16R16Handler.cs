namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m16, r16 instruction (0x31 with 0x66 prefix)
/// </summary>
public class XorRm16R16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorRm16R16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorRm16R16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x31 and there's an operand size prefix (0x66)
        return opcode == 0x31 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a XOR r/m16, r16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";

        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();

        // Get register name
        string regName = ModRMDecoder.GetRegisterName(reg, 16);

        // Set the operands
        instruction.Operands = $"{memOperand}, {regName}";

        return true;
    }
}
