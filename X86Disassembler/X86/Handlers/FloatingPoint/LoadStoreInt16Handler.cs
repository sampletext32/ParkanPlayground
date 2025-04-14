using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load/store int16 and miscellaneous operations (DF opcode)
/// </summary>
public class LoadStoreInt16Handler : InstructionHandler
{
    // Memory operand instruction types for DF opcode - load/store int16, misc
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Fild,   // fild word ptr [r/m]
        InstructionType.Unknown, // fistt word ptr [r/m] (not implemented)
        InstructionType.Fst,    // fist word ptr [r/m]
        InstructionType.Fstp,   // fistp word ptr [r/m]
        InstructionType.Fld,    // fbld packed BCD [r/m]
        InstructionType.Fild,   // fild qword ptr [r/m] (64-bit integer)
        InstructionType.Fst,    // fbstp packed BCD [r/m]
        InstructionType.Fstp    // fistp qword ptr [r/m] (64-bit integer)
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (InstructionType Type, FpuRegisterIndex OperandIndex, FpuRegisterIndex? SrcIndex)> RegisterOperations = new()
    {
        // FFREEP ST(i)
        { (RegisterIndex.A, RegisterIndex.A), (InstructionType.Ffreep, FpuRegisterIndex.ST0, null) },
        { (RegisterIndex.A, RegisterIndex.C), (InstructionType.Ffreep, FpuRegisterIndex.ST1, null) },
        { (RegisterIndex.A, RegisterIndex.D), (InstructionType.Ffreep, FpuRegisterIndex.ST2, null) },
        { (RegisterIndex.A, RegisterIndex.B), (InstructionType.Ffreep, FpuRegisterIndex.ST3, null) },
        { (RegisterIndex.A, RegisterIndex.Sp), (InstructionType.Ffreep, FpuRegisterIndex.ST4, null) },
        { (RegisterIndex.A, RegisterIndex.Bp), (InstructionType.Ffreep, FpuRegisterIndex.ST5, null) },
        { (RegisterIndex.A, RegisterIndex.Si), (InstructionType.Ffreep, FpuRegisterIndex.ST6, null) },
        { (RegisterIndex.A, RegisterIndex.Di), (InstructionType.Ffreep, FpuRegisterIndex.ST7, null) },
        
        // Special cases
        { (RegisterIndex.B, RegisterIndex.A), (InstructionType.Fxch, FpuRegisterIndex.ST0, null) },
        { (RegisterIndex.C, RegisterIndex.A), (InstructionType.Fstp, FpuRegisterIndex.ST1, null) },
        { (RegisterIndex.D, RegisterIndex.A), (InstructionType.Fstp, FpuRegisterIndex.ST1, null) },
        
        // FUCOMIP ST(0), ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Di, RegisterIndex.C), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Di, RegisterIndex.D), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Di, RegisterIndex.Sp), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Di, RegisterIndex.Bp), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Di, RegisterIndex.Si), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Di, RegisterIndex.Di), (InstructionType.Fucomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCOMIP ST(0), ST(i)
        { (RegisterIndex.Sp, RegisterIndex.A), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Sp, RegisterIndex.C), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Sp, RegisterIndex.D), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Sp, RegisterIndex.B), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Sp, RegisterIndex.Sp), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Sp, RegisterIndex.Bp), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Sp, RegisterIndex.Si), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Sp, RegisterIndex.Di), (InstructionType.Fcomip, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) }
    };

    /// <summary>
    /// Initializes a new instance of the LoadStoreInt16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LoadStoreInt16Handler(InstructionDecoder decoder)
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
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();

        // Check for FNSTSW AX (DF E0)
        if (mod == 3 && reg == RegisterIndex.Bp && rm == RegisterIndex.A)
        {
            // This is handled by the FnstswHandler, so we should not handle it here
            return false;
        }

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the instruction type based on the reg field
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
            // Set the size based on the operation
            if (reg == RegisterIndex.A || reg == RegisterIndex.C || reg == RegisterIndex.D) // 16-bit integer operations
            {
                // Set to 16-bit for integer operations
                memoryOperand.Size = 16;
            }
            else if (reg == RegisterIndex.Di || reg == RegisterIndex.Bp) // 64-bit integer operations
            {
                // Set to 64-bit for integer operations
                memoryOperand.Size = 64;
            }
            else if (reg == RegisterIndex.Si || reg == RegisterIndex.Sp) // 80-bit packed BCD operations
            {
                // Set to 80-bit for BCD operations
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
                var destOperand = OperandFactory.CreateFPURegisterOperand(operation.OperandIndex);
                
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