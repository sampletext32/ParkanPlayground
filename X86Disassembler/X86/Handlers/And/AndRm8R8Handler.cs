namespace X86Disassembler.X86.Handlers.And;

using Operands;

/// <summary>
/// Handler for AND r/m8, r8 instruction (0x20)
/// </summary>
public class AndRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndRm8R8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndRm8R8Handler(InstructionDecoder decoder)
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
        return opcode == 0x20;
    }

    /// <summary>
    /// Decodes an AND r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM8();

        // Create the source register operand using the 8-bit register type
        var srcOperand = OperandFactory.CreateRegisterOperand8(reg);

        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            // Create a register operand for the r/m field using the 8-bit register type
            var rmOperand = OperandFactory.CreateRegisterOperand8(rm);
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                rmOperand,
                srcOperand
            ];
        }
        else // Memory operand
        {
            // Note: The operand size is already set to 8-bit by the ReadModRM8 method
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                destOperand,
                srcOperand
            ];
        }

        return true;
    }
}