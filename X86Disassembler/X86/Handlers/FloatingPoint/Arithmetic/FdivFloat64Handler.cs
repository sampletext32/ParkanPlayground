namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FDIV float64 instruction (DC /6)
/// </summary>
public class FdivFloat64Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FdivFloat64Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FdivFloat64Handler(InstructionDecoder decoder)
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
        // FDIV is DC /6
        if (opcode != 0xDC) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 6 and mod != 3 (memory operand)
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3) with reg = 6
        return reg == 6 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FDIV float64 instruction
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

        // Read the ModR/M byte using the specialized FPU method for 64-bit operands
        var (mod, reg, fpuRm, rawOperand) = ModRMDecoder.ReadModRMFpu64();
        
        // We've already verified reg field is 6 (FDIV) in CanHandle
        // and we only handle memory operands (mod != 3)
        
        // Set the instruction type
        instruction.Type = InstructionType.Fdiv;

        // Set the structured operands - the operand already has the correct size from ReadModRMFpu64
        instruction.StructuredOperands = 
        [
            rawOperand
        ];

        return true;
    }
}
