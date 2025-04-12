namespace X86Disassembler.X86.Handlers.Pop;

/// <summary>
/// Handler for POP r32 instruction (0x58-0x5F)
/// </summary>
public class PopRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PopRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public PopRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode >= 0x58 && opcode <= 0x5F;
    }
    
    /// <summary>
    /// Decodes a POP r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "pop";
        
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Set the operands
        instruction.Operands = regName;
        
        return true;
    }
}
