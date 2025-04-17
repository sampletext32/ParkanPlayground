using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Shift;

/// <summary>
/// Handler for ROR r/m8, imm8 instruction (0xC0 /1)
/// </summary>
public class RorRm8ByImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RorRm8ByImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RorRm8ByImmHandler(InstructionDecoder decoder)
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
        // ROR r/m8, imm8 is encoded as 0xC0 /1
        if (opcode != 0xC0)
            return false;

        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the reg field of the ModR/M byte is 1 (ROR)
        var reg = ModRMDecoder.PeakModRMReg();
        return reg == 1; // 1 = ROR
    }

    /// <summary>
    /// Decodes a ROR r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Ror;

        // Read the ModR/M byte
        var (_, _, _, operand) = ModRMDecoder.ReadModRM8();

        // Check if we can read the immediate byte
        if (!Decoder.CanReadByte())
            return false;

        // Read the immediate byte (rotate count)
        byte imm8 = Decoder.ReadByte();

        // Create an immediate operand for the rotate count
        var immOperand = OperandFactory.CreateImmediateOperand(imm8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            operand,
            immOperand
        ];

        return true;
    }
}
