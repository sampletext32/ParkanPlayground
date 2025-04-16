using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point operations on float32 (D8 opcode)
/// </summary>
public class Float32OperationHandler : InstructionHandler
{
    // D8 opcode - operations on float32
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
    /// Initializes a new instance of the Float32OperationHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Float32OperationHandler(InstructionDecoder decoder)
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
        return opcode == 0xD8;
    }

    /// <summary>
    /// Decodes a floating-point instruction for float32 operations
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
        var (mod, reg, rm, rawOperand) = ModRMDecoder.ReadModRM();

        // Set the instruction type based on the reg field
        instruction.Type = InstructionTypes[(int)reg];

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            // Create a new memory operand with 32-bit size using the appropriate factory method
            Operand operand;
            
            if (rawOperand is DirectMemoryOperand directMemory)
            {
                operand = OperandFactory.CreateDirectMemoryOperand(directMemory.Address, 32);
            }
            else if (rawOperand is BaseRegisterMemoryOperand baseRegMemory)
            {
                operand = OperandFactory.CreateBaseRegisterMemoryOperand(baseRegMemory.BaseRegister, 32);
            }
            else if (rawOperand is DisplacementMemoryOperand dispMemory)
            {
                operand = OperandFactory.CreateDisplacementMemoryOperand(dispMemory.BaseRegister, dispMemory.Displacement, 32);
            }
            else if (rawOperand is ScaledIndexMemoryOperand scaledMemory)
            {
                operand = OperandFactory.CreateScaledIndexMemoryOperand(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement, 32);
            }
            else
            {
                operand = rawOperand;
            }
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                operand
            ];
        }
        else // Register operand (ST(i))
        {
            // For register operands, we need to handle the stack registers
            var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0); // ST(0)
            var stiOperand = OperandFactory.CreateFPURegisterOperand((FpuRegisterIndex)rm); // ST(i)
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                st0Operand,
                stiOperand
            ];
        }

        return true;
    }
}