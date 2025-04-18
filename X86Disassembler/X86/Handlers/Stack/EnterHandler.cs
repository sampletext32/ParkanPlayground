namespace X86Disassembler.X86.Handlers.Stack;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for ENTER instruction (0xC8)
/// Creates a stack frame for a procedure with nested procedures
/// </summary>
public class EnterHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the EnterHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public EnterHandler(InstructionDecoder decoder)
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
        // ENTER is 0xC8
        return opcode == 0xC8;
    }
    
    /// <summary>
    /// Decodes an ENTER instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // ENTER requires 3 bytes: 1 opcode + 2-byte size operand + 1-byte nesting level
        // Check if we can read the 16-bit size operand
        if (!Decoder.CanReadUShort())
        {
            return false;
        }

        // Read the size operand (16-bit immediate value)
        ushort size = Decoder.ReadUInt16();
        
        // Check if we can read the nesting level byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the nesting level (8-bit immediate value)
        byte nestingLevel = Decoder.ReadByte();
        
        // Set the instruction type
        instruction.Type = InstructionType.Enter;
        
        // Create immediate operands for size and nesting level
        var sizeOperand = OperandFactory.CreateImmediateOperand(size);
        var nestingLevelOperand = OperandFactory.CreateImmediateOperand(nestingLevel);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            sizeOperand,
            nestingLevelOperand
        ];

        return true;
    }
}
