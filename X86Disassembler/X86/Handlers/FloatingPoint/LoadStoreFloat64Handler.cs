namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store float64 operations (DD opcode)
/// </summary>
public class LoadStoreFloat64Handler : InstructionHandler
{
    // Memory operand mnemonics for DD opcode - load/store float64
    private static readonly string[] MemoryMnemonics =
    [
        "fld",     // 0
        "??",      // 1
        "fst",     // 2
        "fstp",    // 3
        "frstor",  // 4
        "??",      // 5
        "fnsave",  // 6
        "fnstsw"   // 7
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (string Mnemonic, string Operands)> RegisterOperations = new()
    {
        // FFREE ST(i)
        { (RegisterIndex.A, RegisterIndex.A), ("ffree", "st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("ffree", "st(1)") },
        { (RegisterIndex.A, RegisterIndex.D), ("ffree", "st(2)") },
        { (RegisterIndex.A, RegisterIndex.B), ("ffree", "st(3)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("ffree", "st(4)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("ffree", "st(5)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("ffree", "st(6)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("ffree", "st(7)") },
        
        // FST ST(i)
        { (RegisterIndex.C, RegisterIndex.A), ("fst", "st(0)") },
        { (RegisterIndex.C, RegisterIndex.C), ("fst", "st(1)") },
        { (RegisterIndex.C, RegisterIndex.D), ("fst", "st(2)") },
        { (RegisterIndex.C, RegisterIndex.B), ("fst", "st(3)") },
        { (RegisterIndex.C, RegisterIndex.Sp), ("fst", "st(4)") },
        { (RegisterIndex.C, RegisterIndex.Bp), ("fst", "st(5)") },
        { (RegisterIndex.C, RegisterIndex.Si), ("fst", "st(6)") },
        { (RegisterIndex.C, RegisterIndex.Di), ("fst", "st(7)") },
        
        // FSTP ST(i)
        { (RegisterIndex.D, RegisterIndex.A), ("fstp", "st(0)") },
        { (RegisterIndex.D, RegisterIndex.C), ("fstp", "st(1)") },
        { (RegisterIndex.D, RegisterIndex.D), ("fstp", "st(2)") },
        { (RegisterIndex.D, RegisterIndex.B), ("fstp", "st(3)") },
        { (RegisterIndex.D, RegisterIndex.Sp), ("fstp", "st(4)") },
        { (RegisterIndex.D, RegisterIndex.Bp), ("fstp", "st(5)") },
        { (RegisterIndex.D, RegisterIndex.Si), ("fstp", "st(6)") },
        { (RegisterIndex.D, RegisterIndex.Di), ("fstp", "st(7)") },
        
        // FUCOM ST(i)
        { (RegisterIndex.Si, RegisterIndex.A), ("fucom", "st(0)") },
        { (RegisterIndex.Si, RegisterIndex.C), ("fucom", "st(1)") },
        { (RegisterIndex.Si, RegisterIndex.D), ("fucom", "st(2)") },
        { (RegisterIndex.Si, RegisterIndex.B), ("fucom", "st(3)") },
        { (RegisterIndex.Si, RegisterIndex.Sp), ("fucom", "st(4)") },
        { (RegisterIndex.Si, RegisterIndex.Bp), ("fucom", "st(5)") },
        { (RegisterIndex.Si, RegisterIndex.Si), ("fucom", "st(6)") },
        { (RegisterIndex.Si, RegisterIndex.Di), ("fucom", "st(7)") },
        
        // FUCOMP ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), ("fucomp", "st(0)") },
        { (RegisterIndex.Di, RegisterIndex.C), ("fucomp", "st(1)") },
        { (RegisterIndex.Di, RegisterIndex.D), ("fucomp", "st(2)") },
        { (RegisterIndex.Di, RegisterIndex.B), ("fucomp", "st(3)") },
        { (RegisterIndex.Di, RegisterIndex.Sp), ("fucomp", "st(4)") },
        { (RegisterIndex.Di, RegisterIndex.Bp), ("fucomp", "st(5)") },
        { (RegisterIndex.Di, RegisterIndex.Si), ("fucomp", "st(6)") },
        { (RegisterIndex.Di, RegisterIndex.Di), ("fucomp", "st(7)") }
    };

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
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM(true); // true for 64-bit operand

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the mnemonic based on the reg field
            instruction.Mnemonic = MemoryMnemonics[(int)reg];
            
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // fld, fst, fstp
            {
                // Keep the qword ptr prefix from ModRMDecoder
                instruction.Operands = memOperand;
            }
            else // frstor, fnsave, fnstsw
            {
                // Remove the qword ptr prefix for these operations
                instruction.Operands = memOperand.Replace("qword ptr ", "");
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