namespace X86Disassembler.X86.Handlers.And;

using Operands;

/// <summary>
/// Handler for AND r/m32, imm32 instruction (0x81 /4)
/// </summary>
public class AndImmToRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AndImmToRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AndImmToRm32Handler(InstructionDecoder decoder)
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
        if (opcode != 0x81)
            return false;

        // Check if the reg field of the ModR/M byte is 4 (AND)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 4; // 4 = AND
    }

    /// <summary>
    /// Decodes an AND r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.And;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (_, _, _, destOperand) = ModRMDecoder.ReadModRM();

        // Read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value in little-endian format
        var imm = Decoder.ReadUInt32();

        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            immOperand
        ];

        return true;
    }
}