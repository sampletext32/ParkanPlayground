using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m8, imm8 instruction (0x80 /5)
/// </summary>
public class SubImmFromRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubImmFromRm8Handler(InstructionDecoder decoder)
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
        if (opcode != 0x80)
            return false;

        // Check if the reg field of the ModR/M byte is 5 (SUB)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        // Extract the fields from the ModR/M byte
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Ensure the destination operand has the correct size (8-bit)
        destinationOperand.Size = 8;

        // Read the immediate byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        byte imm8 = Decoder.ReadByte();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}