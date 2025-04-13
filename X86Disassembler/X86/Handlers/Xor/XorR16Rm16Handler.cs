namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r16, r/m16 instruction (0x33 with 0x66 prefix)
/// </summary>
public class XorR16Rm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorR16Rm16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public XorR16Rm16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x33 and there's an operand size prefix (0x66)
        return opcode == 0x33 && Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a XOR r16, r/m16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "xor";

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();

        // Get register name for the first operand (16-bit)
        string regName = ModRMDecoder.GetRegisterName(reg, 16);
        
        // For the second operand, handle based on addressing mode
        string rmOperand;
        if (mod == 3) // Register addressing mode
        {
            // Get 16-bit register name for the second operand
            rmOperand = ModRMDecoder.GetRegisterName(rm, 16);
        }
        else // Memory addressing mode
        {
            // For memory operands, replace "dword ptr" with "word ptr"
            if (memOperand.StartsWith("dword ptr "))
            {
                rmOperand = memOperand.Replace("dword ptr", "word ptr");
            }
            else
            {
                rmOperand = memOperand;
            }
        }

        // Set the operands
        instruction.Operands = $"{regName}, {rmOperand}";

        return true;
    }
}
