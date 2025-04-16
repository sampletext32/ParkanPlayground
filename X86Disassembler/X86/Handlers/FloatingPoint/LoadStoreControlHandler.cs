using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load, store, and control operations (D9 opcode)
/// </summary>
public class LoadStoreControlHandler : InstructionHandler
{
    // Memory operand instruction types for D9 opcode - load, store, and control operations
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Fld,     // 0 - fld dword ptr [r/m]
        InstructionType.Unknown, // 1 - (reserved)
        InstructionType.Fst,     // 2 - fst dword ptr [r/m]
        InstructionType.Fstp,    // 3 - fstp dword ptr [r/m]
        InstructionType.Fldenv,  // 4 - fldenv [r/m]
        InstructionType.Fldcw,   // 5 - fldcw [r/m]
        InstructionType.Fnstenv, // 6 - fnstenv [r/m]
        InstructionType.Fnstcw   // 7 - fnstcw [r/m]
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (InstructionType Type, FpuRegisterIndex? OperandIndex)> RegisterOperations = new()
    {
        // FLD ST(i)
        { (RegisterIndex.A, RegisterIndex.A), (InstructionType.Fld, FpuRegisterIndex.ST0) },
        { (RegisterIndex.A, RegisterIndex.C), (InstructionType.Fld, FpuRegisterIndex.ST1) },
        { (RegisterIndex.A, RegisterIndex.D), (InstructionType.Fld, FpuRegisterIndex.ST2) },
        { (RegisterIndex.A, RegisterIndex.B), (InstructionType.Fld, FpuRegisterIndex.ST3) },
        { (RegisterIndex.A, RegisterIndex.Sp), (InstructionType.Fld, FpuRegisterIndex.ST4) },
        { (RegisterIndex.A, RegisterIndex.Bp), (InstructionType.Fld, FpuRegisterIndex.ST5) },
        { (RegisterIndex.A, RegisterIndex.Si), (InstructionType.Fld, FpuRegisterIndex.ST6) },
        { (RegisterIndex.A, RegisterIndex.Di), (InstructionType.Fld, FpuRegisterIndex.ST7) },
        
        // FXCH ST(i)
        { (RegisterIndex.B, RegisterIndex.A), (InstructionType.Fxch, FpuRegisterIndex.ST0) },
        { (RegisterIndex.B, RegisterIndex.C), (InstructionType.Fxch, FpuRegisterIndex.ST1) },
        { (RegisterIndex.B, RegisterIndex.D), (InstructionType.Fxch, FpuRegisterIndex.ST2) },
        { (RegisterIndex.B, RegisterIndex.B), (InstructionType.Fxch, FpuRegisterIndex.ST3) },
        { (RegisterIndex.B, RegisterIndex.Sp), (InstructionType.Fxch, FpuRegisterIndex.ST4) },
        { (RegisterIndex.B, RegisterIndex.Bp), (InstructionType.Fxch, FpuRegisterIndex.ST5) },
        { (RegisterIndex.B, RegisterIndex.Si), (InstructionType.Fxch, FpuRegisterIndex.ST6) },
        { (RegisterIndex.B, RegisterIndex.Di), (InstructionType.Fxch, FpuRegisterIndex.ST7) },
        
        // D9E0-D9EF special instructions (reg=6)
        { (RegisterIndex.Si, RegisterIndex.A), (InstructionType.Fchs, null) },
        { (RegisterIndex.Si, RegisterIndex.B), (InstructionType.Fabs, null) },
        { (RegisterIndex.Si, RegisterIndex.Si), (InstructionType.Ftst, null) },
        { (RegisterIndex.Si, RegisterIndex.Di), (InstructionType.Fxam, null) },
        
        // D9F0-D9FF special instructions (reg=7)
        { (RegisterIndex.Di, RegisterIndex.A), (InstructionType.F2xm1, null) }, 
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Fyl2x, null) }, 
        { (RegisterIndex.Di, RegisterIndex.C), (InstructionType.Fptan, null) }, 
        { (RegisterIndex.Di, RegisterIndex.D), (InstructionType.Fpatan, null) }, 
        { (RegisterIndex.Di, RegisterIndex.Si), (InstructionType.Fxtract, null) }, 
        { (RegisterIndex.Di, RegisterIndex.Di), (InstructionType.Fprem1, null) }, 
        { (RegisterIndex.Di, RegisterIndex.Sp), (InstructionType.Fdecstp, null) }, 
        { (RegisterIndex.Di, RegisterIndex.Bp), (InstructionType.Fincstp, null) },
        
        // D9D0-D9DF special instructions (reg=5)
        { (RegisterIndex.Sp, RegisterIndex.A), (InstructionType.Fprem, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.B), (InstructionType.Fyl2xp1, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.C), (InstructionType.Fsqrt, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.D), (InstructionType.Fsincos, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.Si), (InstructionType.Frndint, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.Di), (InstructionType.Fscale, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.Sp), (InstructionType.Fsin, null) }, 
        { (RegisterIndex.Sp, RegisterIndex.Bp), (InstructionType.Fcos, null) },
        
        // D9C8-D9CF special instructions (reg=4)
        { (RegisterIndex.Bp, RegisterIndex.A), (InstructionType.Fnop, null) }, 
        { (RegisterIndex.Bp, RegisterIndex.C), (InstructionType.Fwait, null) }  
    };

    /// <summary>
    /// Initializes a new instance of the LoadStoreControlHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LoadStoreControlHandler(InstructionDecoder decoder) 
        : base(decoder)
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
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();
        
        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the instruction type based on the reg field
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
            // Set the size based on the operation
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // fld, fst, fstp
            {
                // Keep the default 32-bit size for floating point operations
                memoryOperand.Size = 32;
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // fldcw, fnstcw
            {
                // Set to 16-bit for control word operations
                memoryOperand.Size = 16;
            }
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                memoryOperand
            ];
        }
        else // Register operand (ST(i))
        {
            // Look up the instruction type in the register operations dictionary
            if (RegisterOperations.TryGetValue((reg, rm), out var operation))
            {
                instruction.Type = operation.Type;
                
                // Set the structured operands
                if (operation.OperandIndex.HasValue)
                {
                    var operand = OperandFactory.CreateFPURegisterOperand(operation.OperandIndex.Value);
                    instruction.StructuredOperands = 
                    [
                        operand
                    ];
                }
                else
                {
                    // No operands for instructions like fchs, fabs, etc.
                    instruction.StructuredOperands = [];
                }
            }
            else
            {
                // Unknown instruction
                instruction.Type = InstructionType.Unknown;
                instruction.StructuredOperands = [];
            }
        }
        
        return true;
    }
}
