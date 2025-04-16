using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m16, imm8 instruction (opcode 83 /0 with 0x66 prefix)
/// </summary>
public class AddImmToRm16SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm16SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddImmToRm16SignExtendedHandler(InstructionDecoder decoder) 
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
        // ADD r/m16, imm8 is encoded as 0x83 with 0x66 prefix
        if (opcode != 0x83)
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
    /// Decodes a ADD r/m16, imm8 instruction
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
        if (!Decoder.CanRead(1))
        {
            return false;
        }

        // Read the immediate value (sign-extended from 8-bit to 16-bit)
        sbyte imm8 = (sbyte)Decoder.ReadByte();
        short signExtendedImm = imm8;
        uint immValue = (ushort)signExtendedImm; // Convert to uint for the operand factory

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            OperandFactory.CreateImmediateOperand(immValue)
        ];

        return true;
    }
}
