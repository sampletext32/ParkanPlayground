using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m8, imm8 instruction (0xF6 /0)
/// </summary>
public class TestImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestImmWithRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TestImmWithRm8Handler(InstructionDecoder decoder)
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
        // This handler only handles opcode 0xF6
        if (opcode != 0xF6)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the reg field is 0 (TEST operation)
        byte modRM = Decoder.PeakByte();
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 0; // 0 = TEST
    }

    /// <summary>
    /// Decodes a TEST r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Test;
        
        // Read the ModR/M byte
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Ensure the destination operand has the correct size (8-bit)
        destinationOperand.Size = 8;

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        byte imm8 = Decoder.ReadByte();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}