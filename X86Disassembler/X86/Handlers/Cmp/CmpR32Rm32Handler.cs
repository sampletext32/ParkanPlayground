using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r32, r/m32 instruction (0x3B)
/// </summary>
public class CmpR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpR32Rm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CmpR32Rm32Handler(InstructionDecoder decoder)
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
        return opcode == 0x3B;
    }

    /// <summary>
    /// Decodes a CMP r32, r/m32 instruction
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
        instruction.Type = InstructionType.Cmp;

        // Read the ModR/M byte
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM();
        
        // Create the destination register operand (32-bit)
        var destinationOperand = OperandFactory.CreateRegisterOperand((RegisterIndex)reg, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
