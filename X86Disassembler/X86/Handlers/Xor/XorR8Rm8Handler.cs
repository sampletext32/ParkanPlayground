using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r8, r/m8 instruction (0x32)
/// </summary>
public class XorR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorR8Rm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x32;
    }

    /// <summary>
    /// Decodes a XOR r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;
        
        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM8();

        // Note: The operand size is already set to 8-bit by the ReadModRM8 method

        // Create the destination register operand using the 8-bit register type
        var destinationOperand = OperandFactory.CreateRegisterOperand8(reg);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
