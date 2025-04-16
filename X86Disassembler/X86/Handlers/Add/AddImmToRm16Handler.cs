using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m16, imm16 instruction (opcode 81 /0 with 0x66 prefix)
/// </summary>
public class AddImmToRm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddImmToRm16Handler(InstructionDecoder decoder) 
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
        // ADD r/m16, imm16 is encoded as 0x81 with 0x66 prefix
        if (opcode != 0x81)
        {
            return false;
        }

        // Only handle when the operand size prefix is present
        if (!Decoder.HasOperandSizePrefix())
            return false;
            
        // Check if the reg field of the ModR/M byte is 0 (ADD)
        if (!Decoder.CanReadByte())
            return false;

        var reg = ModRMDecoder.PeakModRMReg();

        return reg == 0; // 0 = ADD
    }

    /// <summary>
    /// Decodes a ADD r/m16, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        // Check if we can read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (_, _, _, destOperand) = ModRMDecoder.ReadModRM16();

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the immediate value
        ushort imm16 = Decoder.ReadUInt16();

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            OperandFactory.CreateImmediateOperand(imm16)
        ];

        return true;
    }
}
