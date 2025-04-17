namespace X86Disassembler.X86.Handlers.FloatingPoint.Comparison;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FCOM ST(0), ST(i) instruction (D8 D0-D7)
/// </summary>
public class FcomSt0Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FcomSt0Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FcomSt0Handler(InstructionDecoder decoder)
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
        // FCOM ST(0), ST(i) is D8 D0-D7
        if (opcode != 0xD8) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 2 and mod = 3
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle register operands (mod = 3) with reg = 2
        return reg == 2 && mod == 3;
    }
    
    /// <summary>
    /// Decodes a FCOM ST(0), ST(i) instruction
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
        var (mod, reg, rm, _) = ModRMDecoder.ReadModRMFpu();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fcom;

        // Map rm field to FPU register index
        FpuRegisterIndex stIndex = rm switch
        {
            FpuRegisterIndex.ST0 => FpuRegisterIndex.ST0,
            FpuRegisterIndex.ST1 => FpuRegisterIndex.ST1,
            FpuRegisterIndex.ST2 => FpuRegisterIndex.ST2,
            FpuRegisterIndex.ST3 => FpuRegisterIndex.ST3,
            FpuRegisterIndex.ST4 => FpuRegisterIndex.ST4,
            FpuRegisterIndex.ST5 => FpuRegisterIndex.ST5,
            FpuRegisterIndex.ST6 => FpuRegisterIndex.ST6,
            FpuRegisterIndex.ST7 => FpuRegisterIndex.ST7,
            _ => FpuRegisterIndex.ST0 // Default case, should not happen
        };
        
        // Create the FPU register operands
        var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0);
        var stiOperand = OperandFactory.CreateFPURegisterOperand(stIndex);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            st0Operand,
            stiOperand
        ];

        return true;
    }
}
