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
    /// Maps the register index from the opcode to the RegisterIndex enum value expected by tests
    /// </summary>
    /// <param name="opcodeRegIndex">The register index from the opcode (0-7)</param>
    /// <returns>The corresponding RegisterIndex enum value</returns>
    private RegisterIndex MapOpcodeToRegisterIndex(int opcodeRegIndex)
    {
        // The mapping from opcode register index to RegisterIndex enum is:
        // 0 -> A (EAX)
        // 1 -> C (ECX)
        // 2 -> D (EDX)
        // 3 -> B (EBX)
        // 4 -> Sp (ESP)
        // 5 -> Bp (EBP)
        // 6 -> Si (ESI)
        // 7 -> Di (EDI)
        
        // This mapping is based on the x86 instruction encoding
        // but we need to map to the RegisterIndex enum values that the tests expect
        return opcodeRegIndex switch
        {
            0 => RegisterIndex.A,  // EAX
            1 => RegisterIndex.C,  // ECX
            2 => RegisterIndex.D,  // EDX
            3 => RegisterIndex.B,  // EBX
            4 => RegisterIndex.Sp, // ESP
            5 => RegisterIndex.Bp, // EBP
            6 => RegisterIndex.Si, // ESI
            7 => RegisterIndex.Di, // EDI
            _ => RegisterIndex.A   // Default case, should never happen
        };
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
        int opcodeRegIndex = opcode & 0x07;
        
        // Map the opcode register index to the RegisterIndex enum value
        RegisterIndex reg = MapOpcodeToRegisterIndex(opcodeRegIndex);
        
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