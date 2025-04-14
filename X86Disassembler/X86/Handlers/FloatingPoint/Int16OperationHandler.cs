using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on int16 (DE opcode)
/// </summary>
public class Int16OperationHandler : InstructionHandler
{
    // Memory operand instruction types for DE opcode - operations on int16
    private static readonly InstructionType[] MemoryInstructionTypes =
    [
        InstructionType.Unknown, // fiadd - not in enum
        InstructionType.Unknown, // fimul - not in enum
        InstructionType.Unknown, // ficom - not in enum
        InstructionType.Unknown, // ficomp - not in enum
        InstructionType.Unknown, // fisub - not in enum
        InstructionType.Unknown, // fisubr - not in enum
        InstructionType.Unknown, // fidiv - not in enum
        InstructionType.Unknown  // fidivr - not in enum
    ];
    
    // Register-register operations mapping (mod=3)
    private static readonly Dictionary<(int Reg, int Rm), (InstructionType Type, FpuRegisterIndex DestIndex, FpuRegisterIndex? SrcIndex)> RegisterOperations = new()
    {
        // FADDP st(i), st(0)
        { (0, 0), (InstructionType.Fadd, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (0, 1), (InstructionType.Fadd, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (0, 2), (InstructionType.Fadd, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (0, 3), (InstructionType.Fadd, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (0, 4), (InstructionType.Fadd, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (0, 5), (InstructionType.Fadd, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (0, 6), (InstructionType.Fadd, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (0, 7), (InstructionType.Fadd, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) },
        
        // FMULP st(i), st(0)
        { (1, 0), (InstructionType.Fmul, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (1, 1), (InstructionType.Fmul, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (1, 2), (InstructionType.Fmul, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (1, 3), (InstructionType.Fmul, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (1, 4), (InstructionType.Fmul, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (1, 5), (InstructionType.Fmul, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (1, 6), (InstructionType.Fmul, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (1, 7), (InstructionType.Fmul, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) },
        
        // Special cases
        { (2, 3), (InstructionType.Fcomp, FpuRegisterIndex.ST0, null) },
        { (3, 3), (InstructionType.Fcompp, FpuRegisterIndex.ST0, null) },
        
        // FSUBP st(i), st(0)
        { (6, 0), (InstructionType.Fsub, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (6, 1), (InstructionType.Fsub, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (6, 2), (InstructionType.Fsub, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (6, 3), (InstructionType.Fsub, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (6, 4), (InstructionType.Fsub, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (6, 5), (InstructionType.Fsub, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (6, 6), (InstructionType.Fsub, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (6, 7), (InstructionType.Fsub, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) },
        
        // FSUBRP st(i), st(0)
        { (7, 0), (InstructionType.Fsubr, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (7, 1), (InstructionType.Fsubr, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (7, 2), (InstructionType.Fsubr, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (7, 3), (InstructionType.Fsubr, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (7, 4), (InstructionType.Fsubr, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (7, 5), (InstructionType.Fsubr, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (7, 6), (InstructionType.Fsubr, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (7, 7), (InstructionType.Fsubr, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) },
        
        // FDIVP st(i), st(0)
        { (4, 0), (InstructionType.Fdiv, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (4, 1), (InstructionType.Fdiv, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (4, 2), (InstructionType.Fdiv, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (4, 3), (InstructionType.Fdiv, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (4, 4), (InstructionType.Fdiv, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (4, 5), (InstructionType.Fdiv, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (4, 6), (InstructionType.Fdiv, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (4, 7), (InstructionType.Fdiv, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) },
        
        // FDIVRP st(i), st(0)
        { (5, 0), (InstructionType.Fdivr, FpuRegisterIndex.ST0, FpuRegisterIndex.ST0) },
        { (5, 1), (InstructionType.Fdivr, FpuRegisterIndex.ST1, FpuRegisterIndex.ST0) },
        { (5, 2), (InstructionType.Fdivr, FpuRegisterIndex.ST2, FpuRegisterIndex.ST0) },
        { (5, 3), (InstructionType.Fdivr, FpuRegisterIndex.ST3, FpuRegisterIndex.ST0) },
        { (5, 4), (InstructionType.Fdivr, FpuRegisterIndex.ST4, FpuRegisterIndex.ST0) },
        { (5, 5), (InstructionType.Fdivr, FpuRegisterIndex.ST5, FpuRegisterIndex.ST0) },
        { (5, 6), (InstructionType.Fdivr, FpuRegisterIndex.ST6, FpuRegisterIndex.ST0) },
        { (5, 7), (InstructionType.Fdivr, FpuRegisterIndex.ST7, FpuRegisterIndex.ST0) }
    };

    /// <summary>
    /// Initializes a new instance of the Int16OperationHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Int16OperationHandler(InstructionDecoder decoder)
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
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM();

        // Handle based on addressing mode
        if (mod != 3) // Memory operand
        {
            // Set the instruction type based on the reg field
            instruction.Type = MemoryInstructionTypes[(int)reg];
            
            // For memory operands, we need to set the size to 16-bit
            // Create a new memory operand with 16-bit size
            var int16Operand = memoryOperand;
            int16Operand.Size = 16;
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                int16Operand
            ];
        }
        else // Register operand (ST(i))
        {
            // Look up the instruction type in the register operations dictionary
            if (RegisterOperations.TryGetValue(((int)reg, (int)rm), out var operation))
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