using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Ret;

/// <summary>
/// Handler for RETF instruction (0xCB)
/// </summary>
public class RetfHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetfHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RetfHandler(InstructionDecoder decoder)
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
        // RETF is encoded as 0xCB
        // Only handle when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0xCB && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a RETF instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Retf;

        // RETF has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
