namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m8, imm8 instruction (0xC6)
/// </summary>
public class MovRm8Imm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRm8Imm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRm8Imm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xC6;
    }

    /// <summary>
    /// Decodes a MOV r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // MOV r/m8, imm8 only uses reg=0
        if (reg != 0)
        {
            return false;
        }
        
        // For direct register addressing (mod == 3), use 8-bit register names
        if (mod == 3)
        {
            // Use 8-bit register names for direct register addressing
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }
        else
        {
            // Replace the size prefix with "byte ptr" for memory operands
            destOperand = destOperand.Replace("dword ptr", "byte ptr");
        }
        
        // Read the immediate value
        if (Decoder.GetPosition() >= Length)
        {
            return false;
        }
        
        byte imm8 = Decoder.ReadByte();
        
        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm8:X2}";
        
        return true;
    }
}
