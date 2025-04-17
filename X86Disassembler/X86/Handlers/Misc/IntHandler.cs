namespace X86Disassembler.X86.Handlers.Misc;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for INT instruction (0xCD)
/// </summary>
public class IntHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IntHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public IntHandler(InstructionDecoder decoder)
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
        // INT is encoded as 0xCD
        return opcode == 0xCD;
    }

    /// <summary>
    /// Decodes an INT instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Int;

        // Check if we can read the immediate byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate byte (interrupt vector)
        byte imm8 = Decoder.ReadByte();

        // Create an immediate operand for the interrupt vector
        var operand = OperandFactory.CreateImmediateOperand(imm8);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
