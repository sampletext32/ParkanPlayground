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
        byte reg = (byte)((modRM & 0x38) >> 3);
        
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
            instruction.RawBytes = new byte[] { opcode };
            return true;
        }

        // Read the ModR/M byte
        byte modRM = CodeBuffer[startPosition];
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);
        
        // CMP r/m8, imm8 is encoded as 80 /7
        if (reg != 7)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] { opcode, modRM };
            return true;
        }
        
        // Use ModRMDecoder to decode the ModR/M byte
        var (_, _, _, rmOperand) = ModRMDecoder.ReadModRM(false);
        
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
        byte imm8 = CodeBuffer[newPosition];
        Decoder.SetPosition(newPosition + 1);
        
        // Replace the size prefix with "byte ptr"
        string operand;
        if (rmOperand.StartsWith("qword ptr "))
        {
            operand = rmOperand.Replace("qword ptr ", "byte ptr ");
        }
        else if (rmOperand.StartsWith("dword ptr "))
        {
            operand = rmOperand.Replace("dword ptr ", "byte ptr ");
        }
        else if (mod != 3) // Memory operand without prefix
        {
            operand = $"byte ptr {rmOperand}";
        }
        else // Register operand
        {
            operand = GetRegister8(rm);
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
