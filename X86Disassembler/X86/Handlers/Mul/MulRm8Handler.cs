using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mul;

/// <summary>
/// Handler for MUL r/m8 instruction (0xF6 /4)
/// </summary>
public class MulRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MulRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MulRm8Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 4 (MUL)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 4; // 4 = MUL
    }

    /// <summary>
    /// Decodes a MUL r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mul;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For MUL r/m8 (0xF6 /4):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM8();
        
        // Verify this is a MUL instruction
        // The reg field should be 4 (MUL), which maps to RegisterIndex8.AH in our enum
        if (reg != RegisterIndex8.AH)
        {
            return false;
        }

        // Set the structured operands
        // MUL has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
