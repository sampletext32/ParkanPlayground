namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST EAX, imm32 instruction (0xA9)
/// </summary>
public class TestEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestEaxImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestEaxImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xA9;
    }
    
    /// <summary>
    /// Decodes a TEST EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        int position = Decoder.GetPosition();
        
        if (position + 3 >= Length)
        {
            return false;
        }
        
        // Read the immediate value - x86 is little-endian, so we need to read the bytes in the correct order
        byte b0 = CodeBuffer[position];
        byte b1 = CodeBuffer[position + 1];
        byte b2 = CodeBuffer[position + 2];
        byte b3 = CodeBuffer[position + 3];
        
        // Combine the bytes to form a 32-bit immediate value
        uint imm32 = (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
        
        Decoder.SetPosition(position + 4);
        
        // Set the operands
        instruction.Operands = $"eax, 0x{imm32:X8}";
        
        return true;
    }
}
