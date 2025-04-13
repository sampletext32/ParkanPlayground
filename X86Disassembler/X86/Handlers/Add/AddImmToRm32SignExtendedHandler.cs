namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m32, imm8 (sign-extended) instruction (0x83 /0)
/// </summary>
public class AddImmToRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AddImmToRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
        : base(codeBuffer, decoder, length)
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

        // Check if the reg field of the ModR/M byte is 0 (ADD)
        int position = Decoder.GetPosition();
        if (!Decoder.CanReadByte())
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 0; // 0 = ADD
    }

    /// <summary>
    /// Decodes an ADD r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "add";

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value as a signed byte and automatically sign-extend it to int
        int imm = (sbyte) Decoder.ReadByte();

        // Format the immediate value
        string immStr;
        if (imm < 0)
        {
            // For negative values, use the full 32-bit representation (0xFFFFFFxx)
            immStr = $"0x{(uint) imm:X8}";
        }
        else
        {
            // For positive values, use the regular format with leading zeros
            immStr = $"0x{imm:X8}";
        }

        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";

        return true;
    }
}