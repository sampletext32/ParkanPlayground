namespace X86Disassembler.X86.Handlers.Xchg;

/// <summary>
/// Handler for XCHG EAX, r32 instruction (0x90-0x97)
/// </summary>
public class XchgEaxRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XchgEaxRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XchgEaxRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode >= 0x90 && opcode <= 0x97;
    }
    
    /// <summary>
    /// Decodes an XCHG EAX, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Special case for NOP (XCHG EAX, EAX)
        if (opcode == 0x90)
        {
            instruction.Mnemonic = "nop";
            instruction.Operands = "";
            return true;
        }
        
        // Set the mnemonic
        instruction.Mnemonic = "xchg";
        
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Set the operands
        instruction.Operands = $"eax, {regName}";
        
        return true;
    }
}
