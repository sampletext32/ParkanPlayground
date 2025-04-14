using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Lea;

/// <summary>
/// Handler for LEA r32, m instruction (0x8D)
/// </summary>
public class LeaR32MHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the LeaR32MHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public LeaR32MHandler(InstructionDecoder decoder)
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
        return opcode == 0x8D;
    }

    /// <summary>
    /// Decodes a LEA r32, m instruction
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
        var (mod, reg, rm, sourceOperand) = ModRMDecoder.ReadModRM();

        // LEA only works with memory operands, not registers
        if (mod == 3)
        {
            return false;
        }

        // Set the instruction type
        instruction.Type = InstructionType.Lea;
        
        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand((RegisterIndex)reg, 32);
        
        // For LEA, we don't care about the size of the memory operand
        // as we're only interested in the effective address calculation
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}