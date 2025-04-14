namespace X86Disassembler.X86.Handlers.Test;

using Operands;

/// <summary>
/// Handler for TEST EAX, imm32 instruction (0xA9)
/// </summary>
public class TestEaxImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestEaxImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public TestEaxImmHandler(InstructionDecoder decoder)
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
        return opcode == 0xA9;
    }

    /// <summary>
    /// Decodes a TEST EAX, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Test;

        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value - x86 is little-endian, so we need to read the bytes in the correct order
        var imm32 = Decoder.ReadUInt32();

        // Create the register operand for EAX
        var eaxOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A);
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            eaxOperand,
            immOperand
        ];

        return true;
    }
}