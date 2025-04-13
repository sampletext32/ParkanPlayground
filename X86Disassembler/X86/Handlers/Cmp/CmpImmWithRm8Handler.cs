namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m8, imm8 instruction (0x80 /7)
/// </summary>
public class CmpImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpImmWithRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        int position = Decoder.GetPosition();
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m8, imm8 instruction
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

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false; // Not enough bytes for the immediate value
        }

        // Read the immediate byte
        byte imm8 = Decoder.ReadByte();

        // Format the destination operand based on addressing mode
        string destOperand;
        if (mod == 3) // Register addressing mode
        {
            // Get 8-bit register name
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }
        else // Memory addressing mode
        {
            // Ensure we have the correct size prefix (byte ptr)
            if (memOperand.Contains("dword ptr") || memOperand.Contains("qword ptr"))
            {
                // Replace the size prefix with "byte ptr"
                destOperand = memOperand.Replace(memOperand.StartsWith("dword") ? "dword ptr " : "qword ptr ", "byte ptr ");
            }
            else
            {
                // Add the byte ptr prefix if it doesn't have one
                destOperand = $"byte ptr {memOperand}";
            }
        }
        
        // Format the immediate value
        string immStr = $"0x{imm8:X2}";

        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";

        return true;
    }
}