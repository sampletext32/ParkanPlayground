namespace X86Disassembler.X86.Handlers.Call;

using Operands;

/// <summary>
/// Handler for CALL rel32 instruction (0xE8)
/// </summary>
public class CallRel32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CallRel32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CallRel32Handler(InstructionDecoder decoder)
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
        return opcode == 0xE8;
    }

    /// <summary>
    /// Decodes a CALL rel32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Call;

        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        int position = Decoder.GetPosition();

        // Read the relative offset
        uint offset = Decoder.ReadUInt32();

        // Calculate the target address
        uint targetAddress = (uint) (position + offset + 4);

        // Create the target address operand
        var targetOperand = OperandFactory.CreateRelativeOffsetOperand(targetAddress);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            targetOperand
        ];

        return true;
    }
}