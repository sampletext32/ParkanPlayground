namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r8, r/m8 instruction (0x0A)
/// </summary>
public class OrR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrR8Rm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrR8Rm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x0A;
    }

    /// <summary>
    /// Decodes an OR r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);

        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);

        // Set the mnemonic
        instruction.Mnemonic = "or";

        // Get the register name
        string regName = GetRegister8(reg);

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
            // Replace dword ptr with byte ptr for 8-bit operations
            operand = operand.Replace("dword ptr", "byte ptr");
            instruction.Operands = $"{regName}, {operand}";
        }
        else // Register operand
        {
            string rmName = GetRegister8(rm);
            instruction.Operands = $"{regName}, {rmName}";
        }

        return true;
    }
}
