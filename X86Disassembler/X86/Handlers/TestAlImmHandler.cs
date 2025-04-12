namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for TEST AL, imm8 instruction (0xA8)
/// </summary>
public class TestAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestAlImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestAlImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xA8;
    }
    
    /// <summary>
    /// Decodes a TEST AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value
        byte imm8 = CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        // Set the operands
        instruction.Operands = $"al, 0x{imm8:X2}";
        
        return true;
    }
}
