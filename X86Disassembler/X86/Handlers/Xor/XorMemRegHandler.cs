namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m32, r32 instruction (0x31)
/// </summary>
public class XorMemRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorMemRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorMemRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Get the source register
        string srcReg = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {srcReg}";
        
        return true;
    }
}
