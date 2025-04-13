namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int32 and miscellaneous operations (DB opcode)
/// </summary>
public class LoadStoreInt32Handler : InstructionHandler
{
    // Memory operand mnemonics for DB opcode - load/store int32, misc
    private static readonly string[] MemoryMnemonics =
    [
        "fild",   // 0 - 32-bit integer
        "??",     // 1
        "fist",   // 2 - 32-bit integer
        "fistp",  // 3 - 32-bit integer
        "??",     // 4
        "fld",    // 5 - 80-bit extended precision
        "??",     // 6
        "fstp"    // 7 - 80-bit extended precision
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (string Mnemonic, string Operands)> RegisterOperations = new()
    {
        // FCMOVNB ST(0), ST(i)
        { (RegisterIndex.A, RegisterIndex.A), ("fcmovnb", "st(0), st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("fcmovnb", "st(0), st(1)") },
        { (RegisterIndex.A, RegisterIndex.D), ("fcmovnb", "st(0), st(2)") },
        { (RegisterIndex.A, RegisterIndex.B), ("fcmovnb", "st(0), st(3)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("fcmovnb", "st(0), st(4)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("fcmovnb", "st(0), st(5)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("fcmovnb", "st(0), st(6)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("fcmovnb", "st(0), st(7)") },
        
        // FCMOVNE ST(0), ST(i)
        { (RegisterIndex.B, RegisterIndex.A), ("fcmovne", "st(0), st(0)") },
        { (RegisterIndex.B, RegisterIndex.C), ("fcmovne", "st(0), st(1)") },
        { (RegisterIndex.B, RegisterIndex.D), ("fcmovne", "st(0), st(2)") },
        { (RegisterIndex.B, RegisterIndex.B), ("fcmovne", "st(0), st(3)") },
        { (RegisterIndex.B, RegisterIndex.Sp), ("fcmovne", "st(0), st(4)") },
        { (RegisterIndex.B, RegisterIndex.Bp), ("fcmovne", "st(0), st(5)") },
        { (RegisterIndex.B, RegisterIndex.Si), ("fcmovne", "st(0), st(6)") },
        { (RegisterIndex.B, RegisterIndex.Di), ("fcmovne", "st(0), st(7)") },
        
        // FCMOVNBE ST(0), ST(i)
        { (RegisterIndex.C, RegisterIndex.A), ("fcmovnbe", "st(0), st(0)") },
        { (RegisterIndex.C, RegisterIndex.C), ("fcmovnbe", "st(0), st(1)") },
        { (RegisterIndex.C, RegisterIndex.D), ("fcmovnbe", "st(0), st(2)") },
        { (RegisterIndex.C, RegisterIndex.B), ("fcmovnbe", "st(0), st(3)") },
        { (RegisterIndex.C, RegisterIndex.Sp), ("fcmovnbe", "st(0), st(4)") },
        { (RegisterIndex.C, RegisterIndex.Bp), ("fcmovnbe", "st(0), st(5)") },
        { (RegisterIndex.C, RegisterIndex.Si), ("fcmovnbe", "st(0), st(6)") },
        { (RegisterIndex.C, RegisterIndex.Di), ("fcmovnbe", "st(0), st(7)") },
        
        // FCMOVNU ST(0), ST(i)
        { (RegisterIndex.D, RegisterIndex.A), ("fcmovnu", "st(0), st(0)") },
        { (RegisterIndex.D, RegisterIndex.C), ("fcmovnu", "st(0), st(1)") },
        { (RegisterIndex.D, RegisterIndex.D), ("fcmovnu", "st(0), st(2)") },
        { (RegisterIndex.D, RegisterIndex.B), ("fcmovnu", "st(0), st(3)") },
        { (RegisterIndex.D, RegisterIndex.Sp), ("fcmovnu", "st(0), st(4)") },
        { (RegisterIndex.D, RegisterIndex.Bp), ("fcmovnu", "st(0), st(5)") },
        { (RegisterIndex.D, RegisterIndex.Si), ("fcmovnu", "st(0), st(6)") },
        { (RegisterIndex.D, RegisterIndex.Di), ("fcmovnu", "st(0), st(7)") },
        
        // Special cases
        { (RegisterIndex.Si, RegisterIndex.C), ("fclex", "") },
        { (RegisterIndex.Si, RegisterIndex.D), ("finit", "") },
        
        // FUCOMI ST(0), ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), ("fucomi", "st(0), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.C), ("fucomi", "st(0), st(1)") },
        { (RegisterIndex.Di, RegisterIndex.D), ("fucomi", "st(0), st(2)") },
        { (RegisterIndex.Di, RegisterIndex.B), ("fucomi", "st(0), st(3)") },
        { (RegisterIndex.Di, RegisterIndex.Sp), ("fucomi", "st(0), st(4)") },
        { (RegisterIndex.Di, RegisterIndex.Bp), ("fucomi", "st(0), st(5)") },
        { (RegisterIndex.Di, RegisterIndex.Si), ("fucomi", "st(0), st(6)") },
        { (RegisterIndex.Di, RegisterIndex.Di), ("fucomi", "st(0), st(7)") },
        
        // FCOMI ST(0), ST(i)
        { (RegisterIndex.Sp, RegisterIndex.A), ("fcomi", "st(0), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.C), ("fcomi", "st(0), st(1)") },
        { (RegisterIndex.Sp, RegisterIndex.D), ("fcomi", "st(0), st(2)") },
        { (RegisterIndex.Sp, RegisterIndex.B), ("fcomi", "st(0), st(3)") },
        { (RegisterIndex.Sp, RegisterIndex.Sp), ("fcomi", "st(0), st(4)") },
        { (RegisterIndex.Sp, RegisterIndex.Bp), ("fcomi", "st(0), st(5)") },
        { (RegisterIndex.Sp, RegisterIndex.Si), ("fcomi", "st(0), st(6)") },
        { (RegisterIndex.Sp, RegisterIndex.Di), ("fcomi", "st(0), st(7)") }
    };

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
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the mnemonic based on the reg field
            instruction.Mnemonic = MemoryMnemonics[(int)reg];
            
            // Get the base operand without size prefix
            string baseOperand = memOperand.Replace("dword ptr ", "");
            
            // Apply the appropriate size prefix based on the operation
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // 32-bit integer operations
            {
                // Keep the dword ptr prefix for integer operations
                instruction.Operands = memOperand;
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // 80-bit extended precision operations
            {
                instruction.Operands = $"tword ptr {baseOperand}";
            }
            else
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