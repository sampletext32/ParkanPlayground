namespace X86Disassembler.X86.Handlers.Push;

using Operands;

/// <summary>
/// Handler for PUSH imm8 instruction (0x6A)
/// </summary>
public class PushImm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PushImm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PushImm8Handler(InstructionDecoder decoder)
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
        return opcode == 0x6A;
    }

    /// <summary>
    /// Decodes a PUSH imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Push;

        if(!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value
        sbyte imm8 = (sbyte)Decoder.ReadByte();

        // Create an 8-bit immediate operand to ensure it's displayed without leading zeros
        var immOperand = new ImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            immOperand
        ];

        return true;
    }
}