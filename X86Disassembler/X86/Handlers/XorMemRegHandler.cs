namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for XOR r/m32, r32 instruction (0x31)
/// </summary>
public class XorMemRegHandler : InstructionHandler
{
    // ModR/M decoder
    private readonly ModRMDecoder _modRMDecoder;
    
    /// <summary>
    /// Initializes a new instance of the XorMemRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorMemRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
        _modRMDecoder = new ModRMDecoder(codeBuffer, decoder, length);
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        return opcode == 0x31;
    }
    
    /// <summary>
    /// Decodes an XOR r/m32, r32 instruction
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
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);
        
        // Decode the destination operand
        string destOperand = _modRMDecoder.DecodeModRM(mod, rm, false);
        
        // Get the source register
        string srcReg = GetRegister32(reg);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {srcReg}";
        
        return true;
    }
    
    /// <summary>
    /// Gets the 32-bit register name for the given register index
    /// </summary>
    /// <param name="reg">The register index</param>
    /// <returns>The register name</returns>
    private static string GetRegister32(byte reg)
    {
        string[] registerNames = { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi" };
        return registerNames[reg & 0x07];
    }
}
