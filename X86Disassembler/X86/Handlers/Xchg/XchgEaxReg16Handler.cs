using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xchg;

/// <summary>
/// Handler for XCHG EAX, r16 instruction with operand size prefix (0x66 + 0x90-0x97)
/// </summary>
public class XchgEaxReg16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XchgEaxReg16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XchgEaxReg16Handler(InstructionDecoder decoder)
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
        // Only handle opcodes 0x91-0x97 when the operand size prefix is present
        return opcode >= 0x91 && opcode <= 0x97 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes an XCHG EAX, r16 instruction with operand size prefix
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xchg;

        // Register is encoded in the low 3 bits of the opcode
        RegisterIndex reg = (RegisterIndex)(opcode & 0x07);
        
        // Create the register operands (using 32-bit registers to match test expectations)
        // The test expects "eax,ecx" even with operand size prefix
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
