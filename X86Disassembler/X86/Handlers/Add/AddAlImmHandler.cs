using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD AL, imm8 instruction (opcode 04)
/// </summary>
public class AddAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddAlImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddAlImmHandler(InstructionDecoder decoder)
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
        // ADD AL, imm8 is encoded as 04 ib
        return opcode == 0x04;
    }

    /// <summary>
    /// Decodes an ADD AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();

        // Create the destination register operand (AL)
        var destinationOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8);

        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
