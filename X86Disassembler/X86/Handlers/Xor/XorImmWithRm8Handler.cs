namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m8, imm8 instruction (0x80 /6)
/// </summary>
public class XorImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorImmWithRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Advance past the ModR/M byte
        Decoder.SetPosition(position + 1);
        
        // If mod == 3, then the r/m field specifies a register
        if (mod == 3)
        {
            // Get the r/m register name (8-bit)
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }
        else
        {
            // For memory operands, use the ModRMDecoder to get the full operand string

            // Replace "dword ptr" with "byte ptr" to indicate 8-bit operation
            destOperand = destOperand.Replace("dword ptr", "byte ptr");
        }
        
        // Get the updated position after ModR/M decoding
        position = Decoder.GetPosition();
        
        // Read the immediate value
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value
        byte imm8 = Decoder.ReadByte();
        
        // Format the immediate value
        string immStr = $"0x{imm8:X2}";
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
