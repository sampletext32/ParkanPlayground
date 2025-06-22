namespace X86Disassembler.X86.Handlers.Stack;

/// <summary>
/// Handler for PUSHAD instruction (0x60)
/// Pushes all general-purpose registers onto the stack in the order: EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI
/// </summary>
public class PushadHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushadHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PushadHandler(InstructionDecoder decoder)
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
        // PUSHAD is 0x60
        return opcode == 0x60;
    }
    
    /// <summary>
    /// Decodes a PUSHAD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Pushad;
        
        // PUSHAD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
