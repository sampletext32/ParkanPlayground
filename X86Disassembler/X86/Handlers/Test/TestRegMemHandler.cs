namespace X86Disassembler.X86.Handlers.Test;

using Operands;

/// <summary>
/// Handler for TEST r/m32, r32 instruction (0x85)
/// </summary>
public class TestRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestRegMemHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TestRegMemHandler(InstructionDecoder decoder)
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
        return opcode == 0x85;
    }

    /// <summary>
    /// Decodes a TEST r/m32, r32 instruction
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

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Create the register operand for the reg field
        var regOperand = OperandFactory.CreateRegisterOperand(reg);
        
        // Set the structured operands based on addressing mode
        if (mod == 3) // Direct register addressing
        {
            // Create the register operand for the r/m field
            var rmOperand = OperandFactory.CreateRegisterOperand(rm);
            
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