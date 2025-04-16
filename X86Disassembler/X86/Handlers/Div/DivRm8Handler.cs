using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Div;

/// <summary>
/// Handler for DIV r/m8 instruction (0xF6 /6)
/// </summary>
public class DivRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the DivRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public DivRm8Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 6 (DIV)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 6; // 6 = DIV
    }

    /// <summary>
    /// Decodes a DIV r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Div;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For DIV r/m8 (0xF6 /6):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM8();
        
        // Verify this is a DIV instruction
        // The reg field should be 6 (DIV), which maps to RegisterIndex8.DH in our enum
        if (reg != RegisterIndex8.DH)
        {
            return false;
        }

        // Set the structured operands
        // DIV has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
