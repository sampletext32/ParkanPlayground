using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m16, r16 instruction (0x29 with 0x66 prefix)
/// </summary>
public class SubRm16R16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubRm16R16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubRm16R16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x29 and we have a 0x66 prefix
        return opcode == 0x29 && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB r/m16, r16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte, specifying that we're dealing with 16-bit operands
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM16();
        
        // Note: The operand size is already set to 16-bit by the ReadModRM16 method
        
        // Create the source register operand (16-bit)
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}