namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m8, r8 instruction (0x08)
/// </summary>
public class OrRm8R8Handler : InstructionHandler
{
    // 8-bit register names
    private static readonly string[] RegisterNames8 = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
    
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
        
        // Proceed with normal ModR/M decoding
        position++;
        Decoder.SetPosition(position);
        
        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6); // Top 2 bits
        byte reg = (byte)((modRM & 0x38) >> 3); // Middle 3 bits
        byte rm = (byte)(modRM & 0x07);         // Bottom 3 bits
        
        // The register operand is in the reg field (8-bit register)
        string regOperand = RegisterNames8[reg];
        
        // Handle the r/m operand based on mod field
        string rmOperand;
        
        if (mod == 3) // Register-to-register
        {
            // Direct register addressing
            rmOperand = RegisterNames8[rm];
        }
        else // Memory addressing
        {
            // Use ModRMDecoder for memory addressing, but we need to adjust for 8-bit operands
            var modRMDecoder = new ModRMDecoder(CodeBuffer, Decoder, Length);
            string memOperand = modRMDecoder.DecodeModRM(mod, rm, false); // false = not 64-bit
            
            // Replace "dword ptr" with "byte ptr" for 8-bit operands
            rmOperand = memOperand.Replace("dword ptr", "byte ptr");
        }
        
        // Set the operands (r/m8, r8 format)
        instruction.Operands = $"{rmOperand}, {regOperand}";
        
        return true;
    }
}
