using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV EAX, moffs32 instruction (0xA1) and MOV AL, moffs8 instruction (0xA0)
/// </summary>
public class MovEaxMoffsHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovEaxMoffsHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovEaxMoffsHandler(InstructionDecoder decoder)
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
        return opcode == 0xA0 || opcode == 0xA1;
    }

    /// <summary>
    /// Decodes a MOV EAX, moffs32 or MOV AL, moffs8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mov;

        // Get the operand size based on the opcode
        int operandSize = (opcode == 0xA0) ? 8 : 32;

        // Read the memory offset
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        uint offset = Decoder.ReadUInt32();

        // Create the destination register operand (EAX or AL)
        var destinationOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, operandSize);
        
        // Create the source memory operand
        // For MOV EAX, moffs32 or MOV AL, moffs8, the memory operand is a direct memory reference
        var sourceOperand = OperandFactory.CreateDirectMemoryOperand(offset, operandSize);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}