using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r8, r/m8 instruction (opcode 02)
/// </summary>
public class AddR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddR8Rm8Handler(InstructionDecoder decoder)
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
        // ADD r8, r/m8 is encoded as 02 /r
        return opcode == 0x02;
    }

    /// <summary>
    /// Decodes an ADD r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For ADD r8, r/m8 (02 /r):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM8();

        // Create the destination register operand using the 8-bit register type
        var destinationOperand = OperandFactory.CreateRegisterOperand8(reg);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
