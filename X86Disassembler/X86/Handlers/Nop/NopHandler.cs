namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for the NOP instruction (opcode 0x90)
/// </summary>
public class NopHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NopHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public NopHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // NOP (XCHG EAX, EAX)
        return opcode == 0x90;
    }

    /// <summary>
    /// Decodes a NOP instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "nop";
        
        // NOP has no operands
        instruction.Operands = "";
        
        return true;
    }
}
