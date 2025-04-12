namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, r/m32 instruction (0x8B) and MOV r8, r/m8 instruction (0x8A)
/// </summary>
public class MovRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegMemHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRegMemHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x8A || opcode == 0x8B;
    }
    
    /// <summary>
    /// Decodes a MOV r32, r/m32 or MOV r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Determine operand size (0 = 8-bit, 1 = 32-bit)
        bool operandSize32 = (opcode & 0x01) != 0;
        int operandSize = operandSize32 ? 32 : 8;
        
        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();
        
        // Get register name based on size
        string regName = ModRMDecoder.GetRegisterName(reg, operandSize);
        
        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, operandSize);
            instruction.Operands = $"{rmRegName}, {regName}";
        }
        else // Memory operand
        {
            instruction.Operands = $"{memOperand}, {regName}";
        }
        
        return true;
    }
}
