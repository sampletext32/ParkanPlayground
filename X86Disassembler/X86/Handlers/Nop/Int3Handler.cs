namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for INT3 instruction (0xCC)
/// </summary>
public class Int3Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Int3Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Int3Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xCC;
    }
    
    /// <summary>
    /// Decodes an INT3 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "int3";
        
        // Set the operands
        instruction.Operands = "";
        
        return true;
    }
}
