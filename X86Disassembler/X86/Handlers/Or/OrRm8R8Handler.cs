using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m8, r8 instruction (0x08)
/// </summary>
public class OrRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrRm8R8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public OrRm8R8Handler(InstructionDecoder decoder) 
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
        return opcode == 0x08;
    }
    
    /// <summary>
    /// Decodes an OR r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Or;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For OR r/m8, r8 (0x08):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The reg field specifies the source register
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Adjust the operand size to 8-bit
        destinationOperand.Size = 8;
        
        // Create the source register operand
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
