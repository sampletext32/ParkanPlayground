namespace X86Disassembler.X86.Handlers.Jump;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for conditional jump instructions (0x70-0x7F)
/// </summary>
public class ConditionalJumpHandler : InstructionHandler
{
    // Mnemonics for conditional jumps
    private static readonly string[] Mnemonics =
    [
        "jo", "jno", "jb", "jnb", "jz", "jnz", "jbe", "jnbe",
        "js", "jns", "jp", "jnp", "jl", "jnl", "jle", "jnle"
    ];

    // Instruction types for conditional jumps
    private static readonly InstructionType[] InstructionTypes =
    [
        InstructionType.Jo, InstructionType.Jno, InstructionType.Jb, InstructionType.Jae, 
        InstructionType.Jz, InstructionType.Jnz, InstructionType.Jbe, InstructionType.Ja,
        InstructionType.Js, InstructionType.Jns, InstructionType.Unknown, InstructionType.Unknown, 
        InstructionType.Jl, InstructionType.Jge, InstructionType.Jle, InstructionType.Jg
    ];

    /// <summary>
    /// Initializes a new instance of the ConditionalJumpHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ConditionalJumpHandler(InstructionDecoder decoder) 
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
        // Conditional jumps are in the range 0x70-0x7F
        return opcode >= 0x70 && opcode <= 0x7F;
    }
    
    /// <summary>
    /// Decodes a conditional jump instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Get the index from the opcode
        int index = opcode - 0x70;
        
        // Set the instruction type
        instruction.Type = InstructionTypes[index];
        
        // Check if we can read the offset byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the offset and calculate target address
        int position = Decoder.GetPosition();
        sbyte offset = (sbyte)Decoder.ReadByte();
        int targetAddress = position + 1 + offset;
        
        // Create the target address operand
        var targetOperand = OperandFactory.CreateRelativeOffsetOperand((ulong)targetAddress, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            targetOperand
        ];
        
        return true;
    }
}
