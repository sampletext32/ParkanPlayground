using X86Disassembler.X86.Operands;

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
    
    // Memory operand instruction types for DD opcode
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Fld,     // 0
        InstructionType.Unknown, // 1
        InstructionType.Fst,     // 2
        InstructionType.Fstp,    // 3
        InstructionType.Unknown, // 4 - frstor not in enum
        InstructionType.Unknown, // 5
        InstructionType.Unknown, // 6 - fnsave not in enum
        InstructionType.Fnstsw   // 7
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (InstructionType Type, FpuRegisterIndex OperandIndex)> RegisterOperations = new()
    {
        // FFREE ST(i)
        { (RegisterIndex.A, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0) },
        { (RegisterIndex.A, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST1) },
        { (RegisterIndex.A, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST2) },
        { (RegisterIndex.A, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST3) },
        { (RegisterIndex.A, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST4) },
        { (RegisterIndex.A, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST5) },
        { (RegisterIndex.A, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST6) },
        { (RegisterIndex.A, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST7) },
        
        // FST ST(i)
        { (RegisterIndex.C, RegisterIndex.A), (InstructionType.Fst, FpuRegisterIndex.ST0) },
        { (RegisterIndex.C, RegisterIndex.C), (InstructionType.Fst, FpuRegisterIndex.ST1) },
        { (RegisterIndex.C, RegisterIndex.D), (InstructionType.Fst, FpuRegisterIndex.ST2) },
        { (RegisterIndex.C, RegisterIndex.B), (InstructionType.Fst, FpuRegisterIndex.ST3) },
        { (RegisterIndex.C, RegisterIndex.Sp), (InstructionType.Fst, FpuRegisterIndex.ST4) },
        { (RegisterIndex.C, RegisterIndex.Bp), (InstructionType.Fst, FpuRegisterIndex.ST5) },
        { (RegisterIndex.C, RegisterIndex.Si), (InstructionType.Fst, FpuRegisterIndex.ST6) },
        { (RegisterIndex.C, RegisterIndex.Di), (InstructionType.Fst, FpuRegisterIndex.ST7) },
        
        // FSTP ST(i)
        { (RegisterIndex.D, RegisterIndex.A), (InstructionType.Fstp, FpuRegisterIndex.ST0) },
        { (RegisterIndex.D, RegisterIndex.C), (InstructionType.Fstp, FpuRegisterIndex.ST1) },
        { (RegisterIndex.D, RegisterIndex.D), (InstructionType.Fstp, FpuRegisterIndex.ST2) },
        { (RegisterIndex.D, RegisterIndex.B), (InstructionType.Fstp, FpuRegisterIndex.ST3) },
        { (RegisterIndex.D, RegisterIndex.Sp), (InstructionType.Fstp, FpuRegisterIndex.ST4) },
        { (RegisterIndex.D, RegisterIndex.Bp), (InstructionType.Fstp, FpuRegisterIndex.ST5) },
        { (RegisterIndex.D, RegisterIndex.Si), (InstructionType.Fstp, FpuRegisterIndex.ST6) },
        { (RegisterIndex.D, RegisterIndex.Di), (InstructionType.Fstp, FpuRegisterIndex.ST7) },
        
        // FUCOM ST(i)
        { (RegisterIndex.Si, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Si, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Si, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Si, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Si, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Si, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Si, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Si, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST7) },
        
        // FUCOMP ST(i)
        { (RegisterIndex.Di, RegisterIndex.A), (InstructionType.Unknown, FpuRegisterIndex.ST0) },
        { (RegisterIndex.Di, RegisterIndex.C), (InstructionType.Unknown, FpuRegisterIndex.ST1) },
        { (RegisterIndex.Di, RegisterIndex.D), (InstructionType.Unknown, FpuRegisterIndex.ST2) },
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Unknown, FpuRegisterIndex.ST3) },
        { (RegisterIndex.Di, RegisterIndex.Sp), (InstructionType.Unknown, FpuRegisterIndex.ST4) },
        { (RegisterIndex.Di, RegisterIndex.Bp), (InstructionType.Unknown, FpuRegisterIndex.ST5) },
        { (RegisterIndex.Di, RegisterIndex.Si), (InstructionType.Unknown, FpuRegisterIndex.ST6) },
        { (RegisterIndex.Di, RegisterIndex.Di), (InstructionType.Unknown, FpuRegisterIndex.ST7) }
    };

    /// <summary>
    /// Initializes a new instance of the LoadStoreFloat64Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LoadStoreFloat64Handler(InstructionDecoder decoder)
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
        return opcode == 0xDD;
    }

    /// <summary>
    /// Decodes a floating point instruction with the DD opcode
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();

        // Set the instruction type based on the mod and reg fields
        if (mod != 3) // Memory operand
        {
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
            // For memory operands, the instruction depends on the reg field
            switch (reg)
            {
                case RegisterIndex.A: // FLD m64real
                    // Set the structured operands
                    memoryOperand.Size = 64; // Set size to 64 bits for double precision
                    instruction.StructuredOperands = 
                    [
                        memoryOperand
                    ];
                    return true;
                    
                case RegisterIndex.C: // FST m64real
                case RegisterIndex.D: // FSTP m64real
                    // Set the structured operands
                    memoryOperand.Size = 64; // Set size to 64 bits for double precision
                    instruction.StructuredOperands = 
                    [
                        memoryOperand
                    ];
                    return true;
                    
                default:
                    // For unsupported instructions, just set the type to Unknown
                    instruction.Type = InstructionType.Unknown;
                    return true;
            }
        }
        else // Register operand (mod == 3)
        {
            // Look up the instruction type in the register operations dictionary
            if (RegisterOperations.TryGetValue((reg, rm), out var operation))
            {
                instruction.Type = operation.Type;
                
                // Create the FPU register operand
                var fpuRegisterOperand = OperandFactory.CreateFPURegisterOperand(operation.OperandIndex);
                
                // Set the structured operands
                instruction.StructuredOperands = 
                [
                    fpuRegisterOperand
                ];
                
                return true;
            }
        }

        return false;
    }
}