using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Imul;

/// <summary>
/// Handler for IMUL r32, r/m32, imm8 instruction (0x6B /r ib)
/// </summary>
public class ImulR32Rm32Imm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ImulR32Rm32Imm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ImulR32Rm32Imm8Handler(InstructionDecoder decoder)
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
        // IMUL r32, r/m32, imm8: opcode 6B /r ib
        return opcode == 0x6B;
    }

    /// <summary>
    /// Decodes an IMUL r32, r/m32, imm8 instruction
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

        // Read imm8 and sign-extend to int32
        sbyte imm8 = (sbyte)Decoder.ReadByte();
        var immOperand = OperandFactory.CreateImmediateOperand((uint)imm8, 8); // 8-bit immediate, sign-extended

        instruction.StructuredOperands =
        [
            destOperand,
            operand,
            immOperand
        ];
        return true;
    }
}
