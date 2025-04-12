namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on float64 (DC opcode)
/// </summary>
public class Float64OperationHandler : FloatingPointBaseHandler
{
    // DC opcode - operations on float64
    private static readonly string[] Mnemonics =
    [
        "fadd",
        "fmul",
        "fcom",
        "fcomp",
        "fsub",
        "fsubr",
        "fdiv",
        "fdivr"
    ];

    /// <summary>
    /// Initializes a new instance of the Float64OperationHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Float64OperationHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xDC;
    }
    
    /// <summary>
    /// Decodes a floating-point instruction for float64 operations
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);
        
        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[reg];
        
        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, true); // true for 64-bit operand
            instruction.Operands = operand;
        }
        else // Register operand (ST(i))
        {
            // For DC C0-DC FF, the operands are reversed: ST(i), ST(0)
            instruction.Operands = $"st({rm}), st(0)";
        }
        
        return true;
    }
}
