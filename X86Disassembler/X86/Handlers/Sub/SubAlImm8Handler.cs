using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB AL, imm8 instruction (0x2C)
/// </summary>
public class SubAlImm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubAlImm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubAlImm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x2C;
    }

    /// <summary>
    /// Decodes a SUB AL, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate byte
        byte imm8 = Decoder.ReadByte();

        // Set the instruction type
        instruction.Type = InstructionType.Sub;
        
        // Create the destination register operand (AL)
        var destinationOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);
        
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