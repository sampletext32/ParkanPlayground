namespace X86Disassembler.X86.Handlers.And;

/// <summary>
/// Handler for AND r/m32, imm8 (sign-extended) instruction (0x83 /4)
/// </summary>
public class AndImmToRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndImmToRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AndImmToRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x83)
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
    /// Decodes an AND r/m32, imm8 (sign-extended) instruction
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
        
        // Read the immediate value as a signed byte and automatically sign-extend it to int
        int signExtendedImm = (sbyte)Decoder.ReadByte();
        
        // Format the destination operand based on addressing mode
        string destOperand;
        if (mod == 3) // Register addressing mode
        {
            // Get 32-bit register name
            destOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }
        else // Memory addressing mode
        {
            // Memory operand already includes dword ptr prefix
            destOperand = memOperand;
        }
        
        // Format the immediate value
        string immStr;
        if (signExtendedImm < 0)
        {
            // For negative values, use the full 32-bit representation
            immStr = $"0x{(uint)signExtendedImm:X8}";
        }
        else
        {
            // For positive values, use the regular format with leading zeros
            immStr = $"0x{signExtendedImm:X8}";
        }
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        return true;
    }
}
