namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, imm32 instruction (0xB8-0xBF)
/// </summary>
public class MovRegImm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegImm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRegImm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode >= 0xB8 && opcode <= 0xBF;
    }
    
    /// <summary>
    /// Decodes a MOV r32, imm32 instruction
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
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();
        if (Decoder.GetPosition() > Length)
        {
            return false;
        }
        
        // Set the operands
        instruction.Operands = $"{regName}, 0x{imm32:X}";
        
        return true;
    }
}
