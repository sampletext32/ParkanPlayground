namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int16 and miscellaneous operations (DF opcode)
/// </summary>
public class LoadStoreInt16Handler : InstructionHandler
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Check for FNSTSW AX (DF E0)
        if (mod == 3 && reg == RegisterIndex.Bp && rm == RegisterIndex.A)
        {
            // This is handled by the FnstswHandler, so we should not handle it here
            return false;
        }

        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[(int)reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
            
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D || reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // fild, fist, fistp, fild, fistp
            {
                if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // 64-bit integer
                {
                    // Replace dword ptr with qword ptr for 64-bit integers
                    operand = operand.Replace("dword ptr", "qword ptr");
                    instruction.Operands = operand;
                }
                else // 16-bit integer
                {
                    // Replace dword ptr with word ptr for 16-bit integers
                    operand = operand.Replace("dword ptr", "word ptr");
                    instruction.Operands = operand;
                }
            }
            else if (reg == RegisterIndex.Si || reg == RegisterIndex.Sp) // fbld, fbstp
            {
                // Replace dword ptr with tbyte ptr for 80-bit packed BCD
                operand = operand.Replace("dword ptr", "tbyte ptr");
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
            if (reg == RegisterIndex.A) // FFREEP
            {
                instruction.Mnemonic = "ffreep";
                instruction.Operands = $"st({(int)rm})";
            }
            else if (reg == RegisterIndex.B && rm == RegisterIndex.A) // FXCH
            {
                instruction.Mnemonic = "fxch";
                instruction.Operands = "";
            }
            else if (reg == RegisterIndex.C && rm == RegisterIndex.A) // FSTP
            {
                instruction.Mnemonic = "fstp";
                instruction.Operands = "st(1)";
            }
            else if (reg == RegisterIndex.D && rm == RegisterIndex.A) // FSTP
            {
                instruction.Mnemonic = "fstp";
                instruction.Operands = "st(1)";
            }
            else if (reg == RegisterIndex.Si) // FNSTSW
            {
                // This should not happen as FNSTSW AX is handled by FnstswHandler
                instruction.Mnemonic = "??";
                instruction.Operands = "";
            }
            else if (reg == RegisterIndex.Di) // FUCOMIP
            {
                instruction.Mnemonic = "fucomip";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.Sp) // FCOMIP
            {
                instruction.Mnemonic = "fcomip";
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