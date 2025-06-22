using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r16, r/m16 instruction (opcode 03 with 0x66 prefix)
/// </summary>
public class AddR16Rm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddR16Rm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddR16Rm16Handler(InstructionDecoder decoder)
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
        // ADD r16, r/m16 is encoded as 0x03 with 0x66 prefix
        if (opcode != 0x03)
        {
            return false;
        }

        // Only handle when the operand size prefix is present
        return Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes an ADD r16, r/m16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // For ADD r16, r/m16 (0x03 with 0x66 prefix):
        // - The reg field of the ModR/M byte specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM16();

        // Note: The operand size is already set to 16-bit by the ReadModRM16 method

        // Create the destination register operand with 16-bit size
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 16);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
