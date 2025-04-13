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
        
        int position = Decoder.GetPosition();
        
        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();
        
        // Read immediate value
        if (position >= Length)
        {
            // Incomplete instruction
            if (mod == 3)
            {
                string rmRegName = ModRMDecoder.GetRegisterName(rm, 8);
                instruction.Operands = $"{rmRegName}, ??";
            }
            else
            {
                instruction.Operands = $"byte ptr {memOperand}, ??";
            }
            return true;
        }
        
        byte imm8 = Decoder.ReadByte();
        
        // Set operands
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, 8);
            instruction.Operands = $"{rmRegName}, 0x{imm8:X2}";
        }
        else
        {
            instruction.Operands = $"byte ptr {memOperand}, 0x{imm8:X2}";
        }
        
        return true;
    }
}
