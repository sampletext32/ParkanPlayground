using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m32, imm8 (sign-extended) instruction (0x83 /5)
/// </summary>
public class SubImmFromRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm32SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubImmFromRm32SignExtendedHandler(InstructionDecoder decoder)
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
            return false;

        // Check if the reg field of the ModR/M byte is 5 (SUB)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Extract the fields from the ModR/M byte
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value as a signed byte and sign-extend it to 32 bits
        int imm32 = (sbyte) Decoder.ReadByte();
        
        // Create the source immediate operand with the sign-extended value
        var sourceOperand = OperandFactory.CreateImmediateOperand((uint)imm32, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}