using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m16, imm16 instruction (0x81 /5 with 0x66 prefix)
/// </summary>
public class SubImmFromRm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public SubImmFromRm16Handler(InstructionDecoder decoder)
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
        // Check if the opcode is 0x81 and we have a 0x66 prefix
        if (opcode != 0x81 || !Decoder.HasOperandSizeOverridePrefix())
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the reg field is 5 (SUB)
        byte modRM = Decoder.PeakByte();
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m16, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Sub;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For SUB r/m16, imm16 (0x81 /5 with 0x66 prefix):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();

        // Adjust the operand size to 16-bit
        destinationOperand.Size = 16;

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the immediate value (16-bit)
        ushort immediate = Decoder.ReadUInt16();

        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(immediate, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}