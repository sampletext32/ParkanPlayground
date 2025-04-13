namespace X86Disassembler.X86.Handlers.Call;

/// <summary>
/// Handler for CALL r/m32 instruction (0xFF /2)
/// </summary>
public class CallRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CallRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CallRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
    /// Decodes a CALL r/m32 instruction
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

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // CALL r/m32 is encoded as FF /2
        if (reg != RegisterIndex.C)
        {
            return false;
        }

        // Set the mnemonic
        instruction.Mnemonic = "call";

        // For register operands, set the operand
        if (mod == 3)
        {
            // Register operand
            destOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }

        instruction.Operands = destOperand;

        return true;
    }
}