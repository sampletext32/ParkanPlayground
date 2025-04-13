namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRm32Imm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRm32Imm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xC7;
    }

    /// <summary>
    /// Decodes a MOV r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();
        
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        if (startPosition >= Length)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] { opcode };
            return true;
        }

        // Use ModRMDecoder to decode the ModR/M byte
        var (mod, reg, rm, rmOperand) = ModRMDecoder.ReadModRM(false);
        
        // MOV r/m32, imm32 only uses reg=0
        if (reg != 0)
        {
            instruction.Operands = "??";
            byte[] rawBytesReg = new byte[Decoder.GetPosition() - startPosition + 1]; // +1 for opcode
            rawBytesReg[0] = opcode;
            for (int i = 0; i < Decoder.GetPosition() - startPosition; i++)
            {
                if (startPosition + i < Length)
                {
                    rawBytesReg[i + 1] = CodeBuffer[startPosition + i];
                }
            }
            instruction.RawBytes = rawBytesReg;
            return true;
        }
        
        // Get the position after decoding the ModR/M byte
        int newPosition = Decoder.GetPosition();
        
        // Check if we have enough bytes for the immediate value (4 bytes)
        if (newPosition + 3 >= Length)
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
        
        // Read the immediate dword
        byte b0 = CodeBuffer[newPosition];
        byte b1 = CodeBuffer[newPosition + 1];
        byte b2 = CodeBuffer[newPosition + 2];
        byte b3 = CodeBuffer[newPosition + 3];
        uint imm32 = (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
        Decoder.SetPosition(newPosition + 4);

        // Set the operands
        instruction.Operands = $"{rmOperand}, 0x{imm32:X8}";
        
        // Set the raw bytes
        byte[] rawBytes = new byte[Decoder.GetPosition() - startPosition + 1]; // +1 for opcode
        rawBytes[0] = opcode;
        for (int i = 0; i < Decoder.GetPosition() - startPosition; i++)
        {
            if (startPosition + i < Length)
            {
                rawBytes[i + 1] = CodeBuffer[startPosition + i];
            }
        }
        instruction.RawBytes = rawBytes;

        return true;
    }
}
