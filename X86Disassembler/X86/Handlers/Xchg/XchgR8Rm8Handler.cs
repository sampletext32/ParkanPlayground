using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xchg;

/// <summary>
/// Handler for XCHG r8, r/m8 instruction (opcode 0x86)
/// Exchanges the contents of an 8-bit register with a register or memory location
/// </summary>
public class XchgR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XchgR8Rm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XchgR8Rm8Handler(InstructionDecoder decoder)
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
        // XCHG r8, r/m8 is 0x86
        return opcode == 0x86;
    }
    
    /// <summary>
    /// Decodes an XCHG r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xchg;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte
        // For XCHG r8, r/m8 (0x86):
        // - The reg field specifies the first register operand
        // - The r/m field with mod specifies the second operand (register or memory)
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM8();
        
        // Create the source register operand from the reg field
        var sourceOperand = OperandFactory.CreateRegisterOperand8(reg);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
