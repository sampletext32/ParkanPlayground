namespace X86Disassembler.X86.Handlers.Xchg;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for XCHG EAX, r32 instruction (0x90-0x97)
/// </summary>
public class XchgEaxRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XchgEaxRegHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XchgEaxRegHandler(InstructionDecoder decoder)
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
        return opcode >= 0x91 && opcode <= 0x97;
    }

    /// <summary>
    /// Decodes an XCHG EAX, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xchg;

        // Register is encoded in the low 3 bits of the opcode
        RegisterIndex reg = (RegisterIndex) (opcode & 0x07);
        
        // Create the register operands
        var eaxOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A);
        var regOperand = OperandFactory.CreateRegisterOperand(reg);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            eaxOperand,
            regOperand
        ];

        return true;
    }
}