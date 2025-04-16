using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on float64 (DC opcode)
/// </summary>
public class Float64OperationHandler : InstructionHandler
{
    // DC opcode - operations on float64
    private static readonly string[] Mnemonics =
    [
        "fadd",
        "fmul",
        "fcom",
        "fcomp",
        "fsub",
        "fsubr",
        "fdiv",
        "fdivr"
    ];

    // Corresponding instruction types for each mnemonic
    private static readonly InstructionType[] InstructionTypes =
    [
        InstructionType.Fadd,
        InstructionType.Fmul,
        InstructionType.Fcom,
        InstructionType.Fcomp,
        InstructionType.Fsub,
        InstructionType.Fsubr,
        InstructionType.Fdiv,
        InstructionType.Fdivr
    ];

    /// <summary>
    /// Initializes a new instance of the Float64OperationHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Float64OperationHandler(InstructionDecoder decoder) 
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
        return opcode == 0xDC;
    }
    
    /// <summary>
    /// Decodes a floating-point instruction for float64 operations
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
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM64(); // Use the 64-bit version

        // Set the instruction type based on the reg field
        instruction.Type = InstructionTypes[(int)reg];
        
        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            // Ensure the memory operand has the correct size (64-bit float)
            operand.Size = 64;
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                operand
            ];
        }
        else // Register operand (ST(i))
        {
            // For DC C0-DC FF, the operands are reversed: ST(i), ST(0)
            var stiOperand = OperandFactory.CreateFPURegisterOperand((FpuRegisterIndex)rm); // ST(i)
            var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0); // ST(0)
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                stiOperand,
                st0Operand
            ];
        }
        
        return true;
    }
}
