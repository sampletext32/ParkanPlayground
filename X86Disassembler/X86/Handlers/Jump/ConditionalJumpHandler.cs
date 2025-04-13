namespace X86Disassembler.X86.Handlers.Jump;

/// <summary>
/// Handler for conditional jump instructions (0x70-0x7F)
/// </summary>
public class ConditionalJumpHandler : InstructionHandler
{
    // Mnemonics for conditional jumps
    private static readonly string[] Mnemonics =
    [
        "jo", "jno", "jb", "jnb", "jz", "jnz", "jbe", "jnbe",
        "js", "jns", "jp", "jnp", "jl", "jnl", "jle", "jnle"
    ];
    
    /// <summary>
    /// Initializes a new instance of the ConditionalJumpHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public ConditionalJumpHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // Conditional jumps are in the range 0x70-0x7F
        return opcode >= 0x70 && opcode <= 0x7F;
    }
    
    /// <summary>
    /// Decodes a conditional jump instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Get the mnemonic from the table
        int index = opcode - 0x70;
        instruction.Mnemonic = Mnemonics[index];
        
        // Check if we can read the offset byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the offset and calculate target address
        int position = Decoder.GetPosition();
        sbyte offset = (sbyte)Decoder.ReadByte();
        int targetAddress = position + 1 + offset;
        
        // Format the target address as a hexadecimal value
        instruction.Operands = $"0x{targetAddress:X8}";
        
        return true;
    }
}
