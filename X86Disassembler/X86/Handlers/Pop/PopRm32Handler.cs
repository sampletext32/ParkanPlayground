using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Pop;

/// <summary>
/// Handler for POP r/m32 instruction (0x8F /0)
/// </summary>
public class PopRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the PopRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public PopRm32Handler(InstructionDecoder decoder)
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
        // POP r/m32 is encoded as 8F /0
        if (opcode != 0x8F)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        var reg = ModRMDecoder.PeakModRMReg();
        
        // POP r/m32 is encoded as 8F /0 (reg field = 0)
        // Only handle when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return reg == 0 && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a POP r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Pop;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For POP r/m32 (8F /0):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, _, _, operand) = ModRMDecoder.ReadModRM();

        // Set the structured operands
        // POP has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
