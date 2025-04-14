using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r32, r/m32 instruction (0x23)
/// </summary>
public class AndR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndR32Rm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndR32Rm32Handler(InstructionDecoder decoder)
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
        return opcode == 0x23;
    }

    /// <summary>
    /// Decodes an AND r32, r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For AND r32, r/m32 (0x23):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        var (mod, reg, rm, sourceOperand) = ModRMDecoder.ReadModRM();

        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}