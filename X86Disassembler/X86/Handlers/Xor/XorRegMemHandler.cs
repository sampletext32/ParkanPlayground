namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r32, r/m32 instruction (0x33)
/// </summary>
public class XorRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorRegMemHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorRegMemHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x33;
    }
    
    /// <summary>
    /// Decodes an XOR r32, r/m32 instruction
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
        var (mod, reg, rm, srcOperand) = ModRMDecoder.ReadModRM();

        // Get the destination register
        string destReg = GetRegister32(reg);
        
        // Set the operands
        instruction.Operands = $"{destReg}, {srcOperand}";
        
        return true;
    }
}
