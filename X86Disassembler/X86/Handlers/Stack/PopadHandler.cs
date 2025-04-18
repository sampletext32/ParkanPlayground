namespace X86Disassembler.X86.Handlers.Stack;

/// <summary>
/// Handler for POPAD instruction (0x61)
/// Pops all general-purpose registers from the stack in the order: EDI, ESI, EBP, ESP (ignored), EBX, EDX, ECX, EAX
/// </summary>
public class PopadHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PopadHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PopadHandler(InstructionDecoder decoder)
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
        // POPAD is 0x61
        return opcode == 0x61;
    }
    
    /// <summary>
    /// Decodes a POPAD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Popad;
        
        // POPAD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
