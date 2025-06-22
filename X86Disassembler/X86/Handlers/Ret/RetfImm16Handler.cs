using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Ret;

/// <summary>
/// Handler for RETF imm16 instruction with operand size prefix (0xCA with 0x66 prefix)
/// </summary>
public class RetfImm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetfImm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RetfImm16Handler(InstructionDecoder decoder)
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
        // RETF imm16 with operand size prefix is encoded as 0xCA with 0x66 prefix
        // Only handle when the operand size prefix IS present
        return opcode == 0xCA && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a RETF imm16 instruction with operand size prefix
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Retf;

        // Check if we can read the immediate word
        if (!Decoder.CanReadUShort())
            return false;

        // Read the immediate word (number of bytes to pop from stack)
        ushort imm16 = Decoder.ReadUInt16();

        // Create an immediate operand for the pop count
        var immOperand = OperandFactory.CreateImmediateOperand(imm16, 16);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            immOperand
        ];

        return true;
    }
}
