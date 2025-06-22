namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FDIVR float64 instruction (DC /7)
/// </summary>
public class FdivrFloat64Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FdivrFloat64Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FdivrFloat64Handler(InstructionDecoder decoder)
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
        // FDIVR is DC /7
        if (opcode != 0xDC) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 7 and mod != 3 (memory operand)
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3) with reg = 7
        return reg == 7 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FDIVR float64 instruction
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
        
        // We've already verified reg field is 7 (FDIVR) in CanHandle
        // and we only handle memory operands (mod != 3)
        
        // Set the instruction type
        instruction.Type = InstructionType.Fdivr;

        // Set the structured operands - the operand already has the correct size from ReadModRMFpu64
        instruction.StructuredOperands = 
        [
            rawOperand
        ];

        return true;
    }
}
