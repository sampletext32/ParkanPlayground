namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for multi-byte NOP instructions (0x0F 0x1F ...)
/// These are used for alignment and are encoded as NOP operations with specific memory operands
/// </summary>
public class MultiByteNopHandler : InstructionHandler
{
    // Dictionary mapping ModR/M byte to NOP variant information (memory operand and additional bytes to skip)
    private static readonly Dictionary<byte, (string MemOperand, int BytesToSkip, byte[] ExpectedBytes)> NopVariants = new()
    {
        // 3-byte NOP: 0F 1F 00
        { 0x00, ("[eax]", 0, Array.Empty<byte>()) },
        
        // 4-byte NOP: 0F 1F 40 00
        { 0x40, ("[eax]", 1, new byte[] { 0x00 }) },
        
        // 5-byte NOP: 0F 1F 44 00 00
        { 0x44, ("[eax+eax*1]", 2, new byte[] { 0x00, 0x00 }) },
        
        // 6-byte NOP: 0F 1F 44 00 00 00
        // Same ModR/M as 5-byte but with an extra 0x00 byte
        // Handled separately in the code
        
        // 7-byte NOP: 0F 1F 80 00 00 00 00
        { 0x80, ("[eax]", 4, new byte[] { 0x00, 0x00, 0x00, 0x00 }) },
        
        // 8-byte NOP: 0F 1F 84 00 00 00 00 00
        { 0x84, ("[eax+eax*1]", 5, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }) }
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
        
        // Read the ModR/M byte to identify the NOP variant
        int position = Decoder.GetPosition();
        byte modRm = CodeBuffer[position];
        Decoder.SetPosition(position + 1); // Skip the ModR/M byte
        
        // Default memory operand if no specific variant is matched
        string memOperand = "[eax]";
        
        // Check for the 6-byte NOP special case (0x44 with 3 zero bytes)
        if (modRm == 0x44 && position + 3 < Length && 
            CodeBuffer[position + 1] == 0x00 && 
            CodeBuffer[position + 2] == 0x00 && 
            CodeBuffer[position + 3] == 0x00)
        {
            memOperand = "[eax+eax*1]";
            Decoder.SetPosition(position + 4); // Skip the SIB, displacement, and extra byte
        }
        // Look up the NOP variant in the dictionary
        else if (NopVariants.TryGetValue(modRm, out var variant))
        {
            // Check if we have enough bytes and if they match the expected pattern
            bool isValidVariant = position + variant.BytesToSkip < Length;
            
            // Verify all expected bytes match
            for (int i = 0; i < variant.ExpectedBytes.Length && isValidVariant; i++)
            {
                isValidVariant = isValidVariant && CodeBuffer[position + 1 + i] == variant.ExpectedBytes[i];
            }
            
            if (isValidVariant)
            {
                memOperand = variant.MemOperand;
                Decoder.SetPosition(position + variant.BytesToSkip + 1); // +1 for ModR/M byte already skipped
            }
        }
        
        // Set the operands with the appropriate size prefix
        instruction.Operands = $"{ptrType} {memOperand}";
        
        return true;
    }
}