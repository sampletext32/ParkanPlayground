using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD EAX, imm32 instruction (0x05)
/// </summary>
public class AddEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddEaxImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddEaxImmHandler(InstructionDecoder decoder)
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
        // ADD EAX, imm32 is encoded as 0x05 without 0x66 prefix
        if (opcode != 0x05)
        {
            return false;
        }

        // Only handle when the operand size prefix is NOT present
        return !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes an ADD EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        instruction.Type = InstructionType.Add;

        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the 32-bit immediate value
        uint imm32 = Decoder.ReadUInt32();

        instruction.StructuredOperands =
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A),
            OperandFactory.CreateImmediateOperand(imm32)
        ];

        return true;
    }
}