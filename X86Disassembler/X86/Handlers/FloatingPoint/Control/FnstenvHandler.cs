namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FNSTENV instruction (D9 /6)
/// </summary>
public class FnstenvHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FnstenvHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FnstenvHandler(InstructionDecoder decoder)
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
        // FNSTENV is D9 /6
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 6
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3)
        return reg == 6 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FNSTENV instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, rawOperand) = ModRMDecoder.ReadModRM();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fnstenv;

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            rawOperand
        ];

        return true;
    }
}
