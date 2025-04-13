namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m8, imm8 instruction (0xF6 /0)
/// </summary>
public class TestImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestImmWithRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestImmWithRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // This handler only handles opcode 0xF6
        // The reg field check (for TEST operation) will be done in the Decode method
        return opcode == 0xF6;
    }

    /// <summary>
    /// Decodes a TEST r/m8, imm8 instruction
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM(true);

        // Check if the reg field is 0 (TEST operation)
        if (reg != RegisterIndex.A)
        {
            return false; // Not a TEST instruction
        }

        // Set the mnemonic
        instruction.Mnemonic = "test";

        // For direct register addressing (mod == 3), the r/m field specifies a register
        if (mod == 3)
        {
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }

        // Use the ModR/M decoder for memory addressing
        // Read the immediate value
        if (position >= Length)
        {
            return false;
        }

        byte imm8 = Decoder.ReadByte();

        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm8:X2}";

        return true;
    }
}