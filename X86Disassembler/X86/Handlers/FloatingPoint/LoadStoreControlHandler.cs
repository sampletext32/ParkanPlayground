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
        InstructionType.Fld,     // 0
        InstructionType.Unknown, // 1
        InstructionType.Fst,     // 2
        InstructionType.Fstp,    // 3
        InstructionType.Unknown, // 4 - fldenv not in enum
        InstructionType.Fldcw,   // 5
        InstructionType.Unknown, // 6 - fnstenv not in enum
        InstructionType.Fnstcw   // 7
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
        { (RegisterIndex.Di, RegisterIndex.A), (InstructionType.Unknown, null) }, // f2xm1 not in enum
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Unknown, null) }, // fyl2x not in enum
        { (RegisterIndex.Di, RegisterIndex.C), (InstructionType.Unknown, null) }, // fptan not in enum
        { (RegisterIndex.Di, RegisterIndex.D), (InstructionType.Unknown, null) }, // fpatan not in enum
        { (RegisterIndex.Di, RegisterIndex.Si), (InstructionType.Unknown, null) }, // fxtract not in enum
        { (RegisterIndex.Di, RegisterIndex.Di), (InstructionType.Unknown, null) }, // fprem1 not in enum
        { (RegisterIndex.Di, RegisterIndex.Sp), (InstructionType.Unknown, null) }, // fdecstp not in enum
        { (RegisterIndex.Di, RegisterIndex.Bp), (InstructionType.Unknown, null) }, // fincstp not in enum
        
        // D9D0-D9DF special instructions (reg=5)
        { (RegisterIndex.Sp, RegisterIndex.A), (InstructionType.Unknown, null) }, // fprem not in enum
        { (RegisterIndex.Sp, RegisterIndex.B), (InstructionType.Unknown, null) }, // fyl2xp1 not in enum
        { (RegisterIndex.Sp, RegisterIndex.C), (InstructionType.Unknown, null) }, // fsqrt not in enum
        { (RegisterIndex.Sp, RegisterIndex.D), (InstructionType.Unknown, null) }, // fsincos not in enum
        { (RegisterIndex.Sp, RegisterIndex.Si), (InstructionType.Unknown, null) }, // frndint not in enum
        { (RegisterIndex.Sp, RegisterIndex.Di), (InstructionType.Unknown, null) }, // fscale not in enum
        { (RegisterIndex.Sp, RegisterIndex.Sp), (InstructionType.Unknown, null) }, // fsin not in enum
        { (RegisterIndex.Sp, RegisterIndex.Bp), (InstructionType.Unknown, null) }, // fcos not in enum
        
        // D9C8-D9CF special instructions (reg=4)
        { (RegisterIndex.Bp, RegisterIndex.A), (InstructionType.Unknown, null) }, // fnop not in enum
        { (RegisterIndex.Bp, RegisterIndex.C), (InstructionType.Unknown, null) }  // fwait not in enum
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
