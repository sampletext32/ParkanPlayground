namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m8, r8 instruction (0x08)
/// </summary>
public class OrRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrRm8R8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrRm8R8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x08;
    }
    
    /// <summary>
    /// Decodes an OR r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "or";
        
        // Read the ModR/M byte
        int position = Decoder.GetPosition();
        if (position >= Length)
        {
            instruction.Operands = "??";
            return true;
        }
        
        byte modRM = CodeBuffer[position];
        
        // Check if the next byte is a valid ModR/M byte or potentially another opcode
        // For the specific case of 0x83, it's a different instruction (ADD r/m32, imm8)
        if (modRM == 0x83)
        {
            // This is likely the start of another instruction, not a ModR/M byte
            instruction.Operands = "??";
            return true;
        }
        
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // The register operand is in the reg field (8-bit register)
        string regOperand = ModRMDecoder.GetRegisterName(reg, 8);
        
        // Handle the r/m operand based on mod field
        string rmOperand;
        
        if (mod == 3) // Register-to-register
        {
            // Direct register addressing
            rmOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }
        else // Memory addressing
        {
            // Replace "dword ptr" with "byte ptr" for 8-bit operands
            rmOperand = destOperand.Replace("dword ptr", "byte ptr");
        }
        
        // Set the operands (r/m8, r8 format)
        instruction.Operands = $"{rmOperand}, {regOperand}";
        
        return true;
    }
}
