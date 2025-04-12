namespace X86Disassembler.X86.Handlers.Group3;

/// <summary>
/// Handler for NOT r/m32 instruction (0xF7 /2)
/// </summary>
public class NotRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NotRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public NotRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // This handler only handles opcode 0xF7
        if (opcode != 0xF7)
            return false;
            
        // Check if the reg field of the ModR/M byte is 2 (NOT)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 2; // 2 = NOT
    }
    
    /// <summary>
    /// Decodes a NOT r/m32 instruction
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
        byte modRM = CodeBuffer[position++];
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3); // Should be 2 for NOT
        byte rm = (byte)(modRM & 0x07);
        
        // Verify this is a NOT instruction
        if (reg != 2)
        {
            return false;
        }
        
        // Set the mnemonic
        instruction.Mnemonic = "not";
        
        Decoder.SetPosition(position);
        
        // For direct register addressing (mod == 3), the r/m field specifies a register
        string operand;
        if (mod == 3)
        {
            operand = GetRegister32(rm);
        }
        else
        {
            // Use the ModR/M decoder for memory addressing
            operand = ModRMDecoder.DecodeModRM(mod, rm, false);
        }
        
        // Set the operands
        instruction.Operands = operand;
        
        return true;
    }
}
