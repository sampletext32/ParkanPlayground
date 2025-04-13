namespace X86Disassembler.X86.Handlers.Jump;

/// <summary>
/// Handler for JMP rel32 instruction (0xE9)
/// </summary>
public class JmpRel32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the JmpRel32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public JmpRel32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xE9;
    }

    /// <summary>
    /// Decodes a JMP rel32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "jmp";
        
        // Check if we have enough bytes for the offset (4 bytes)
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read the offset and calculate target address
        uint offset = Decoder.ReadUInt32();
        
        // Calculate target address (instruction address + instruction length + offset)
        // For JMP rel32, the instruction is 5 bytes: opcode (1 byte) + offset (4 bytes)
        uint targetAddress = (uint)(instruction.Address + 5 + offset);
        
        // Set the operands
        instruction.Operands = $"0x{targetAddress:X8}";
        
        return true;
    }
}
