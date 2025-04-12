namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP AL, imm8 instruction (0x3C)
/// </summary>
public class CmpAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpAlImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpAlImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x3C;
    }
    
    /// <summary>
    /// Decodes a CMP AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "cmp";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value
        byte imm8 = CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Set the operands
        instruction.Operands = $"al, 0x{imm8:X2}";
        
        return true;
    }
}
