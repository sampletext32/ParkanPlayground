namespace X86Disassembler.X86.Handlers.Inc;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for INC r32 instructions (0x40-0x47)
/// </summary>
public class IncRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IncRegHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public IncRegHandler(InstructionDecoder decoder)
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
        // INC EAX = 0x40, INC ECX = 0x41, ..., INC EDI = 0x47
        return opcode >= 0x40 && opcode <= 0x47;
    }

    /// <summary>
    /// Decodes an INC r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Calculate the register index (0 for EAX, 1 for ECX, etc.)
        RegisterIndex reg = (RegisterIndex)(byte)(opcode - 0x40);
        
        // Set the instruction type
        instruction.Type = InstructionType.Inc;
        
        // Create the register operand
        var regOperand = OperandFactory.CreateRegisterOperand(reg, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            regOperand
        ];
        
        return true;
    }
}
