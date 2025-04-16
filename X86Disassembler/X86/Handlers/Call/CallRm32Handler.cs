using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Call;

/// <summary>
/// Handler for CALL r/m32 instruction (FF /2)
/// </summary>
public class CallRm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CallRm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CallRm32Handler(InstructionDecoder decoder)
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
        // CALL r/m32 is encoded as FF /2
        if (opcode != 0xFF)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Extract the reg field (bits 3-5)
        var reg = ModRMDecoder.PeakModRMReg();
        
        // CALL r/m32 is encoded as FF /2 (reg field = 2)
        // Only handle when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return reg == 2 && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes a CALL r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Call;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For CALL r/m32 (FF /2):
        // - The r/m field with mod specifies the operand (register or memory)
        var (_, _, _, operand) = ModRMDecoder.ReadModRM();

        // Set the structured operands
        // CALL has only one operand
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}