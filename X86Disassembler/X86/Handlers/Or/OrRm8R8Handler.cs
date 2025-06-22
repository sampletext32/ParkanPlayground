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
        if (opcode != 0x08)
            return false;
            
        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
            return false;
        
        return true;
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

        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM8();
        
        // Note: The operand size is already set to 8-bit by the ReadModRM8 method
        
        // Create the source register operand using the 8-bit register type
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
