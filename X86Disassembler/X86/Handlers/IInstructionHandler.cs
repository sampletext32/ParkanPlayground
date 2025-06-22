namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Interface for instruction handlers
/// </summary>
public interface IInstructionHandler
{
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    bool CanHandle(byte opcode);
    
    /// <summary>
    /// Decodes an instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    bool Decode(byte opcode, Instruction instruction);
}
