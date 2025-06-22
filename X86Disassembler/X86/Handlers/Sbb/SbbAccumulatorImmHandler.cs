namespace X86Disassembler.X86.Handlers.Sbb;

using Operands;

/// <summary>
/// Handler for SBB AX/EAX, imm16/32 instruction (0x1D)
/// </summary>
public class SbbAccumulatorImmHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SbbAccumulatorImmHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SbbAccumulatorImmHandler(InstructionDecoder decoder)
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
        return opcode == 0x1D;
    }

    /// <summary>
    /// Decodes a SBB AX/EAX, imm16/32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sbb;

        // Determine operand size based on prefix
        int operandSize = Decoder.HasOperandSizePrefix() ? 16 : 32;
        
        // Check if we have enough bytes for the immediate value
        if (operandSize == 16 && !Decoder.CanReadUShort())
        {
            return false;
        }
        else if (operandSize == 32 && !Decoder.CanReadUInt())
        {
            return false;
        }

        // Create the accumulator register operand (AX or EAX)
        var accumulatorOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, operandSize);

        // Read and create the immediate operand based on operand size
        var immOperand = operandSize == 16 
            ? OperandFactory.CreateImmediateOperand(Decoder.ReadUInt16(), operandSize)
            : OperandFactory.CreateImmediateOperand(Decoder.ReadUInt32(), operandSize);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            accumulatorOperand,
            immOperand
        ];

        return true;
    }
}
