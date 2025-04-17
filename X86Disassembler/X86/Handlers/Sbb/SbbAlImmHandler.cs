namespace X86Disassembler.X86.Handlers.Sbb;

using Operands;

/// <summary>
/// Handler for SBB AL, imm8 instruction (0x1C)
/// </summary>
public class SbbAlImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbAlImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SbbAlImmHandler(InstructionDecoder decoder)
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
        return opcode == 0x1C;
    }

    /// <summary>
    /// Decodes a SBB AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sbb;

        // Create the AL register operand
        var alOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8);

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();

        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm8, 8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            alOperand,
            immOperand
        ];

        return true;
    }
}
