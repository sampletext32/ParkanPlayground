namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for the 2-byte NOP instruction (0x66 0x90)
/// This is a NOP with an operand size prefix
/// </summary>
public class TwoByteNopHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TwoByteNopHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TwoByteNopHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x90 and we have a 0x66 prefix
        return opcode == 0x90 && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a 2-byte NOP instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "nop";
        
        // NOP has no operands, even with the operand size prefix
        instruction.Operands = "";
        
        return true;
    }
}
