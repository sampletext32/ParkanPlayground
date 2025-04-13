namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r/m8, r8 instruction (0x20)
/// </summary>
public class AndRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndRm8R8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AndRm8R8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x20;
    }

    /// <summary>
    /// Decodes an AND r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "and";

        if (!Decoder.CanReadByte())
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
            instruction.Operands = $"{rmRegName}, {regName}";
        }
        else // Memory operand
        {
            instruction.Operands = $"byte ptr {memOperand}, {regName}";
        }

        return true;
    }
}