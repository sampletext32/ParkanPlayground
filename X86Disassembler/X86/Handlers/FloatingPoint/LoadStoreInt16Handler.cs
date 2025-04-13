namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int16 and miscellaneous operations (DF opcode)
/// </summary>
public class LoadStoreInt16Handler : InstructionHandler
{
    // Memory operand mnemonics for DF opcode - load/store int16, misc
    private static readonly string[] MemoryMnemonics =
    [
        "fild",   // 0 - 16-bit integer
        "??",     // 1
        "fist",   // 2 - 16-bit integer
        "fistp",  // 3 - 16-bit integer
        "fbld",   // 4 - 80-bit packed BCD
        "fild",   // 5 - 64-bit integer
        "fbstp",  // 6 - 80-bit packed BCD
        "fistp"   // 7 - 64-bit integer
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (string Mnemonic, string Operands)> RegisterOperations = new()
    {
        // FFREEP ST(i)
        { (RegisterIndex.A, RegisterIndex.A), ("ffreep", "st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("ffreep", "st(1)") },
        { (RegisterIndex.A, RegisterIndex.D), ("ffreep", "st(2)") },
        { (RegisterIndex.A, RegisterIndex.B), ("ffreep", "st(3)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("ffreep", "st(4)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("ffreep", "st(5)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("ffreep", "st(6)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("ffreep", "st(7)") },
        
        // Special cases
        { (RegisterIndex.B, RegisterIndex.A), ("fxch", "") },
        { (RegisterIndex.C, RegisterIndex.A), ("fstp", "st(1)") },
        { (RegisterIndex.D, RegisterIndex.A), ("fstp", "st(1)") },
        
        // FUCOMIP ST(0), ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), ("fucomip", "st(0), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.C), ("fucomip", "st(0), st(1)") },
        { (RegisterIndex.Di, RegisterIndex.D), ("fucomip", "st(0), st(2)") },
        { (RegisterIndex.Di, RegisterIndex.B), ("fucomip", "st(0), st(3)") },
        { (RegisterIndex.Di, RegisterIndex.Sp), ("fucomip", "st(0), st(4)") },
        { (RegisterIndex.Di, RegisterIndex.Bp), ("fucomip", "st(0), st(5)") },
        { (RegisterIndex.Di, RegisterIndex.Si), ("fucomip", "st(0), st(6)") },
        { (RegisterIndex.Di, RegisterIndex.Di), ("fucomip", "st(0), st(7)") },
        
        // FCOMIP ST(0), ST(i)
        { (RegisterIndex.Sp, RegisterIndex.A), ("fcomip", "st(0), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.C), ("fcomip", "st(0), st(1)") },
        { (RegisterIndex.Sp, RegisterIndex.D), ("fcomip", "st(0), st(2)") },
        { (RegisterIndex.Sp, RegisterIndex.B), ("fcomip", "st(0), st(3)") },
        { (RegisterIndex.Sp, RegisterIndex.Sp), ("fcomip", "st(0), st(4)") },
        { (RegisterIndex.Sp, RegisterIndex.Bp), ("fcomip", "st(0), st(5)") },
        { (RegisterIndex.Sp, RegisterIndex.Si), ("fcomip", "st(0), st(6)") },
        { (RegisterIndex.Sp, RegisterIndex.Di), ("fcomip", "st(0), st(7)") }
    };

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
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();

        // Check for FNSTSW AX (DF E0)
        if (mod == 3 && reg == RegisterIndex.Bp && rm == RegisterIndex.A)
        {
            // This is handled by the FnstswHandler, so we should not handle it here
            return false;
        }

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the mnemonic based on the reg field
            instruction.Mnemonic = MemoryMnemonics[(int)reg];
            
            // Get the base operand without size prefix
            string baseOperand = memOperand.Replace("dword ptr ", "");
            
            // Apply the appropriate size prefix based on the operation
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // 16-bit integer operations
            {
                instruction.Operands = $"word ptr {baseOperand}";
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // 64-bit integer operations
            {
                instruction.Operands = $"qword ptr {baseOperand}";
            }
            else if (reg == RegisterIndex.Si || reg == RegisterIndex.Sp) // 80-bit packed BCD operations
            {
                instruction.Operands = $"tbyte ptr {baseOperand}";
            }
            else // Other operations
            {
                instruction.Operands = memOperand;
            }
        }
        else // Register operand (ST(i))
        {
            // Look up the register operation in our dictionary
            if (RegisterOperations.TryGetValue((reg, rm), out var operation))
            {
                instruction.Mnemonic = operation.Mnemonic;
                instruction.Operands = operation.Operands;
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