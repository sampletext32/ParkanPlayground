namespace X86Disassembler.X86.Handlers.Push;

/// <summary>
/// Handler for PUSH r32 instruction (0x50-0x57)
/// </summary>
public class PushRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public PushRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode >= 0x50 && opcode <= 0x57;
    }

    /// <summary>
    /// Decodes a PUSH r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "push";

        // Register is encoded in the low 3 bits of the opcode
        RegisterIndex reg = (RegisterIndex) (opcode & 0x07);
        string regName = ModRMDecoder.GetRegisterName(reg, 32);

        // Set the operands
        instruction.Operands = regName;

        return true;
    }
}