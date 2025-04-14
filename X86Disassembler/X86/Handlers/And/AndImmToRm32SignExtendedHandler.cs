using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r/m32, imm8 (sign-extended) instruction (0x83 /4)
/// </summary>
public class AndImmToRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndImmToRm32SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndImmToRm32SignExtendedHandler(InstructionDecoder decoder)
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
        if (opcode != 0x83)
        {
            return false;
        }

        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte to check the reg field (bits 5-3)
        byte modRM = Decoder.PeakByte();
        int reg = (modRM >> 3) & 0x7;

        // reg = 4 means AND operation
        return reg == 4;
    }

    /// <summary>
    /// Decodes an AND r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        // Read the ModR/M byte
        // For AND r/m32, imm8 (sign-extended) (0x83 /4):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand (sign-extended from 8 to 32 bits)
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();

        if (!Decoder.CanReadByte())
        {
            return false; // Not enough bytes for the immediate value
        }

        // Read the immediate value as a signed byte and automatically sign-extend it to int
        int imm = (sbyte)Decoder.ReadByte();

        // Create the source immediate operand with the sign-extended value
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}