namespace X86Disassembler.X86.Handlers.Push;

/// <summary>
/// Handler for PUSH r/m32 instruction (0xFF /6)
/// </summary>
public class PushRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public PushRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xFF;
    }

    /// <summary>
    /// Decodes a PUSH r/m32 instruction
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

        // PUSH r/m32 is encoded as FF /6
        if (reg != RegisterIndex.Sp)
        {
            return false;
        }

        // Set the mnemonic
        instruction.Mnemonic = "push";

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            instruction.Operands = destOperand;
        }
        else // Register operand
        {
            string rmName = ModRMDecoder.GetRegisterName(rm, 32);
            instruction.Operands = rmName;
        }

        return true;
    }
}
