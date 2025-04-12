namespace X86Disassembler.X86.Handlers.Push;

/// <summary>
/// Handler for PUSH imm32 instruction (0x68)
/// </summary>
public class PushImm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushImm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public PushImm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x68;
    }
    
    /// <summary>
    /// Decodes a PUSH imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "push";
        
        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();
        if (Decoder.GetPosition() > Length)
        {
            return false;
        }
        
        // Set the operands
        instruction.Operands = $"0x{imm32:X}";
        
        return true;
    }
}
