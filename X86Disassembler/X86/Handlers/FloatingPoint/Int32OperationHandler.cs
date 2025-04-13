namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int32 (DA opcode)
/// </summary>
public class Int32OperationHandler : InstructionHandler
{
    // DA opcode - operations on int32
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
    /// Initializes a new instance of the Int32OperationHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Int32OperationHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xDA;
    }

    /// <summary>
    /// Decodes a floating-point instruction for int32 operations
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
            instruction.Operands = destOperand;
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == RegisterIndex.A) // FCMOVB
            {
                instruction.Mnemonic = "fcmovb";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.B) // FCMOVE
            {
                instruction.Mnemonic = "fcmove";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.C) // FCMOVBE
            {
                instruction.Mnemonic = "fcmovbe";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.D) // FCMOVU
            {
                instruction.Mnemonic = "fcmovu";
                instruction.Operands = $"st(0), st({(int)rm})";
            }
            else if (reg == RegisterIndex.Di && rm == RegisterIndex.B) // FUCOMPP
            {
                instruction.Mnemonic = "fucompp";
                instruction.Operands = "";
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