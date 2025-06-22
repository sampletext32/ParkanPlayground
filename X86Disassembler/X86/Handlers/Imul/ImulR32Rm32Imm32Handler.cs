using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Imul;

/// <summary>
/// Handler for IMUL r32, r/m32, imm32 instruction (0x69 /r id)
/// </summary>
public class ImulR32Rm32Imm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ImulR32Rm32Imm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ImulR32Rm32Imm32Handler(InstructionDecoder decoder)
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
        // IMUL r32, r/m32, imm32: opcode 69 /r id
        return opcode == 0x69;
    }

    /// <summary>
    /// Decodes an IMUL r32, r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        instruction.Type = InstructionType.IMul;

        // Read ModR/M: reg = destination, r/m = source
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM();

        var destOperand = OperandFactory.CreateRegisterOperand(reg);

        // Read imm32 (4 bytes)
        uint imm32 = Decoder.ReadUInt32();
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);

        instruction.StructuredOperands =
        [
            destOperand,
            operand,
            immOperand
        ];
        return true;
    }
}
