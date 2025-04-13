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
        if (opcode != 0xF6)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the reg field is 0 (TEST operation)
        byte modRM = CodeBuffer[Decoder.GetPosition()];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 0; // 0 = TEST
    }

    /// <summary>
    /// Decodes a TEST r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Get the destination operand based on addressing mode
        if (mod == 3) // Register operand
        {
            // For direct register addressing, use the correct 8-bit register name
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();

        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm8:X2}";

        return true;
    }
}