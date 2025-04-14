namespace X86Disassembler.X86.Handlers.Nop;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for the NOP instruction (opcode 0x90)
/// </summary>
public class NopHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NopHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public NopHandler(InstructionDecoder decoder)
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
        // Set the instruction type
        instruction.Type = InstructionType.Nop;
        
        // NOP has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
