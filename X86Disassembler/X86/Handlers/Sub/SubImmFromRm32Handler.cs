namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m32, imm32 instruction (0x81 /5)
/// </summary>
public class SubImmFromRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubImmFromRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
            
        // Check if the reg field of the ModR/M byte is 5 (SUB)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 5; // 5 = SUB
    }
    
    /// <summary>
    /// Decodes a SUB r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";
        
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
        byte reg = (byte)((modRM & 0x38) >> 3); // Should be 5 for SUB
        byte rm = (byte)(modRM & 0x07);
        
        // Decode the destination operand
        string destOperand = ModRMDecoder.DecodeModRM(mod, rm, false);
        
        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }
        
        // Read the immediate value in little-endian format and convert to big-endian for display
        byte b0 = CodeBuffer[position];
        byte b1 = CodeBuffer[position + 1];
        byte b2 = CodeBuffer[position + 2];
        byte b3 = CodeBuffer[position + 3];
        
        // Convert from little-endian to big-endian for display
        uint imm32 = (uint)((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);
        
        // Advance the position
        Decoder.SetPosition(position + 4);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm32:X8}";
        
        return true;
    }
}
