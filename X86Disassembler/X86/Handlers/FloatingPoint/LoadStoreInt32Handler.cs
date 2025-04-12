namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int32 and miscellaneous operations (DB opcode)
/// </summary>
public class LoadStoreInt32Handler : FloatingPointBaseHandler
{
    // DB opcode - load/store int32, misc
    private static readonly string[] Mnemonics =
    [
        "fild",
        "??",
        "fist",
        "fistp",
        "??",
        "fld",
        "??",
        "fstp",
    ];

    /// <summary>
    /// Initializes a new instance of the LoadStoreInt32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LoadStoreInt32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xDB;
    }

    /// <summary>
    /// Decodes a floating-point instruction for load/store int32 and miscellaneous operations
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

        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
            
            if (reg == 0 || reg == 2 || reg == 3) // fild, fist, fistp
            {
                // Keep the dword ptr prefix for integer operations
                instruction.Operands = operand;
            }
            else if (reg == 5 || reg == 7) // fld, fstp (extended precision)
            {
                // Replace dword ptr with tword ptr for extended precision
                operand = operand.Replace("dword ptr", "tword ptr");
                instruction.Operands = operand;
            }
            else
            {
                instruction.Operands = operand;
            }
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == 0) // FCMOVNB
            {
                instruction.Mnemonic = "fcmovnb";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 1) // FCMOVNE
            {
                instruction.Mnemonic = "fcmovne";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 2) // FCMOVNBE
            {
                instruction.Mnemonic = "fcmovnbe";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 3) // FCMOVNU
            {
                instruction.Mnemonic = "fcmovnu";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 4)
            {
                if (rm == 2) // FCLEX
                {
                    instruction.Mnemonic = "fclex";
                    instruction.Operands = "";
                }
                else if (rm == 3) // FINIT
                {
                    instruction.Mnemonic = "finit";
                    instruction.Operands = "";
                }
                else
                {
                    instruction.Mnemonic = "??";
                    instruction.Operands = "";
                }
            }
            else if (reg == 5) // FUCOMI
            {
                instruction.Mnemonic = "fucomi";
                instruction.Operands = $"st(0), st({rm})";
            }
            else if (reg == 6) // FCOMI
            {
                instruction.Mnemonic = "fcomi";
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