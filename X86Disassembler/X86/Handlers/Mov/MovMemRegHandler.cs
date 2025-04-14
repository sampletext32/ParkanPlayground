using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m32, r32 instruction (0x89) and MOV r/m8, r8 instruction (0x88)
/// </summary>
public class MovMemRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovMemRegHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovMemRegHandler(InstructionDecoder decoder)
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
        return opcode == 0x88 || opcode == 0x89;
    }

    /// <summary>
    /// Decodes a MOV r/m32, r32 or MOV r/m8, r8 instruction
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
        bool operandSize32 = (opcode & 0x01) != 0;
        int operandSize = operandSize32 ? 32 : 8;

        // Read the ModR/M byte
        // For MOV r/m32, r32 (0x89) or MOV r/m8, r8 (0x88):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The reg field specifies the source register
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();

        // Adjust the operand size based on the opcode
        destinationOperand.Size = operandSize;

        // Create the source register operand
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, operandSize);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}