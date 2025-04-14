using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r16, r/m16 instruction (0x2B with 0x66 prefix)
/// </summary>
public class SubR16Rm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubR16Rm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubR16Rm16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x2B and we have a 0x66 prefix
        return opcode == 0x2B && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB r16, r/m16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, sourceOperand) = ModRMDecoder.ReadModRM();
        
        // Ensure the source operand has the correct size (16-bit)
        sourceOperand.Size = 16;
        
        // Create the destination register operand (16-bit)
        var destinationOperand = OperandFactory.CreateRegisterOperand((RegisterIndex)reg, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}