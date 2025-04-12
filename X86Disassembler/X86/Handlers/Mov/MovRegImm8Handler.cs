namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r8, imm8 instruction (0xB0-0xB7)
/// </summary>
public class MovRegImm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegImm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRegImm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode >= 0xB0 && opcode <= 0xB7;
    }
    
    /// <summary>
    /// Decodes a MOV r8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 8);
        
        // Read the immediate value
        byte imm8 = Decoder.ReadByte();
        if (Decoder.GetPosition() > Length)
        {
            return false;
        }
        
        // Set the operands
        instruction.Operands = $"{regName}, 0x{imm8:X2}";
        
        return true;
    }
}
