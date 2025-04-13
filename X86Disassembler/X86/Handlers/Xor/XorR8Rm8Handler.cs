namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r8, r/m8 instruction (0x32)
/// </summary>
public class XorR8Rm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorR8Rm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorR8Rm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x32;
    }

    /// <summary>
    /// Decodes a XOR r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Get register name (8-bit)
        string regName = ModRMDecoder.GetRegisterName(reg, 8);

        // If mod == 3, then the r/m field specifies a register
        if (mod == 3)
        {
            // Get the r/m register name (8-bit)
            string rmRegName = ModRMDecoder.GetRegisterName(rm, 8);
            
            // Set the operands
            instruction.Operands = $"{regName}, {rmRegName}";
            return true;
        }
        
        // Replace "dword ptr" with "byte ptr" to indicate 8-bit operation
        string byteOperand = destOperand.Replace("dword ptr", "byte ptr");
        
        // Set the operands
        instruction.Operands = $"{regName}, {byteOperand}";

        return true;
    }
}
