using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m8, r8 instruction (0x28)
/// </summary>
public class SubRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubRm8R8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubRm8R8Handler(InstructionDecoder decoder)
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
        return opcode == 0x28;
    }

    /// <summary>
    /// Decodes a SUB r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;
        
        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM8();
        
        // Ensure the destination operand has the correct size (8-bit)
        destinationOperand.Size = 8;
        
        // Create the source register operand
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}