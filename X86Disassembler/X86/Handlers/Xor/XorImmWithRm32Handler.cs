namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m32, imm32 instruction (0x81 /6)
/// </summary>
public class XorImmWithRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorImmWithRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        if (!Decoder.CanReadByte())
            return false;
            
        byte modRM = CodeBuffer[Decoder.GetPosition()];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";
        
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        var imm = Decoder.ReadUInt32();

        // Format the immediate value
        string immStr = $"0x{imm:X}";
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
