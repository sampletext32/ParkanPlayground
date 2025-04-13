namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, r32 instruction (0x39)
/// </summary>
public class CmpRm32R32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpRm32R32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpRm32R32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x39;
    }
    
    /// <summary>
    /// Decodes a CMP r/m32, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "cmp";
        
        // Save the original position to properly handle the ModR/M byte
        int originalPosition = Decoder.GetPosition();
        
        if (originalPosition >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Get the register name for the reg field
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Use the destOperand directly from ModRMDecoder
        string rmOperand = destOperand;
        
        // If it's a direct register operand, we need to remove the size prefix
        if (mod == 3)
        {
            rmOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }
        else if (rmOperand.StartsWith("dword ptr "))
        {
            // Remove the "dword ptr " prefix as we'll handle the operands differently
            rmOperand = rmOperand.Substring(10);
        }
        
        // Set the operands
        instruction.Operands = $"{rmOperand}, {regName}";
        
        return true;
    }
}
