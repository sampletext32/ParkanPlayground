using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m32, imm32 instruction (0xF7 /0)
/// </summary>
public class TestImmWithRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestImmWithRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TestImmWithRm32Handler(InstructionDecoder decoder)
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
        // This handler only handles opcode 0xF7
        if (opcode != 0xF7)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the reg field is 0 (TEST operation)
        var reg = ModRMDecoder.PeakModRMReg();
        
        return reg == 0; // 0 = TEST
    }

    /// <summary>
    /// Decodes a TEST r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Test;

        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        uint imm32 = Decoder.ReadUInt32();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm32, 32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}