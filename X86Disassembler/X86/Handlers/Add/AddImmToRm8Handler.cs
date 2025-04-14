namespace X86Disassembler.X86.Handlers.Add;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for ADD r/m8, imm8 instruction (0x80 /0)
/// </summary>
public class AddImmToRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddImmToRm8Handler(InstructionDecoder decoder)
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

        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 0; // 0 = ADD
    }

    /// <summary>
    /// Decodes an ADD r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type and mnemonic
        instruction.Type = InstructionType.Add;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Adjust the operand size to 8-bit
        destOperand.Size = 8;

        // Read the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        byte imm8 = Decoder.ReadByte();

        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            sourceOperand
        ];

        return true;
    }
}