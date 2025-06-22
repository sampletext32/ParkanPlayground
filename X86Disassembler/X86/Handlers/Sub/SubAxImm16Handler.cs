using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB AX, imm16 instruction (0x2D with 0x66 prefix)
/// </summary>
public class SubAxImm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubAxImm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubAxImm16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x2D and we have a 0x66 prefix
        return opcode == 0x2D && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB AX, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        if (!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the immediate value (16-bit)
        var immediate = Decoder.ReadUInt16();
        
        // Create the destination register operand (AX)
        var destinationOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 16);
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(immediate, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}