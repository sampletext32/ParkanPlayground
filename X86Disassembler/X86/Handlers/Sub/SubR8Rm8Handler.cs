namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r8, r/m8 instruction (0x2A)
/// </summary>
public class SubR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubR8Rm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubR8Rm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x2A;
    }

    /// <summary>
    /// Decodes a SUB r8, r/m8 instruction
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
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();

        // Get register name
        string regName = ModRMDecoder.GetRegisterName(reg, 8);

        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, 8);
            instruction.Operands = $"{regName}, {rmRegName}";
        }
        else // Memory operand
        {
            instruction.Operands = $"{regName}, byte ptr {memOperand}";
        }

        return true;
    }
}