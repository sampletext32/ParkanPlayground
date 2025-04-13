namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, r/m32 instruction (0x8B) and MOV r8, r/m8 instruction (0x8A)
/// </summary>
public class MovRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegMemHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRegMemHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x8A || opcode == 0x8B;
    }

    /// <summary>
    /// Decodes a MOV r32, r/m32 or MOV r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Determine operand size (0 = 8-bit, 1 = 32-bit)
        int operandSize = (opcode & 0x01) != 0 ? 32 : 8;

        // Use ModRMDecoder to decode the ModR/M byte
        var (mod, reg, rm, rmOperand) = ModRMDecoder.ReadModRM();

        // Get register name based on size
        string regName = ModRMDecoder.GetRegisterName(reg, operandSize);

        // Set the operands - register is the destination, r/m is the source (for 0x8B)
        // This matches the correct x86 instruction format: MOV r32, r/m32
        instruction.Operands = $"{regName}, {rmOperand}";

        return true;
    }
}