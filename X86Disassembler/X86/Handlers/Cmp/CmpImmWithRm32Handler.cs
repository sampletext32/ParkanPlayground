namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, imm32 instruction (0x81 /7)
/// </summary>
public class CmpImmWithRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpImmWithRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x81)
            return false;

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = CodeBuffer[Decoder.GetPosition()];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "cmp";

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();
        
        // Get the position after decoding the ModR/M byte
        int position = Decoder.GetPosition();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false; // Not enough bytes for the immediate value
        }

        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();

        // Format the destination operand based on addressing mode
        string destOperand;
        if (mod == 3) // Register addressing mode
        {
            // Get 32-bit register name
            destOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }
        else // Memory addressing mode
        {
            // Memory operand already includes dword ptr prefix
            destOperand = memOperand;
        }
        
        // Format the immediate value
        string immStr = $"0x{imm32:X8}";

        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";

        return true;
    }
}