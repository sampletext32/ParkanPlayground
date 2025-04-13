namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m32, imm8 (sign-extended) instruction (0x83 /5)
/// </summary>
public class SubImmFromRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubImmFromRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x83)
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
    /// Decodes a SUB r/m32, imm8 (sign-extended) instruction
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
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value as a signed byte and sign-extend it
        sbyte imm8 = (sbyte)CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{(uint)imm8:X2}";
        
        return true;
    }
}
