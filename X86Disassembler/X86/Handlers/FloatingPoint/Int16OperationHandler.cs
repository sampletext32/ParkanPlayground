namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int16 (DE opcode)
/// </summary>
public class Int16OperationHandler : FloatingPointBaseHandler
{
    // DE opcode - operations on int16
    private static readonly string[] Mnemonics =
    [
        "fiadd",
        "fimul",
        "ficom",
        "ficomp",
        "fisub",
        "fisubr",
        "fidiv",
        "fidivr"
    ];

    /// <summary>
    /// Initializes a new instance of the Int16OperationHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Int16OperationHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xDE;
    }

    /// <summary>
    /// Decodes a floating-point instruction for int16 operations
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
            // Need to modify the default dword ptr to word ptr for 16-bit integers
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
            operand = operand.Replace("dword ptr", "word ptr");
            instruction.Operands = operand;
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == 0) // FADDP
            {
                instruction.Mnemonic = "faddp";
                instruction.Operands = $"st({rm}), st(0)";
            }
            else if (reg == 1) // FMULP
            {
                instruction.Mnemonic = "fmulp";
                instruction.Operands = $"st({rm}), st(0)";
            }
            else if (reg == 2 && rm == 1) // FCOMP
            {
                instruction.Mnemonic = "fcomp";
                instruction.Operands = "";
            }
            else if (reg == 3 && rm == 1) // FCOMPP
            {
                instruction.Mnemonic = "fcompp";
                instruction.Operands = "";
            }
            else if (reg == 4) // FSUBP
            {
                instruction.Mnemonic = "fsubp";
                instruction.Operands = $"st({rm}), st(0)";
            }
            else if (reg == 5) // FSUBRP
            {
                instruction.Mnemonic = "fsubrp";
                instruction.Operands = $"st({rm}), st(0)";
            }
            else if (reg == 6) // FDIVP
            {
                instruction.Mnemonic = "fdivp";
                instruction.Operands = $"st({rm}), st(0)";
            }
            else if (reg == 7) // FDIVRP
            {
                instruction.Mnemonic = "fdivrp";
                instruction.Operands = $"st({rm}), st(0)";
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