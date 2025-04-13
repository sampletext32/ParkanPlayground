namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int32 and miscellaneous operations (DB opcode)
/// </summary>
public class LoadStoreInt32Handler : InstructionHandler
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[(int)reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // fild, fist, fistp
            {
                // Keep the dword ptr prefix for integer operations
                instruction.Operands = destOperand;
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // fld, fstp (extended precision)
            {
                // Replace dword ptr with tword ptr for extended precision
                instruction.Operands = destOperand.Replace("dword ptr", "tword ptr");
            }
            else
            {
                instruction.Operands = destOperand;
            }
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == RegisterIndex.A) // FCMOVNB
            {
                instruction.Mnemonic = "fcmovnb";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.B) // FCMOVNE
            {
                instruction.Mnemonic = "fcmovne";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.C) // FCMOVNBE
            {
                instruction.Mnemonic = "fcmovnbe";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.D) // FCMOVNU
            {
                instruction.Mnemonic = "fcmovnu";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.Si)
            {
                if (rm == RegisterIndex.C) // FCLEX
                {
                    instruction.Mnemonic = "fclex";
                    instruction.Operands = "";
                }
                else if (rm == RegisterIndex.D) // FINIT
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
            else if (reg == RegisterIndex.Di) // FUCOMI
            {
                instruction.Mnemonic = "fucomi";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.Sp) // FCOMI
            {
                instruction.Mnemonic = "fcomi";
                instruction.Operands = $"st(0), st({(int)rm})";
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