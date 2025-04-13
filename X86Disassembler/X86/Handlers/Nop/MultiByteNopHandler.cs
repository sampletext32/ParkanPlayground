namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for multi-byte NOP instructions (0x0F 0x1F ...)
/// These are used for alignment and are encoded as NOP operations with specific memory operands
/// </summary>
public class MultiByteNopHandler : InstructionHandler
{
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

        int position = Decoder.GetPosition();

        // Check if we have enough bytes to read the second opcode
        if (position >= Length)
        {
            return false;
        }

        // Check if the second byte is 0x1F (part of the multi-byte NOP encoding)
        byte secondByte = CodeBuffer[position];
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
        if (Decoder.GetPosition() >= Length)
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
        
        // Determine the operand based on the NOP variant
        string memOperand;
        
        // 3-byte NOP: 0F 1F 00
        if (modRm == 0x00)
        {
            memOperand = "[eax]";
        }
        // 4-byte NOP: 0F 1F 40 00
        else if (modRm == 0x40 && position + 1 < Length && CodeBuffer[position + 1] == 0x00)
        {
            memOperand = "[eax]";
            Decoder.SetPosition(position + 2); // Skip the displacement byte
        }
        // 5-byte NOP: 0F 1F 44 00 00
        else if (modRm == 0x44 && position + 2 < Length && 
                CodeBuffer[position + 1] == 0x00 && CodeBuffer[position + 2] == 0x00)
        {
            memOperand = "[eax+eax*1]";
            Decoder.SetPosition(position + 3); // Skip the SIB and displacement bytes
        }
        // 6-byte NOP: 0F 1F 44 00 00 00
        else if (modRm == 0x44 && position + 3 < Length && 
                CodeBuffer[position + 1] == 0x00 && CodeBuffer[position + 2] == 0x00 &&
                CodeBuffer[position + 3] == 0x00)
        {
            memOperand = "[eax+eax*1]";
            Decoder.SetPosition(position + 4); // Skip the SIB, displacement, and extra byte
        }
        // 7-byte NOP: 0F 1F 80 00 00 00 00
        else if (modRm == 0x80 && position + 4 < Length && 
                CodeBuffer[position + 1] == 0x00 && CodeBuffer[position + 2] == 0x00 && 
                CodeBuffer[position + 3] == 0x00 && CodeBuffer[position + 4] == 0x00)
        {
            memOperand = "[eax]";
            Decoder.SetPosition(position + 5); // Skip the displacement bytes
        }
        // 8-byte NOP: 0F 1F 84 00 00 00 00 00
        else if (modRm == 0x84 && position + 5 < Length && 
                CodeBuffer[position + 1] == 0x00 && CodeBuffer[position + 2] == 0x00 && 
                CodeBuffer[position + 3] == 0x00 && CodeBuffer[position + 4] == 0x00 && 
                CodeBuffer[position + 5] == 0x00)
        {
            memOperand = "[eax+eax*1]";
            Decoder.SetPosition(position + 6); // Skip the SIB and displacement bytes
        }
        // For any other variant, use a generic NOP operand
        else
        {
            memOperand = "[eax]";
        }
        
        // Set the operands with the appropriate size prefix
        instruction.Operands = $"{ptrType} {memOperand}";
        
        return true;
    }
}