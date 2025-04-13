namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRm32Imm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRm32Imm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xC7;
    }

    /// <summary>
    /// Decodes a MOV r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "mov";
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Use ModRMDecoder to decode the ModR/M byte
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM(false);
        
        // MOV r/m32, imm32 only uses reg=0
        if (reg != 0)
        {
            return false;
        }
        
        // Check if we have enough bytes for the immediate value (4 bytes)
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read the immediate dword and format the operands
        uint imm32 = Decoder.ReadUInt32();
        instruction.Operands = $"{operand}, 0x{imm32:X8}";
        
        return true;
    }
}
