namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for STD (Set Direction Flag) instruction (opcode FD)
/// </summary>
public class StdHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the StdHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public StdHandler(InstructionDecoder decoder)
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
        // STD is FD
        return opcode == 0xFD;
    }
    
    /// <summary>
    /// Decodes a STD instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Std;
        
        // STD has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
