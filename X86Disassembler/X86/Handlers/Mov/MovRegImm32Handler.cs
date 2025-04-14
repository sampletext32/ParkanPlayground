using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, imm32 instruction (0xB8-0xBF)
/// </summary>
public class MovRegImm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegImm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovRegImm32Handler(InstructionDecoder decoder)
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
        return opcode >= 0xB8 && opcode <= 0xBF;
    }

    /// <summary>
    /// Decodes a MOV r32, imm32 instruction
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
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        uint imm32 = Decoder.ReadUInt32();

        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 32);
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm32, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}