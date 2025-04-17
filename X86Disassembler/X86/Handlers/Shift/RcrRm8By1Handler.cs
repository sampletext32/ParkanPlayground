using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Shift;

/// <summary>
/// Handler for RCR r/m8, 1 instruction (0xD0 /3)
/// </summary>
public class RcrRm8By1Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RcrRm8By1Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RcrRm8By1Handler(InstructionDecoder decoder)
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
        // RCR r/m8, 1 is encoded as 0xD0 /3
        if (opcode != 0xD0)
            return false;

        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the reg field of the ModR/M byte is 3 (RCR)
        var reg = ModRMDecoder.PeakModRMReg();
        return reg == 3; // 3 = RCR
    }

    /// <summary>
    /// Decodes a RCR r/m8, 1 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Rcr;

        // Read the ModR/M byte
        var (_, _, _, operand) = ModRMDecoder.ReadModRM8();

        // Create an immediate operand for the constant 1
        var immOperand = OperandFactory.CreateImmediateOperand(1);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            operand,
            immOperand
        ];

        return true;
    }
}
