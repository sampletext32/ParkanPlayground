using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int32 (DA opcode)
/// </summary>
public class Int32OperationHandler : InstructionHandler
{
    // Memory operand instruction types for DA opcode - operations on int32
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Fiadd,  // fiadd dword ptr [r/m]
        InstructionType.Fimul,  // fimul dword ptr [r/m]
        InstructionType.Ficom,  // ficom dword ptr [r/m]
        InstructionType.Ficomp, // ficomp dword ptr [r/m]
        InstructionType.Fisub,  // fisub dword ptr [r/m]
        InstructionType.Fisubr, // fisubr dword ptr [r/m]
        InstructionType.Fidiv,  // fidiv dword ptr [r/m]
        InstructionType.Fidivr  // fidivr dword ptr [r/m]
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(RegisterIndex Reg, RegisterIndex Rm), (InstructionType Type, FpuRegisterIndex DestIndex, FpuRegisterIndex SrcIndex)> RegisterOperations = new()
    {
        // FCMOVB st(0), st(i)
        { (RegisterIndex.A, RegisterIndex.A), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.A, RegisterIndex.C), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.A, RegisterIndex.D), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.A, RegisterIndex.B), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.A, RegisterIndex.Sp), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.A, RegisterIndex.Bp), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.A, RegisterIndex.Si), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.A, RegisterIndex.Di), (InstructionType.Fcmovb, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVE st(0), st(i)
        { (RegisterIndex.B, RegisterIndex.A), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.B, RegisterIndex.C), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.B, RegisterIndex.D), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.B, RegisterIndex.B), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.B, RegisterIndex.Sp), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.B, RegisterIndex.Bp), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.B, RegisterIndex.Si), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.B, RegisterIndex.Di), (InstructionType.Fcmove, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVBE st(0), st(i)
        { (RegisterIndex.C, RegisterIndex.A), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.C, RegisterIndex.C), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.C, RegisterIndex.D), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.C, RegisterIndex.B), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.C, RegisterIndex.Sp), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.C, RegisterIndex.Bp), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.C, RegisterIndex.Si), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.C, RegisterIndex.Di), (InstructionType.Fcmovbe, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // FCMOVU st(0), st(i)
        { (RegisterIndex.D, RegisterIndex.A), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (RegisterIndex.D, RegisterIndex.C), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST1) },
        { (RegisterIndex.D, RegisterIndex.D), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST2) },
        { (RegisterIndex.D, RegisterIndex.B), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST3) },
        { (RegisterIndex.D, RegisterIndex.Sp), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST4) },
        { (RegisterIndex.D, RegisterIndex.Bp), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST5) },
        { (RegisterIndex.D, RegisterIndex.Si), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST6) },
        { (RegisterIndex.D, RegisterIndex.Di), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST7) },
        
        // Special case
        { (RegisterIndex.Di, RegisterIndex.B), (InstructionType.Fcmovu, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) }
    };

    /// <summary>
    /// Initializes a new instance of the Int32OperationHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Int32OperationHandler(InstructionDecoder decoder)
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
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the instruction type based on the reg field
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
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
                var srcOperand = OperandFactory.CreateFPURegisterOperand(operation.SrcIndex);
                
                // Set the structured operands
                instruction.StructuredOperands = 
                [
                    destOperand,
                    srcOperand
                ];
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