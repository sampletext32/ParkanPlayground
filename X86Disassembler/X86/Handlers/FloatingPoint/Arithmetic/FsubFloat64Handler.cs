namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FSUB float64 instruction (DC /4)
/// </summary>
public class FsubFloat64Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FsubFloat64Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FsubFloat64Handler(InstructionDecoder decoder)
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
        // FSUB is DC /4
        if (opcode != 0xDC) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 4
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        
        return reg == 4;
    }
    
    /// <summary>
    /// Decodes a FSUB float64 instruction
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

        // Read the ModR/M byte using the specialized FPU method
        var (mod, reg, fpuRm, rawOperand) = ModRMDecoder.ReadModRMFpu();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fsub;

        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            // Set the structured operands - the operand already has the correct size from ReadModRM
            instruction.StructuredOperands = 
            [
                rawOperand
            ];
        }
        else // Register operand (ST(i))
        {
            // For DC E0-DC E7, the operands are reversed: ST(i), ST(0)
            var stiOperand = OperandFactory.CreateFPURegisterOperand(fpuRm); // ST(i)
            var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0); // ST(0)
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                stiOperand,
                st0Operand
            ];
        }

        return true;
    }
}
