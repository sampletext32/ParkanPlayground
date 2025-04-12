namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for RET instruction with immediate operand (0xC2)
/// </summary>
public class RetImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RetImmHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public RetImmHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xC2;
    }
    
    /// <summary>
    /// Decodes a RET instruction with immediate operand
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "ret";
        
        int position = Decoder.GetPosition();
        
        if (position + 2 > Length)
        {
            return false;
        }
        
        // Read the immediate value
        ushort imm16 = BitConverter.ToUInt16(CodeBuffer, position);
        Decoder.SetPosition(position + 2);
        
        // Set the operands
        instruction.Operands = $"0x{imm16:X4}";
        
        return true;
    }
}
