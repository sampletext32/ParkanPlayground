namespace X86Disassembler.X86.Handlers.Jump;

/// <summary>
/// Handler for two-byte conditional jump instructions (0x0F 0x80-0x8F)
/// </summary>
public class TwoByteConditionalJumpHandler : InstructionHandler
{
    // Mnemonics for conditional jumps
    private static readonly string[] ConditionalJumpMnemonics =
    [
        "jo", "jno", "jb", "jnb", "jz", "jnz", "jbe", "jnbe",
        "js", "jns", "jp", "jnp", "jl", "jnl", "jle", "jnle"
    ];
    
    /// <summary>
    /// Initializes a new instance of the TwoByteConditionalJumpHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TwoByteConditionalJumpHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // Two-byte conditional jumps start with 0x0F
        if (opcode != 0x0F)
        {
            return false;
        }
        
        int position = Decoder.GetPosition();
        if (!Decoder.CanReadByte())
        {
            return false;
        }
            
        byte secondByte = CodeBuffer[position];
        // Second byte must be in the range 0x80-0x8F
        return secondByte >= 0x80 && secondByte <= 0x8F;
    }
    
    /// <summary>
    /// Decodes a two-byte conditional jump instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        // Check if we have enough bytes for the second byte
        if (!Decoder.CanReadByte())
        {   
            return false;
        }
        
        // Read the second byte of the opcode
        byte secondByte = Decoder.ReadByte();
        
        // Get the mnemonic from the table
        int index = secondByte - 0x80;
        instruction.Mnemonic = ConditionalJumpMnemonics[index];
        
        // Check if we have enough bytes for the offset
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read the relative offset (32-bit)
        uint offset = Decoder.ReadUInt32();
        
        // Calculate the target address
        // For two-byte conditional jumps, the instruction is 6 bytes: first opcode (1) + second opcode (1) + offset (4)
        uint targetAddress = (uint)(instruction.Address + 6 + offset);
        
        // Format the target address
        instruction.Operands = $"0x{targetAddress:X8}";
        
        return true;
    }
}
