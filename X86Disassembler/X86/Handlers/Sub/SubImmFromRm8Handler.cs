namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m8, imm8 instruction (0x80 /5)
/// </summary>
public class SubImmFromRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubImmFromRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x80)
            return false;

        // Check if the reg field of the ModR/M byte is 5 (SUB)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";

        // Extract the fields from the ModR/M byte
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM();

        // Read the immediate byte
        var position = Decoder.GetPosition();
        if (position >= Length)
        {
            return false;
        }

        byte imm8 = CodeBuffer[position++];
        Decoder.SetPosition(position);

        // Set the instruction information
        // For mod == 3, the operand is a register
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, 8);
            instruction.Operands = $"{rmRegName}, 0x{imm8:X2}";
        }
        else // Memory operand
        {
            // Get the memory operand string
            string memOperand = ModRMDecoder.DecodeModRM(mod, rm, false);
            instruction.Operands = $"byte ptr {memOperand}, 0x{imm8:X2}";
        }

        return true;
    }
}