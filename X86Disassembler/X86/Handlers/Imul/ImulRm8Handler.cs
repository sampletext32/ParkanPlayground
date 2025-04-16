using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Imul;

/// <summary>
/// Handler for IMUL r/m8 instruction (0xF6 /5)
/// </summary>
public class ImulRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ImulRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ImulRm8Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 5 (IMUL)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 5; // 5 = IMUL
    }

    /// <summary>
    /// Decodes an IMUL r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.IMul;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For IMUL r/m8 (0xF6 /5):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, _, _, operand) = ModRMDecoder.ReadModRM8();
        
        // Set the structured operands
        // IMUL has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
