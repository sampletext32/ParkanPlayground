namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV moffs32, EAX instruction (0xA3) and MOV moffs8, AL instruction (0xA2)
/// </summary>
public class MovMoffsEaxHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovMoffsEaxHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovMoffsEaxHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xA2 || opcode == 0xA3;
    }

    /// <summary>
    /// Decodes a MOV moffs32, EAX or MOV moffs8, AL instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";

        // Get the operand size and register name
        int operandSize = opcode == 0xA2 ? 8 : 32;

        string regName = ModRMDecoder.GetRegisterName(RegisterIndex.A, operandSize);

        // Read the memory offset
        uint offset = Decoder.ReadUInt32();
        if (Decoder.GetPosition() > Length)
        {
            return false;
        }

        // Set the operands
        instruction.Operands = $"[0x{offset:X}], {regName}";

        return true;
    }
}