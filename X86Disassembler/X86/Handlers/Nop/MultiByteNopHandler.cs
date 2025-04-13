namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for multi-byte NOP instructions (0x0F 0x1F ...)
/// These are used for alignment and are encoded as NOP operations with specific memory operands
/// </summary>
public class MultiByteNopHandler : InstructionHandler
{
    // NOP variant information (ModR/M byte, memory operand, and expected bytes pattern)
    private static readonly (byte ModRm, string MemOperand, byte[] ExpectedBytes)[] NopVariants =
    {
        // 8-byte NOP: 0F 1F 84 00 00 00 00 00 (check longest patterns first)
        (0x84, "[eax+eax*1]", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }),
        
        // 7-byte NOP: 0F 1F 80 00 00 00 00
        (0x80, "[eax]", new byte[] { 0x00, 0x00, 0x00, 0x00 }),
        
        // 6-byte NOP: 0F 1F 44 00 00 00
        (0x44, "[eax+eax*1]", new byte[] { 0x00, 0x00, 0x00 }),
        
        // 5-byte NOP: 0F 1F 44 00 00
        (0x44, "[eax+eax*1]", new byte[] { 0x00, 0x00 }),
        
        // 4-byte NOP: 0F 1F 40 00
        (0x40, "[eax]", new byte[] { 0x00 }),
        
        // 3-byte NOP: 0F 1F 00
        (0x00, "[eax]", Array.Empty<byte>())
    };

    /// <summary>
    /// Initializes a new instance of the MultiByteNopHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MultiByteNopHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Multi-byte NOPs start with 0x0F
        if (opcode != 0x0F)
        {
            return false;
        }

        // Check if we have enough bytes to read the second opcode
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the second byte is 0x1F (part of the multi-byte NOP encoding)
        byte secondByte = CodeBuffer[Decoder.GetPosition()];
        return secondByte == 0x1F;
    }

    /// <summary>
    /// Decodes a multi-byte NOP instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "nop";

        // Skip the second byte (0x1F)
        Decoder.ReadByte();

        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if we have an operand size prefix (0x66)
        bool hasOperandSizePrefix = Decoder.HasOperandSizeOverridePrefix();
        
        // Determine the size of the operand
        string ptrType = hasOperandSizePrefix ? "word ptr" : "dword ptr";
        
        // Read the ModR/M byte but don't advance the position yet
        int position = Decoder.GetPosition();
        byte modRm = CodeBuffer[position];
        
        // Default memory operand if no specific variant is matched
        string memOperand = "[eax]";
        int bytesToSkip = 1; // Skip at least the ModR/M byte
        
        // Try to find a matching NOP variant (we check longest patterns first)
        foreach (var (variantModRm, operand, expectedBytes) in NopVariants)
        {
            // Skip if ModR/M doesn't match
            if (variantModRm != modRm)
            {
                continue;
            }
            
            // Check if we have enough bytes for this pattern
            if (position + expectedBytes.Length >= Length)
            {
                continue;
            }
            
            // Check if the expected bytes match
            bool isMatch = true;
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                if (position + i + 1 >= Length || CodeBuffer[position + i + 1] != expectedBytes[i])
                {
                    isMatch = false;
                    break;
                }
            }
            
            // If we found a match, use it and stop checking
            if (isMatch)
            {
                memOperand = operand;
                bytesToSkip = 1 + expectedBytes.Length; // ModR/M byte + additional bytes
                break;
            }
        }
        
        // Skip the bytes we've processed
        Decoder.SetPosition(position + bytesToSkip);
        
        // Set the operands with the appropriate size prefix
        instruction.Operands = $"{ptrType} {memOperand}";
        
        return true;
    }
}