using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r8, imm8 instruction (0xB0-0xB7)
/// </summary>
public class MovRegImm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegImm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovRegImm8Handler(InstructionDecoder decoder)
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
        return opcode >= 0xB0 && opcode <= 0xB7;
    }

    /// <summary>
    /// Decodes a MOV r8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mov;

        // Register is encoded in the low 3 bits of the opcode
        RegisterIndex reg = (RegisterIndex)(opcode & 0x07);

        // Read the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        byte imm8 = Decoder.ReadByte();

        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 8);
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}