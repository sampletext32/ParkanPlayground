namespace X86Disassembler.X86.Handlers.And;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for AND r8, r/m8 instruction (0x22)
/// </summary>
public class AndR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndR8Rm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x22;
    }

    /// <summary>
    /// Decodes an AND r8, r/m8 instruction
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

        // Read the ModR/M byte
        var (mod, reg, rm, srcOperand) = ModRMDecoder.ReadModRM();

        // Create the destination register operand
        var destOperand = OperandFactory.CreateRegisterOperand(reg, 8);

        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            // Create a register operand for the r/m field
            var rmOperand = OperandFactory.CreateRegisterOperand(rm, 8);
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                destOperand,
                rmOperand
            ];
        }
        else // Memory operand
        {
            // Ensure memory operand has the correct size (8-bit)
            if (srcOperand is MemoryOperand memOperand)
            {
                memOperand.Size = 8;
            }
            
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