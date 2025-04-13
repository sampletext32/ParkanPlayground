namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m16, imm16 instruction (0x81 /6 with 0x66 prefix)
/// </summary>
public class XorImmWithRm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorImmWithRm16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x81 || !Decoder.HasOperandSizePrefix())
            return false;
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        if (!Decoder.CanReadByte())
            return false;
            
        byte modRM = CodeBuffer[Decoder.GetPosition()];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m16, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // For direct register addressing (mod == 3), use the correct 16-bit register name
        if (mod == 3)
        {
            destOperand = ModRMDecoder.GetRegisterName(rm, 16);
        }
        else
        {
            // For memory operands, ensure we have the correct size prefix
            destOperand = destOperand.Replace("dword ptr", "word ptr");
        }
        
        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUShort())
        {
            return false;
        }
        
        // Read the immediate value
        ushort imm16 = Decoder.ReadUInt16();
        
        // Format the immediate value
        string immStr = $"0x{imm16:X4}";
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
