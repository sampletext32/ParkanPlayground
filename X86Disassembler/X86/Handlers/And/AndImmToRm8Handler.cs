namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r/m8, imm8 instruction (0x80 /4)
/// </summary>
public class AndImmToRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndImmToRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AndImmToRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x80)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        int position = Decoder.GetPosition();
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte to check the reg field (bits 5-3)
        byte modRM = CodeBuffer[position];
        int reg = (modRM >> 3) & 0x7;
        
        // reg = 4 means AND operation
        return reg == 4;
    }
    
    /// <summary>
    /// Decodes an AND r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "and";
        
        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();
        
        // Get the position after decoding the ModR/M byte
        int position = Decoder.GetPosition();
        
        // Check if we have enough bytes for the immediate value
        if (position >= Length)
        {
            return false; // Not enough bytes for the immediate value
        }
        
        // Read the immediate value
        byte imm8 = Decoder.ReadByte();
        
        // Format the destination operand based on addressing mode
        string destOperand;
        if (mod == 3) // Register addressing mode
        {
            // Get 8-bit register name
            destOperand = ModRMDecoder.GetRegisterName(rm, 8);
        }
        else // Memory addressing mode
        {
            // Add byte ptr prefix for memory operands
            destOperand = $"byte ptr {memOperand}";
        }
        
        // Format the immediate value
        string immStr = $"0x{imm8:X2}";
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
