using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Idiv;

/// <summary>
/// Handler for IDIV r/m8 instruction (0xF6 /7)
/// </summary>
public class IdivRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IdivRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public IdivRm8Handler(InstructionDecoder decoder)
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
        if (opcode != 0xF6)
            return false;

        // Check if the reg field of the ModR/M byte is 7 (IDIV)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 7; // 7 = IDIV
    }

    /// <summary>
    /// Decodes an IDIV r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.IDiv;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For IDIV r/m8 (0xF6 /7):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, _, _, operand) = ModRMDecoder.ReadModRM8();

        // Set the structured operands
        // IDIV has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
