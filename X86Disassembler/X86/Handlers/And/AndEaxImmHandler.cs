namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND EAX, imm32 instruction (0x25)
/// </summary>
public class AndEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndEaxImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AndEaxImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x25;
    }
    
    /// <summary>
    /// Decodes an AND EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "and";

        // Read immediate value
        if (!Decoder.CanReadUInt())
        {
            instruction.Operands = "eax, ??";
            return true;
        }
        
        // Read immediate value
        uint imm32 = Decoder.ReadUInt32();
        
        // Set operands
        instruction.Operands = $"eax, 0x{imm32:X8}";
        
        return true;
    }
}
