using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m32, imm32 instruction (0x81 /0)
/// </summary>
public class AddImmToRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddImmToRm32Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 0 (ADD)
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = Decoder.PeakByte();
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 0; // 0 = ADD
    }

    /// <summary>
    /// Decodes an ADD r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Type = InstructionType.Add;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value in little-endian format
        var imm = Decoder.ReadUInt32();

        instruction.StructuredOperands = [
            destOperand, 
            OperandFactory.CreateImmediateOperand(imm, 32)
        ];

        return true;
    }
}