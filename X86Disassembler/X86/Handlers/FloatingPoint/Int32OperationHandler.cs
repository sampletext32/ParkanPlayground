namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int32 (DA opcode)
/// </summary>
public class Int32OperationHandler : InstructionHandler
{
    // Memory operand mnemonics for DA opcode - operations on int32
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
        // FCMOVB st(0), st(i)
        { (RegisterIndex.A, RegisterIndex.A), ("fcmovb", "st(0), st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("fcmovb", "st(0), st(1)") },
        { (RegisterIndex.A, RegisterIndex.D), ("fcmovb", "st(0), st(2)") },
        { (RegisterIndex.A, RegisterIndex.B), ("fcmovb", "st(0), st(3)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("fcmovb", "st(0), st(4)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("fcmovb", "st(0), st(5)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("fcmovb", "st(0), st(6)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("fcmovb", "st(0), st(7)") },
        
        // FCMOVE st(0), st(i)
        { (RegisterIndex.B, RegisterIndex.A), ("fcmove", "st(0), st(0)") },
        { (RegisterIndex.B, RegisterIndex.C), ("fcmove", "st(0), st(1)") },
        { (RegisterIndex.B, RegisterIndex.D), ("fcmove", "st(0), st(2)") },
        { (RegisterIndex.B, RegisterIndex.B), ("fcmove", "st(0), st(3)") },
        { (RegisterIndex.B, RegisterIndex.Sp), ("fcmove", "st(0), st(4)") },
        { (RegisterIndex.B, RegisterIndex.Bp), ("fcmove", "st(0), st(5)") },
        { (RegisterIndex.B, RegisterIndex.Si), ("fcmove", "st(0), st(6)") },
        { (RegisterIndex.B, RegisterIndex.Di), ("fcmove", "st(0), st(7)") },
        
        // FCMOVBE st(0), st(i)
        { (RegisterIndex.C, RegisterIndex.A), ("fcmovbe", "st(0), st(0)") },
        { (RegisterIndex.C, RegisterIndex.C), ("fcmovbe", "st(0), st(1)") },
        { (RegisterIndex.C, RegisterIndex.D), ("fcmovbe", "st(0), st(2)") },
        { (RegisterIndex.C, RegisterIndex.B), ("fcmovbe", "st(0), st(3)") },
        { (RegisterIndex.C, RegisterIndex.Sp), ("fcmovbe", "st(0), st(4)") },
        { (RegisterIndex.C, RegisterIndex.Bp), ("fcmovbe", "st(0), st(5)") },
        { (RegisterIndex.C, RegisterIndex.Si), ("fcmovbe", "st(0), st(6)") },
        { (RegisterIndex.C, RegisterIndex.Di), ("fcmovbe", "st(0), st(7)") },
        
        // FCMOVU st(0), st(i)
        { (RegisterIndex.D, RegisterIndex.A), ("fcmovu", "st(0), st(0)") },
        { (RegisterIndex.D, RegisterIndex.C), ("fcmovu", "st(0), st(1)") },
        { (RegisterIndex.D, RegisterIndex.D), ("fcmovu", "st(0), st(2)") },
        { (RegisterIndex.D, RegisterIndex.B), ("fcmovu", "st(0), st(3)") },
        { (RegisterIndex.D, RegisterIndex.Sp), ("fcmovu", "st(0), st(4)") },
        { (RegisterIndex.D, RegisterIndex.Bp), ("fcmovu", "st(0), st(5)") },
        { (RegisterIndex.D, RegisterIndex.Si), ("fcmovu", "st(0), st(6)") },
        { (RegisterIndex.D, RegisterIndex.Di), ("fcmovu", "st(0), st(7)") },
        
        // Special case
        { (RegisterIndex.Di, RegisterIndex.B), ("fucompp", "") }
    };

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
            
            // Set the operands (already has dword ptr prefix for int32)
            instruction.Operands = memOperand;
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