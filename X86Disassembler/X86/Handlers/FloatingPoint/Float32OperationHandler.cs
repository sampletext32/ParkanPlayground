namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on float32 (D8 opcode)
/// </summary>
public class Float32OperationHandler : InstructionHandler
{
    // D8 opcode - operations on float32
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
    /// Initializes a new instance of the Float32OperationHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Float32OperationHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xD8;
    }

    /// <summary>
    /// Decodes a floating-point instruction for float32 operations
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
            // For register operands, we need to handle the stack registers
            instruction.Operands = $"st(0), st({(int)rm})";
        }

        return true;
    }
}