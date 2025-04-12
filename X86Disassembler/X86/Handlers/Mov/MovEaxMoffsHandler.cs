namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV EAX, moffs32 instruction (0xA1) and MOV AL, moffs8 instruction (0xA0)
/// </summary>
public class MovEaxMoffsHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovEaxMoffsHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovEaxMoffsHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xA0 || opcode == 0xA1;
    }
    
    /// <summary>
    /// Decodes a MOV EAX, moffs32 or MOV AL, moffs8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        // Get the operand size and register name
        int operandSize = (opcode == 0xA0) ? 8 : 32;
        string regName = (opcode == 0xA0) ? "al" : "eax";
        
        // Read the memory offset
        uint offset = Decoder.ReadUInt32();
        if (Decoder.GetPosition() > Length)
        {
            return false;
        }
        
        // Set the operands
        instruction.Operands = $"{regName}, [0x{offset:X}]";
        
        return true;
    }
}
