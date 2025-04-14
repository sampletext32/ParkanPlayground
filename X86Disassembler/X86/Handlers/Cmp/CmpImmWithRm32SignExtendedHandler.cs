namespace X86Disassembler.X86.Handlers.Cmp;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for CMP r/m32, imm8 (sign-extended) instruction (0x83 /7)
/// </summary>
public class CmpImmWithRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm32SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmpImmWithRm32SignExtendedHandler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cmp;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value as a signed byte and sign-extend it
        sbyte imm8 = (sbyte) Decoder.ReadByte();
        
        // Create the immediate operand with sign extension
        var immOperand = OperandFactory.CreateImmediateOperand((uint)imm8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            immOperand
        ];

        return true;
    }
}