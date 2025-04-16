using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Not;

/// <summary>
/// Handler for NOT r/m8 instruction (0xF6 /2)
/// </summary>
public class NotRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NotRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public NotRm8Handler(InstructionDecoder decoder)
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
        // This handler only handles opcode 0xF6
        if (opcode != 0xF6)
            return false;

        // Check if the reg field of the ModR/M byte is 2 (NOT)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 2; // 2 = NOT
    }

    /// <summary>
    /// Decodes a NOT r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Not;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For NOT r/m8 (0xF6 /2):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM8();

        // Set the structured operands
        // NOT has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
