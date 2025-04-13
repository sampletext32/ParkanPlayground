namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load, store, and control operations (D9 opcode)
/// </summary>
public class LoadStoreControlHandler : InstructionHandler
{
    // Memory operand mnemonics for D9 opcode - load, store, and control operations
    private static readonly string[] MemoryMnemonics =
    [
        "fld",     // 0
        "??",      // 1
        "fst",     // 2
        "fstp",    // 3
        "fldenv",  // 4
        "fldcw",   // 5
        "fnstenv", // 6
        "fnstcw"   // 7
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (string Mnemonic, string Operands)> RegisterOperations = new()
    {
        // FLD ST(i)
        { (RegisterIndex.A, RegisterIndex.A), ("fld", "st(0)") },
        { (RegisterIndex.A, RegisterIndex.C), ("fld", "st(1)") },
        { (RegisterIndex.A, RegisterIndex.D), ("fld", "st(2)") },
        { (RegisterIndex.A, RegisterIndex.B), ("fld", "st(3)") },
        { (RegisterIndex.A, RegisterIndex.Sp), ("fld", "st(4)") },
        { (RegisterIndex.A, RegisterIndex.Bp), ("fld", "st(5)") },
        { (RegisterIndex.A, RegisterIndex.Si), ("fld", "st(6)") },
        { (RegisterIndex.A, RegisterIndex.Di), ("fld", "st(7)") },
        
        // FXCH ST(i)
        { (RegisterIndex.B, RegisterIndex.A), ("fxch", "st(0)") },
        { (RegisterIndex.B, RegisterIndex.C), ("fxch", "st(1)") },
        { (RegisterIndex.B, RegisterIndex.D), ("fxch", "st(2)") },
        { (RegisterIndex.B, RegisterIndex.B), ("fxch", "st(3)") },
        { (RegisterIndex.B, RegisterIndex.Sp), ("fxch", "st(4)") },
        { (RegisterIndex.B, RegisterIndex.Bp), ("fxch", "st(5)") },
        { (RegisterIndex.B, RegisterIndex.Si), ("fxch", "st(6)") },
        { (RegisterIndex.B, RegisterIndex.Di), ("fxch", "st(7)") },
        
        // D9E0-D9EF special instructions (reg=6)
        { (RegisterIndex.Si, RegisterIndex.A), ("fchs", "") },
        { (RegisterIndex.Si, RegisterIndex.B), ("fabs", "") },
        { (RegisterIndex.Si, RegisterIndex.Si), ("ftst", "") },
        { (RegisterIndex.Si, RegisterIndex.Di), ("fxam", "") },
        
        // D9F0-D9FF special instructions (reg=7)
        { (RegisterIndex.Di, RegisterIndex.A), ("f2xm1", "") },
        { (RegisterIndex.Di, RegisterIndex.B), ("fyl2x", "") },
        { (RegisterIndex.Di, RegisterIndex.C), ("fptan", "") },
        { (RegisterIndex.Di, RegisterIndex.D), ("fpatan", "") },
        { (RegisterIndex.Di, RegisterIndex.Si), ("fxtract", "") },
        { (RegisterIndex.Di, RegisterIndex.Di), ("fprem1", "") },
        { (RegisterIndex.Di, RegisterIndex.Sp), ("fdecstp", "") },
        { (RegisterIndex.Di, RegisterIndex.Bp), ("fincstp", "") },
        
        // D9D0-D9DF special instructions (reg=5)
        { (RegisterIndex.Sp, RegisterIndex.A), ("fprem", "") },
        { (RegisterIndex.Sp, RegisterIndex.B), ("fyl2xp1", "") },
        { (RegisterIndex.Sp, RegisterIndex.C), ("fsqrt", "") },
        { (RegisterIndex.Sp, RegisterIndex.D), ("fsincos", "") },
        { (RegisterIndex.Sp, RegisterIndex.Si), ("frndint", "") },
        { (RegisterIndex.Sp, RegisterIndex.Di), ("fscale", "") },
        { (RegisterIndex.Sp, RegisterIndex.Sp), ("fsin", "") },
        { (RegisterIndex.Sp, RegisterIndex.Bp), ("fcos", "") },
        
        // D9C8-D9CF special instructions (reg=4)
        { (RegisterIndex.Bp, RegisterIndex.A), ("fnop", "") },
        { (RegisterIndex.Bp, RegisterIndex.C), ("fwait", "") }
    };
    
    /// <summary>
    /// Initializes a new instance of the LoadStoreControlHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LoadStoreControlHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xD9;
    }
    
    /// <summary>
    /// Decodes a floating-point instruction for load, store, and control operations
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
            
            // Different operand types based on the instruction
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // fld, fst, fstp
            {
                // Keep the dword ptr prefix from ModRMDecoder
                instruction.Operands = memOperand;
            }
            else // fldenv, fldcw, fnstenv, fnstcw
            {
                if (reg == RegisterIndex.Di) // fldcw - should use word ptr
                {
                    instruction.Operands = memOperand.Replace("dword ptr", "word ptr");
                }
                else // fldenv, fnstenv, fnstcw
                {
                    // Remove the dword ptr prefix for other control operations
                    instruction.Operands = memOperand.Replace("dword ptr ", "");
                }
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
