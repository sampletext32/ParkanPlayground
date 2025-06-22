using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Shift;

/// <summary>
/// Handler for SAR r/m32, 1 instruction (0xD1 /7)
/// </summary>
public class SarRm32By1Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SarRm32By1Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SarRm32By1Handler(InstructionDecoder decoder)
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
        // SAR r/m32, 1 is encoded as 0xD1 /7
        if (opcode != 0xD1)
            return false;

        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the reg field of the ModR/M byte is 7 (SAR)
        var reg = ModRMDecoder.PeakModRMReg();
        return reg == 7; // 7 = SAR
    }

    /// <summary>
    /// Decodes a SAR r/m32, 1 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sar;

        // Read the ModR/M byte
        var (_, _, _, operand) = ModRMDecoder.ReadModRM();

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
