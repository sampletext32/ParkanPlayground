namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m16, r16 instruction (0x29 with 0x66 prefix)
/// </summary>
public class SubRm16R16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubRm16R16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubRm16R16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x29 and we have a 0x66 prefix
        return opcode == 0x29 && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB r/m16, r16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Get register name (16-bit)
        string regName = ModRMDecoder.GetRegisterName(reg, 16);

        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, 16);
            instruction.Operands = $"{rmRegName}, {regName}";
        }
        else // Memory operand
        {
            destOperand = destOperand.Replace("dword", "word");

            instruction.Operands = $"{destOperand}, {regName}";
        }

        return true;
    }
}