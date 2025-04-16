using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.ArithmeticUnary;

/// <summary>
/// Handler for NEG r/m32 instruction (0xF7 /3)
/// </summary>
public class NegRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NegRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public NegRm32Handler(InstructionDecoder decoder)
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
        if (opcode != 0xF7)
            return false;

        // Check if the reg field of the ModR/M byte is 3 (NEG)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 3; // 3 = NEG
    }

    /// <summary>
    /// Decodes a NEG r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Neg;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For NEG r/m32 (0xF7 /3):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM();
        
        // Verify this is a NEG instruction
        // The reg field should be 3 (NEG), which maps to RegisterIndex.B in our enum
        if (reg != RegisterIndex.B)
        {
            return false;
        }

        // Set the structured operands
        // NEG has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}