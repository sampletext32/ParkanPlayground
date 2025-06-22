using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Ret;

/// <summary>
/// Handler for RETF instruction with operand size prefix (0xCB with 0x66 prefix)
/// </summary>
public class Retf16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Retf16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Retf16Handler(InstructionDecoder decoder)
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
        // RETF with operand size prefix is encoded as 0xCB with 0x66 prefix
        // Only handle when the operand size prefix IS present
        return opcode == 0xCB && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a RETF instruction with operand size prefix
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
