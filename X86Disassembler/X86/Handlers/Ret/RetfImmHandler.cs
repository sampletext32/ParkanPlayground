using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Ret;

/// <summary>
/// Handler for RETF imm16 instruction (0xCA)
/// </summary>
public class RetfImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetfImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RetfImmHandler(InstructionDecoder decoder)
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
        // RETF imm16 is encoded as 0xCA
        // Only handle when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0xCA && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a RETF imm16 instruction
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
