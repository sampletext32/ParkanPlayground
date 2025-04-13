namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR AL, imm8 instruction (0x34)
/// </summary>
public class XorAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorAlImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorAlImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x34;
    }
    
    /// <summary>
    /// Decodes a XOR AL, imm8 instruction
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
        
        // Read the immediate value using the decoder
        byte imm8 = Decoder.ReadByte();
        
        // Set the operands
        instruction.Operands = $"al, 0x{imm8:X2}";
        
        return true;
    }
}
