using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND EAX, imm32 instruction (0x25)
/// </summary>
public class AndEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndEaxImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndEaxImmHandler(InstructionDecoder decoder) 
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
        // AND EAX, imm32 is encoded as 0x25 without 0x66 prefix
        if (opcode != 0x25)
        {
            return false;
        }

        // Only handle when the operand size prefix is NOT present
        return !Decoder.HasOperandSizePrefix();
    }
    
    /// <summary>
    /// Decodes an AND EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        // Create the destination register operand (EAX)
        var destinationOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32);

        // Read immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read immediate value
        uint imm32 = Decoder.ReadUInt32();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm32, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
