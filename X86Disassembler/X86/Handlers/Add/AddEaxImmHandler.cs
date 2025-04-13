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
        // Set the mnemonic
        instruction.Mnemonic = "add";
        
        int position = Decoder.GetPosition();
        
        // Check if we have enough bytes for the immediate value
        if (position + 3 >= Length)
        {
            return false; // Not enough bytes for the immediate value
        }
        
        // Read the 32-bit immediate value
        uint imm32 = Decoder.ReadUInt32();
        
        // Format the immediate value
        string immStr = $"0x{imm32:X}";
        
        // Set the operands
        instruction.Operands = $"eax, {immStr}";
        
        return true;
    }
}
