namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store float64 operations (DD opcode)
/// </summary>
public class LoadStoreFloat64Handler : InstructionHandler
{
    // DD opcode - load/store float64
    private static readonly string[] Mnemonics =
    [
        "fld",
        "??",
        "fst",
        "fstp",
        "frstor",
        "??",
        "fnsave",
        "fnstsw",
    ];

    /// <summary>
    /// Initializes a new instance of the LoadStoreFloat64Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LoadStoreFloat64Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xDD;
    }

    /// <summary>
    /// Decodes a floating-point instruction for load/store float64 operations
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM(true);// true for 64-bit operand

        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[(int)reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // fld, fst, fstp
            {
                instruction.Operands = destOperand;
            }
            else // frstor, fnsave, fnstsw
            {
                // Remove the qword ptr prefix for these operations
                instruction.Operands = destOperand.Replace("qword ptr ", "");
            }
        }
        else // Register operand (ST(i))
        {
            // Special handling for register-register operations
            if (reg == RegisterIndex.A) // FFREE
            {
                instruction.Mnemonic = "ffree";
                instruction.Operands = $"st({(int)rm})";
            }
            else if (reg == RegisterIndex.C) // FST
            {
                instruction.Mnemonic = "fst";
                instruction.Operands = $"st({(int)rm})";
            }
            else if (reg == RegisterIndex.D) // FSTP
            {
                instruction.Mnemonic = "fstp";
                instruction.Operands = $"st({(int)rm})";
            }
            else if (reg == RegisterIndex.Si) // FUCOM
            {
                instruction.Mnemonic = "fucom";
                instruction.Operands = $"st({(int)rm})";
            }
            else if (reg == RegisterIndex.Di) // FUCOMP
            {
                instruction.Mnemonic = "fucomp";
                instruction.Operands = $"st({(int)rm})";
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