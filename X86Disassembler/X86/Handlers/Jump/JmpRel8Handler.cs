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
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();
        
        // Set the mnemonic
        instruction.Mnemonic = "jmp";
        
        // Check if we have enough bytes for the offset
        if (startPosition >= Length)
        {
            // Not enough bytes for the offset
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] { opcode };
            return true;
        }
        
        // Read the relative offset
        sbyte offset = (sbyte)CodeBuffer[startPosition];
        
        // Advance the decoder position past the offset byte
        Decoder.SetPosition(startPosition + 1);
        
        // Calculate the target address
        // The target is relative to the next instruction (after the JMP instruction)
        uint targetAddress = (uint)(instruction.Address + offset + 2);
        
        // Set the operands
        instruction.Operands = $"0x{targetAddress:X8}";
        
        // Set the raw bytes
        instruction.RawBytes = new byte[] { opcode, (byte)offset };
        
        return true;
    }
}
