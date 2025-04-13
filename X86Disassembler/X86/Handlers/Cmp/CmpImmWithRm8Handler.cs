namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m8, imm8 instruction (0x80 /7)
/// </summary>
public class CmpImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpImmWithRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x80)
            return false;

        // Check if the reg field of the ModR/M byte is 7 (CMP)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;

        byte modRM = CodeBuffer[position];
        byte reg = (byte) ((modRM & 0x38) >> 3);

        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();

        // Set the mnemonic
        instruction.Mnemonic = "cmp";

        if (startPosition >= Length)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] {opcode};
            return true;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // CMP r/m8, imm8 is encoded as 80 /7
        if (reg != RegisterIndex.Bp)
        {
            instruction.Operands = "??";
            return true;
        }

        // Get the position after decoding the ModR/M byte
        int newPosition = Decoder.GetPosition();

        // Check if we have enough bytes for the immediate value
        if (newPosition >= Length)
        {
            instruction.Operands = "??";
            byte[] rawBytesImm = new byte[newPosition - startPosition + 1]; // +1 for opcode
            rawBytesImm[0] = opcode;
            for (int i = 0; i < newPosition - startPosition; i++)
            {
                if (startPosition + i < Length)
                {
                    rawBytesImm[i + 1] = CodeBuffer[startPosition + i];
                }
            }

            instruction.RawBytes = rawBytesImm;
            return true;
        }

        // Read the immediate byte
        byte imm8 = Decoder.ReadByte();

        // Replace the size prefix with "byte ptr"
        string operand;
        if (destOperand.StartsWith("qword ptr "))
        {
            operand = destOperand.Replace("qword ptr ", "byte ptr ");
        }
        else if (destOperand.StartsWith("dword ptr "))
        {
            operand = destOperand.Replace("dword ptr ", "byte ptr ");
        }
        else if (mod != 3) // Memory operand without prefix
        {
            operand = $"byte ptr {destOperand}";
        }
        else // Register operand
        {
            operand = ModRMDecoder.GetRegisterName(rm, 8);
        }

        // Set the operands
        instruction.Operands = $"{operand}, 0x{imm8:X2}";

        // Set the raw bytes
        byte[] rawBytes = new byte[newPosition - startPosition + 2]; // +1 for opcode, +1 for immediate
        rawBytes[0] = opcode;
        for (int i = 0; i < newPosition - startPosition; i++)
        {
            if (startPosition + i < Length)
            {
                rawBytes[i + 1] = CodeBuffer[startPosition + i];
            }
        }

        rawBytes[rawBytes.Length - 1] = imm8;
        instruction.RawBytes = rawBytes;

        return true;
    }
}