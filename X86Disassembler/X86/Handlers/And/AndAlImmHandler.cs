namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND AL, imm8 instruction (0x24)
/// </summary>
public class AndAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndAlImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AndAlImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x24;
    }
    
    /// <summary>
    /// Decodes an AND AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "and";
        
        int position = Decoder.GetPosition();
        
        // Read immediate value
        if (position >= Length)
        {
            instruction.Operands = "al, ??";
            return true;
        }
        
        // Read immediate value
        byte imm8 = Decoder.ReadByte();
        
        // Set operands
        instruction.Operands = $"al, 0x{imm8:X2}";
        
        return true;
    }
}
