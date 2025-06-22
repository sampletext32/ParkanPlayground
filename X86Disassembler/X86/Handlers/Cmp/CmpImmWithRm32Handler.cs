using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, imm32 instruction (0x81 /7)
/// </summary>
public class CmpImmWithRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmpImmWithRm32Handler(InstructionDecoder decoder)
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
        if (opcode != 0x81)
            return false;

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cmp;

        // Read the ModR/M byte
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

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