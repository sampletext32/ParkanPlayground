namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD EAX, imm32 instruction (0x05)
/// </summary>
public class AddEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddEaxImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AddEaxImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x05;
    }
    
    /// <summary>
    /// Decodes an ADD EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();
        
        // Set the mnemonic
        instruction.Mnemonic = "add";
        
        // Check if we have enough bytes for the immediate value
        if (startPosition + 4 > Length)
        {
            // Not enough bytes for the immediate value
            instruction.Operands = "eax, ??";
            
            // Set the raw bytes to just the opcode
            instruction.RawBytes = new byte[] { opcode };
            
            return true; // Still return true as we've set a valid mnemonic and operands
        }
        
        // Check for special cases where the immediate value might be part of another instruction
        // For example, if the next byte is 0x83 (Group 1 sign-extended immediate)
        // or 0xEB (JMP rel8), it's likely the start of a new instruction
        byte nextByte = CodeBuffer[startPosition];
        if (nextByte == 0x83 || nextByte == 0xEB)
        {
            // This is likely the start of a new instruction, not part of our immediate value
            instruction.Operands = "eax, ??";
            
            // Set the raw bytes to just the opcode
            instruction.RawBytes = new byte[] { opcode };
            
            return true;
        }
        
        // Read the 32-bit immediate value
        uint imm32 = 0;
        for (int i = 0; i < 4; i++)
        {
            if (startPosition + i < Length)
            {
                imm32 |= (uint)(CodeBuffer[startPosition + i] << (i * 8));
            }
        }
        
        // Advance the decoder position
        Decoder.SetPosition(startPosition + 4);
        
        // Set the operands
        instruction.Operands = $"eax, 0x{imm32:X8}";
        
        // Set the raw bytes
        byte[] rawBytes = new byte[5]; // opcode + 4 bytes for immediate
        rawBytes[0] = opcode;
        for (int i = 0; i < 4; i++)
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
