namespace X86Disassembler.X86.Handlers.Group3;

/// <summary>
/// Handler for DIV r/m32 instruction (0xF7 /6)
/// </summary>
public class DivRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the DivRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public DivRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0xF7)
            return false;
            
        // Check if the reg field of the ModR/M byte is 6 (DIV)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = DIV
    }
    
    /// <summary>
    /// Decodes a DIV r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "div";
        
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
        byte reg = (byte)((modRM & 0x38) >> 3); // Should be 6 for DIV
        byte rm = (byte)(modRM & 0x07);
        
        // Decode the operand
        string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
        
        // Set the operands
        instruction.Operands = operand;
        
        return true;
    }
}
