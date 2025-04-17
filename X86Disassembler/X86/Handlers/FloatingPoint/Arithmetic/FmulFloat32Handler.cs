using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

/// <summary>
/// Handler for FMUL float32 instruction (D8 /1)
/// </summary>
public class FmulFloat32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FmulFloat32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FmulFloat32Handler(InstructionDecoder decoder)
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
        // FMUL is D8 /1
        if (opcode != 0xD8) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 1 and mod != 3 (memory operand)
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3) with reg = 1
        return reg == 1 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FMUL float32 instruction
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

        // Read the ModR/M byte using the specialized FPU method for 32-bit operands
        var (mod, reg, fpuRm, rawOperand) = ModRMDecoder.ReadModRMFpu();
        
        // We've already verified reg field is 1 (FMUL) in CanHandle
        // and we only handle memory operands (mod != 3)
        
        // Set the instruction type
        instruction.Type = InstructionType.Fmul;

        // Set the structured operands - the operand already has the correct size from ReadModRMFpu
        instruction.StructuredOperands = 
        [
            rawOperand
        ];

        return true;
    }
}
