using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.ArithmeticUnary;

/// <summary>
/// Handler for IDIV r/m32 instruction (0xF7 /7)
/// </summary>
public class IdivRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IdivRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public IdivRm32Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 7 (IDIV)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = IDIV
    }

    /// <summary>
    /// Decodes an IDIV r/m32 instruction
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
        // For IDIV r/m32 (0xF7 /7):
        // - The r/m field with mod specifies the operand (register or memory)
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM();
        
        // Verify this is an IDIV instruction
        // The reg field should be 7 (IDIV)
        if (reg != RegisterIndex.Di)
        {
            return false;
        }

        // Set the structured operands
        // IDIV has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}