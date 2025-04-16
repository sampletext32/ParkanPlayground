namespace X86Disassembler.X86.Handlers.Nop;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for the 2-byte NOP instruction (0x66 0x90)
/// This is actually XCHG AX, AX with an operand size prefix
/// </summary>
public class TwoByteNopHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TwoByteNopHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TwoByteNopHandler(InstructionDecoder decoder)
        : base(decoder)
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
    /// Decodes a 2-byte NOP instruction (XCHG AX, AX)
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Although this is actually XCHG AX, AX, it's treated as NOP in the x86 architecture
        // and is commonly disassembled as such
        instruction.Type = InstructionType.Nop;
        
        // NOP has no operands, even with the operand size prefix
        instruction.StructuredOperands = [];
        
        return true;
    }
}
