namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m32, r32 instruction (0x89) and MOV r/m8, r8 instruction (0x88)
/// </summary>
public class MovMemRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovMemRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovMemRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x88 || opcode == 0x89;
    }
    
    /// <summary>
    /// Decodes a MOV r/m32, r32 or MOV r/m8, r8 instruction
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
