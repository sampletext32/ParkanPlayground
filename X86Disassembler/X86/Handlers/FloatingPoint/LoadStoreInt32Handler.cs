using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int32 and miscellaneous operations (DB opcode)
/// </summary>
public class LoadStoreInt32Handler : InstructionHandler
{
    // Memory operand instruction types for DB opcode - load/store int32, misc
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Unknown, // fild - not in enum
        InstructionType.Unknown, // ??
        InstructionType.Unknown, // fist - not in enum
        InstructionType.Unknown, // fistp - not in enum
        InstructionType.Unknown, // ??
        InstructionType.Fld,     // fld - 80-bit extended precision
        InstructionType.Unknown, // ??
        InstructionType.Fstp     // fstp - 80-bit extended precision
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (InstructionType Type, FpuRegisterIndex DestIndex, FpuRegisterIndex? SrcIndex)> RegisterOperations = new()
    {
        // FCMOVNB ST(0), ST(i)
        { (RegisterIndex.A, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.A, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.A, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.A, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.A, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.A, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.A, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.A, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVNE ST(0), ST(i)
        { (RegisterIndex.B, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.B, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.B, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.B, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.B, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.B, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.B, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.B, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVNBE ST(0), ST(i)
        { (RegisterIndex.C, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.C, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.C, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.C, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.C, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.C, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.C, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.C, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVNU ST(0), ST(i)
        { (RegisterIndex.D, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.D, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.D, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.D, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.D, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.D, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.D, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.D, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // Special cases
        { (RegisterIndex.Si, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, null) }, // fclex
        { (RegisterIndex.Si, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, null) }, // finit
        
        // FUCOMI ST(0), ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Di, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Di, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Di, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Di, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Di, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Di, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCOMI ST(0), ST(i)
        { (RegisterIndex.Sp, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Sp, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Sp, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Sp, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Sp, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Sp, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Sp, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Sp, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) }
    };

    /// <summary>
    /// Initializes a new instance of the LoadStoreInt32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LoadStoreInt32Handler(InstructionDecoder decoder)
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
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the instruction type based on the reg field
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
            // Set the size based on the operation
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // 32-bit integer operations
            {
                // Keep the default 32-bit size
                memoryOperand.Size = 32;
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // 80-bit extended precision operations
            {
                // Set to 80-bit for extended precision
                memoryOperand.Size = 80;
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
                
                // Create the FPU register operands
                var destOperand = OperandFactory.CreateFPURegisterOperand(operation.DestIndex);
                
                // Set the structured operands
                if (operation.SrcIndex.HasValue)
                {
                    var srcOperand = OperandFactory.CreateFPURegisterOperand(operation.SrcIndex.Value);
                    instruction.StructuredOperands = 
                    [
                        destOperand,
                        srcOperand
                    ];
                }
                else
                {
                    instruction.StructuredOperands = 
                    [
                        destOperand
                    ];
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