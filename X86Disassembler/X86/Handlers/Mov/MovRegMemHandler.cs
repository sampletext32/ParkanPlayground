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
        return opcode == 0x8A || opcode == 0x8B;
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
        var (_, reg, _, sourceOperand) = ModRMDecoder.ReadModRM();

        // Adjust the operand size based on the opcode
        sourceOperand.Size = operandSize;

        // Create the destination register operand
        var destinationOperand = OperandFactory.CreateRegisterOperand(reg, operandSize);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}