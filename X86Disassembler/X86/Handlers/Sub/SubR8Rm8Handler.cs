using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r8, r/m8 instruction (0x2A)
/// </summary>
public class SubR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubR8Rm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x2A;
    }

    /// <summary>
    /// Decodes a SUB r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;
        
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM8();
        
        // Ensure the source operand has the correct size (8-bit)
        sourceOperand.Size = 8;
        
        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}