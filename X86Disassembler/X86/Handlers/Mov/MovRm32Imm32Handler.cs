namespace X86Disassembler.X86.Handlers.Mov;

using Operands;

/// <summary>
/// Handler for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRm32Imm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MovRm32Imm32Handler(InstructionDecoder decoder)
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
        if (opcode != 0xC7)
        {
            return false;
        }

        // Then check if we can peek at the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Peak at the ModR/M byte without advancing the position
        var reg = ModRMDecoder.PeakModRMReg();
        
        // MOV r/m8, imm8 only uses reg=0
        return reg == 0;
    }

    /// <summary>
    /// Decodes a MOV r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Mov;
        
        // Read the ModR/M byte
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Check if we have enough bytes for the immediate value (4 bytes)
        if (!Decoder.CanReadUInt())
        {
            return false;
        }
        
        // Read the immediate dword and create the operands
        uint imm32 = Decoder.ReadUInt32();
        
        // Create the immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
