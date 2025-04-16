using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r32, r/m32 instruction (0x03)
/// </summary>
public class AddR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddR32Rm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddR32Rm32Handler(InstructionDecoder decoder)
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
        return opcode == 0x03;
    }

    /// <summary>
    /// Decodes an ADD r32, r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For ADD r32, r/m32 (0x03):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        // The sourceOperand is already created by ModRMDecoder based on mod and rm fields
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM();

        // Create the destination register operand from the reg field
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg);

        // Set the structured operands
        instruction.StructuredOperands =
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}