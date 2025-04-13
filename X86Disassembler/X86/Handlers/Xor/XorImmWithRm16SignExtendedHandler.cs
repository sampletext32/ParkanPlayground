namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m16, imm8 (sign-extended) instruction (0x83 /6 with 0x66 prefix)
/// </summary>
public class XorImmWithRm16SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm16SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorImmWithRm16SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x83 || !Decoder.HasOperandSizePrefix())
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
    /// Decodes a XOR r/m16, imm8 (sign-extended) instruction
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
        
        // Get the updated position after ModR/M decoding
        position = Decoder.GetPosition();
        
        // Read the immediate value (sign-extended from 8 to 16 bits)
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value and sign-extend it to 16 bits
        short imm16 = (sbyte)Decoder.ReadByte();
        
        // Format the immediate value
        string immStr = $"0x{(ushort)imm16:X4}";
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
