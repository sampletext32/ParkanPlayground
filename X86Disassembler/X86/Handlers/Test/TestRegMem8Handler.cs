namespace X86Disassembler.X86.Handlers.Test;

using Operands;

/// <summary>
/// Handler for TEST r/m8, r8 instruction (0x84)
/// </summary>
public class TestRegMem8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestRegMem8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TestRegMem8Handler(InstructionDecoder decoder)
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
        return opcode == 0x84;
    }

    /// <summary>
    /// Decodes a TEST r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Test;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM8();
        
        // Note: The operand size is already set to 8-bit by the ReadModRM8 method

        // Create the register operand for the reg field using the 8-bit register type
        var regOperand = OperandFactory.CreateRegisterOperand8(reg);
        
        // Set the structured operands based on addressing mode
        if (mod == 3) // Direct register addressing
        {
            // Create the register operand for the r/m field using the 8-bit register type
            var rmOperand = OperandFactory.CreateRegisterOperand8(rm);
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                rmOperand,
                regOperand
            ];
        }
        else // Memory addressing
        {
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                destOperand,
                regOperand
            ];
        }

        return true;
    }
}