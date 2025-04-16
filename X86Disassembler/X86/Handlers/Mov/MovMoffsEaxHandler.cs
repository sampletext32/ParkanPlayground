using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV moffs32, EAX instruction (0xA3) and MOV moffs8, AL instruction (0xA2)
/// </summary>
public class MovMoffsEaxHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovMoffsEaxHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovMoffsEaxHandler(InstructionDecoder decoder)
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
        return opcode == 0xA2 || opcode == 0xA3;
    }

    /// <summary>
    /// Decodes a MOV moffs32, EAX or MOV moffs8, AL instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mov;

        // Get the operand size based on the opcode
        int operandSize = opcode == 0xA2 ? 8 : 32;

        // Read the memory offset
        // Fixed bug: Changed from if (Decoder.CanReadUInt()) to if (!Decoder.CanReadUInt())
        if (!Decoder.CanReadUInt())
        {
            return false;
        }

        uint offset = Decoder.ReadUInt32();

        // Create the destination memory operand
        // For MOV moffs32, EAX or MOV moffs8, AL, the memory operand is a direct memory reference
        var destinationOperand = OperandFactory.CreateDirectMemoryOperand(offset, operandSize);
        
        // Create the source register operand (EAX or AL)
        var sourceOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, operandSize);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}