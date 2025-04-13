namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r32, r/m32 instruction (0x3B)
/// </summary>
public class CmpR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpR32Rm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpR32Rm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x3B;
    }

    /// <summary>
    /// Decodes a CMP r32, r/m32 instruction
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

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Set the mnemonic
        instruction.Mnemonic = "cmp";

        // Get the register name
        string regName = ModRMDecoder.GetRegisterName(reg, 32);

        // For register operands, set the operand
        if (mod == 3)
        {
            // Register operand
            destOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }

        instruction.Operands = $"{regName}, {destOperand}";

        return true;
    }
}
