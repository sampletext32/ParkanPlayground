namespace X86Disassembler.X86.Handlers.Or;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for OR r32, r/m32 instruction (0x0B)
/// </summary>
public class OrR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrR32Rm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public OrR32Rm32Handler(InstructionDecoder decoder)
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
        return opcode == 0x0B;
    }

    /// <summary>
    /// Decodes an OR r32, r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Or;
        
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
                regOperand,
                rmOperand
            ];
        }
        else // Memory addressing
        {
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                regOperand,
                destOperand
            ];
        }

        return true;
    }
}
