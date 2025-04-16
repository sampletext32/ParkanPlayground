using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r32, r/m32 instruction (0x8B) and MOV r8, r/m8 instruction (0x8A)
/// </summary>
public class MovRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRegMemHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovRegMemHandler(InstructionDecoder decoder)
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
        // For 8-bit operations (0x8A), no prefix check needed
        if (opcode == 0x8A)
            return true;
            
        // For 32-bit operations (0x8B), only handle when operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return opcode == 0x8B && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a MOV r32, r/m32 or MOV r8, r/m8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mov;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Determine operand size (0 = 8-bit, 1 = 32-bit)
        int operandSize = (opcode & 0x01) != 0 ? 32 : 8;

        // Use ModRMDecoder to decode the ModR/M byte
        // For MOV r32, r/m32 (0x8B) or MOV r8, r/m8 (0x8A):
        // - The reg field specifies the destination register
        // - The r/m field with mod specifies the source operand (register or memory)
        Operand sourceOperand;
        Operand destinationOperand;
        
        if (operandSize == 8)
        {
            // For 8-bit operands, use the 8-bit ModR/M decoder and factory methods
            var (_, reg8, _, srcOperand8) = ModRMDecoder.ReadModRM8();
            sourceOperand = srcOperand8;
            destinationOperand = OperandFactory.CreateRegisterOperand8(reg8);
        }
        else
        {
            // For 32-bit operands, use the standard ModR/M decoder
            var (_, regStd, _, srcOperandStd) = ModRMDecoder.ReadModRM();
            sourceOperand = srcOperandStd;
            destinationOperand = OperandFactory.CreateRegisterOperand(regStd, operandSize);
        }
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}