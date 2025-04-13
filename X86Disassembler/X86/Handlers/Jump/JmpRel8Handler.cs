namespace X86Disassembler.X86.Handlers.Jump;

/// <summary>
/// Handler for JMP rel8 instruction (0xEB)
/// </summary>
public class JmpRel8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the JmpRel8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public JmpRel8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xEB;
    }
    
    /// <summary>
    /// Decodes a JMP rel8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "jmp";
        
        // Check if we can read the offset byte
        if (!Decoder.CanReadByte())
        {
            return true;
        }

        sbyte offset = (sbyte)Decoder.ReadByte();
        
        // Calculate target address (instruction address + instruction length + offset)
        ulong targetAddress = instruction.Address + 2UL + (uint)offset;
        
        // Format the target address
        instruction.Operands = $"0x{targetAddress:X8}";
        
        return true;
    }
}
