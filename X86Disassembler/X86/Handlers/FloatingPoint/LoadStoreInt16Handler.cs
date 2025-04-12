namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int16 and miscellaneous operations (DF opcode)
/// </summary>
public class LoadStoreInt16Handler : FloatingPointBaseHandler
{
    // DF opcode - load/store int16, misc
    private static readonly string[] Mnemonics =
    [
        "fild",
        "??",
        "fist",
        "fistp",
        "fbld",
        "fild",
        "fbstp",
        "fistp"
    ];

    /// <summary>
    /// Initializes a new instance of the LoadStoreInt16Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LoadStoreInt16Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xDF;
    }

    /// <summary>
    /// Decodes a floating-point instruction for load/store int16 and miscellaneous operations
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
        byte mod = (byte) ((modRM & 0xC0) >> 6);
        byte reg = (byte) ((modRM & 0x38) >> 3);
        byte rm = (byte) (modRM & 0x07);

        // Check for FNSTSW AX (DF E0)
        if (mod == 3 && reg == 7 && rm == 0)
        {
            // This is handled by the FnstswHandler, so we should not handle it here
            return false;
        }

        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);

            if (reg == 0 || reg == 2 || reg == 3 || reg == 5 || reg == 7) // fild, fist, fistp, fild, fistp
            {
                if (reg == 5 || reg == 7) // 64-bit integer
                {
                    instruction.Operands = $"qword ptr {operand}";
                }
                else // 16-bit integer
                {
                    instruction.Operands = $"word ptr {operand}";
                }
            }
            else if (reg == 4 || reg == 6) // fbld, fbstp
            {
                instruction.Operands = $"tbyte ptr {operand}";
            }
            else
            {
                instruction.Operands = operand;
            }
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == 0) // FFREEP
            {
                instruction.Mnemonic = "ffreep";
                instruction.Operands = $"st({rm})";
            }
            else if (reg == 1 && rm == 0) // FXCH
            {
                instruction.Mnemonic = "fxch";
                instruction.Operands = "";
            }
            else if (reg == 2 && rm == 0) // FSTP
            {
                instruction.Mnemonic = "fstp";
                instruction.Operands = "st(1)";
            }
            else if (reg == 3 && rm == 0) // FSTP
            {
                instruction.Mnemonic = "fstp";
                instruction.Operands = "st(1)";
            }
            else if (reg == 4) // FNSTSW
            {
                // This should not happen as FNSTSW AX is handled by FnstswHandler
                instruction.Mnemonic = "??";
                instruction.Operands = "";
            }
            else if (reg == 5) // FUCOMIP
            {
                instruction.Mnemonic = "fucomip";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 6) // FCOMIP
            {
                instruction.Mnemonic = "fcomip";
                instruction.Operands = $"st(0), st({rm})";
            }
            else
            {
                // Unknown instruction
                instruction.Mnemonic = "??";
                instruction.Operands = "";
            }
        }

        return true;
    }
}