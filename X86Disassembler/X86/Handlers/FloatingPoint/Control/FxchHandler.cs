namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FXCH instruction (D9 /1 with mod=3)
/// </summary>
public class FxchHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FxchHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FxchHandler(InstructionDecoder decoder)
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
        // FXCH is D9 /1 with mod=3
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 1 and mod = 3
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle register operands (mod = 3) with reg = 1
        return reg == 1 && mod == 3;
    }
    
    /// <summary>
    /// Decodes a FXCH instruction
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
        var (mod, reg, rm, _) = ModRMDecoder.ReadModRM();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fxch;

        // Map rm field to FPU register index
        FpuRegisterIndex stIndex = rm switch
        {
            RegisterIndex.A => FpuRegisterIndex.ST0,
            RegisterIndex.C => FpuRegisterIndex.ST1,
            RegisterIndex.D => FpuRegisterIndex.ST2,
            RegisterIndex.B => FpuRegisterIndex.ST3,
            RegisterIndex.Sp => FpuRegisterIndex.ST4,
            RegisterIndex.Bp => FpuRegisterIndex.ST5,
            RegisterIndex.Si => FpuRegisterIndex.ST6,
            RegisterIndex.Di => FpuRegisterIndex.ST7,
            _ => FpuRegisterIndex.ST0 // Default case, should not happen
        };
        
        // Create the FPU register operand
        var operand = OperandFactory.CreateFPURegisterOperand(stIndex);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
