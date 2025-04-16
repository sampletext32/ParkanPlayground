using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP EAX, imm32 instruction (opcode 3D)
/// </summary>
public class CmpEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpEaxImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmpEaxImmHandler(InstructionDecoder decoder)
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
        // CMP EAX, imm32 is encoded as 3D
        return opcode == 0x3D;
    }

    /// <summary>
    /// Decodes a CMP EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cmp;

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();

        // Set the structured operands
        // CMP EAX, imm32 has two operands: EAX and the immediate value
        instruction.StructuredOperands = 
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A),
            OperandFactory.CreateImmediateOperand(imm32)
        ];

        return true;
    }
}
