namespace X86Disassembler.X86.Handlers.Flags;

/// <summary>
/// Handler for CLI (Clear Interrupt Flag) instruction (opcode FA)
/// </summary>
public class CliHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CliHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CliHandler(InstructionDecoder decoder)
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
        // CLI is FA
        return opcode == 0xFA;
    }
    
    /// <summary>
    /// Decodes a CLI instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cli;
        
        // CLI has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
