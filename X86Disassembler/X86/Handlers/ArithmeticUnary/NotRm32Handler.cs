using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.ArithmeticUnary;

/// <summary>
/// Handler for NOT r/m32 instruction (0xF7 /2)
/// </summary>
public class NotRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the NotRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public NotRm32Handler(InstructionDecoder decoder)
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
        // This handler only handles opcode 0xF7
        if (opcode != 0xF7)
            return false;

        // Check if the reg field of the ModR/M byte is 2 (NOT)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 2; // 2 = NOT
    }

    /// <summary>
    /// Decodes a NOT r/m32 instruction
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
        // For NOT r/m32 (0xF7 /2):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM();

        // Verify this is a NOT instruction
        // The reg field should be 2 (NOT), which maps to RegisterIndex.D in our enum
        if (reg != RegisterIndex.D)
        {
            return false;
        }

        // Set the structured operands
        // NOT has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}