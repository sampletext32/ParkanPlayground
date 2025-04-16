using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m32, r32 instruction (0x29)
/// </summary>
public class SubRm32R32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubRm32R32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubRm32R32Handler(InstructionDecoder decoder)
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
        // Only handle opcode 0x29 when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0x29 && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a SUB r/m32, r32 instruction
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

        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        // Read the ModR/M byte
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Create the source register operand (32-bit)
        var sourceOperand = OperandFactory.CreateRegisterOperand((RegisterIndex)reg, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}