namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR EAX, imm32 instruction (0x35)
/// </summary>
public class XorEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorEaxImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorEaxImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x35;
    }
    
    /// <summary>
    /// Decodes a XOR EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";
        
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Read the immediate value
        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        // Set the operands
        instruction.Operands = $"eax, 0x{imm32:X8}";
        
        return true;
    }
}
