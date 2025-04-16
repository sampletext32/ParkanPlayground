namespace X86Disassembler.X86.Handlers.Or;

using Operands;

/// <summary>
/// Handler for OR r/m32, imm32 instruction (0x81 /1)
/// </summary>
public class OrImmToRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrImmToRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public OrImmToRm32Handler(InstructionDecoder decoder)
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

        // Check if the reg field of the ModR/M byte is 1 (OR)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 1; // 1 = OR
    }

    /// <summary>
    /// Decodes an OR r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Or;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (_, _, _, destOperand) = ModRMDecoder.ReadModRM();

        // Check if we can read the immediate value
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        // Read the immediate value
        uint imm32 = Decoder.ReadUInt32();
        
        // Create the immediate operand
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            immOperand
        ];

        return true;
    }
}