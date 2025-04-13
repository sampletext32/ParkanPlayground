namespace X86Disassembler.X86.Handlers.Sbb;

/// <summary>
/// Handler for SBB r/m32, imm32 instruction (0x81 /3)
/// </summary>
public class SbbImmFromRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbImmFromRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SbbImmFromRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
            
        // Check if the reg field of the ModR/M byte is 3 (SBB)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 3; // 3 = SBB
    }
    
    /// <summary>
    /// Decodes a SBB r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sbb";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3); // Should be 3 for SBB
        byte rm = (byte)(modRM & 0x07);
        
        // Decode the destination operand
        string destOperand = ModRMDecoder.DecodeModRM(mod, rm, false);
        
        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }
        
        // Read the immediate value in little-endian format
        byte b0 = CodeBuffer[position];
        byte b1 = CodeBuffer[position + 1];
        byte b2 = CodeBuffer[position + 2];
        byte b3 = CodeBuffer[position + 3];
        
        // Format the immediate value as expected by the tests (0x12345678)
        // Note: The bytes are reversed to match the expected format in the tests
        string immStr = $"0x{b3:X2}{b2:X2}{b1:X2}{b0:X2}";
        
        // Advance the position past the immediate value
        position += 4;
        Decoder.SetPosition(position);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
