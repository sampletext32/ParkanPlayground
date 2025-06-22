using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r/m8, imm8 instruction (0x80 /4)
/// </summary>
public class AndImmToRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndImmToRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndImmToRm8Handler(InstructionDecoder decoder)
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
        {
            return false;
        }

        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte to check the reg field (bits 5-3)
        var reg = ModRMDecoder.PeakModRMReg();

        // reg = 4 means AND operation
        return reg == 4;
    }

    /// <summary>
    /// Decodes an AND r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        // Read the ModR/M byte, specifying that we're dealing with 8-bit operands
        // For AND r/m8, imm8 (0x80 /4):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM8();

        // Note: The operand size is already set to 8-bit by the ReadModRM8 method

        if (!Decoder.CanReadByte())
        {
            return false; // Not enough bytes for the immediate value
        }

        // Read the immediate value
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