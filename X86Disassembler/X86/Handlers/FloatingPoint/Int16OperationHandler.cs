namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int16 (DE opcode)
/// </summary>
public class Int16OperationHandler : InstructionHandler
{
    // Memory operand mnemonics for DE opcode - operations on int16
    private static readonly string[] MemoryMnemonics =
    [
        "fiadd",  // 0
        "fimul",  // 1
        "ficom",  // 2
        "ficomp", // 3
        "fisub",  // 4
        "fisubr", // 5
        "fidiv",  // 6
        "fidivr"  // 7
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (string Mnemonic, string Operands)> RegisterOperations = new()
    {
        // FADDP st(i), st(0)
        { (RegisterIndex.A, RegisterIndex.A), ("faddp", "st(0), st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("faddp", "st(1), st(0)") },
        { (RegisterIndex.A, RegisterIndex.D), ("faddp", "st(2), st(0)") },
        { (RegisterIndex.A, RegisterIndex.B), ("faddp", "st(3), st(0)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("faddp", "st(4), st(0)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("faddp", "st(5), st(0)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("faddp", "st(6), st(0)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("faddp", "st(7), st(0)") },
        
        // FMULP st(i), st(0)
        { (RegisterIndex.B, RegisterIndex.A), ("fmulp", "st(0), st(0)") },
        { (RegisterIndex.B, RegisterIndex.C), ("fmulp", "st(1), st(0)") },
        { (RegisterIndex.B, RegisterIndex.D), ("fmulp", "st(2), st(0)") },
        { (RegisterIndex.B, RegisterIndex.B), ("fmulp", "st(3), st(0)") },
        { (RegisterIndex.B, RegisterIndex.Sp), ("fmulp", "st(4), st(0)") },
        { (RegisterIndex.B, RegisterIndex.Bp), ("fmulp", "st(5), st(0)") },
        { (RegisterIndex.B, RegisterIndex.Si), ("fmulp", "st(6), st(0)") },
        { (RegisterIndex.B, RegisterIndex.Di), ("fmulp", "st(7), st(0)") },
        
        // Special cases
        { (RegisterIndex.C, RegisterIndex.B), ("fcomp", "") },
        { (RegisterIndex.D, RegisterIndex.B), ("fcompp", "") },
        
        // FSUBP st(i), st(0)
        { (RegisterIndex.Si, RegisterIndex.A), ("fsubp", "st(0), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.C), ("fsubp", "st(1), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.D), ("fsubp", "st(2), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.B), ("fsubp", "st(3), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.Sp), ("fsubp", "st(4), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.Bp), ("fsubp", "st(5), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.Si), ("fsubp", "st(6), st(0)") },
        { (RegisterIndex.Si, RegisterIndex.Di), ("fsubp", "st(7), st(0)") },
        
        // FSUBRP st(i), st(0)
        { (RegisterIndex.Di, RegisterIndex.A), ("fsubrp", "st(0), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.C), ("fsubrp", "st(1), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.D), ("fsubrp", "st(2), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.B), ("fsubrp", "st(3), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.Sp), ("fsubrp", "st(4), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.Bp), ("fsubrp", "st(5), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.Si), ("fsubrp", "st(6), st(0)") },
        { (RegisterIndex.Di, RegisterIndex.Di), ("fsubrp", "st(7), st(0)") },
        
        // FDIVP st(i), st(0)
        { (RegisterIndex.Sp, RegisterIndex.A), ("fdivp", "st(0), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.C), ("fdivp", "st(1), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.D), ("fdivp", "st(2), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.B), ("fdivp", "st(3), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.Sp), ("fdivp", "st(4), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.Bp), ("fdivp", "st(5), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.Si), ("fdivp", "st(6), st(0)") },
        { (RegisterIndex.Sp, RegisterIndex.Di), ("fdivp", "st(7), st(0)") },
        
        // FDIVRP st(i), st(0)
        { (RegisterIndex.Bp, RegisterIndex.A), ("fdivrp", "st(0), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.C), ("fdivrp", "st(1), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.D), ("fdivrp", "st(2), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.B), ("fdivrp", "st(3), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.Sp), ("fdivrp", "st(4), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.Bp), ("fdivrp", "st(5), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.Si), ("fdivrp", "st(6), st(0)") },
        { (RegisterIndex.Bp, RegisterIndex.Di), ("fdivrp", "st(7), st(0)") }
    };

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
            
            // Need to modify the default dword ptr to word ptr for 16-bit integers
            instruction.Operands = memOperand.Replace("dword ptr", "word ptr");
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