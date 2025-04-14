using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, r32 instruction (0x39)
/// </summary>
public class CmpRm32R32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpRm32R32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmpRm32R32Handler(InstructionDecoder decoder) 
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
        return opcode == 0x39;
    }
    
    /// <summary>
    /// Decodes a CMP r/m32, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cmp;

        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte
        // For CMP r/m32, r32 (0x39):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The reg field specifies the source register
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Create the source register operand
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
