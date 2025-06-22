namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FSTP instruction with DF opcode (DF D0-D8)
/// </summary>
public class FstpDfHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FstpDfHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FstpDfHandler(InstructionDecoder decoder)
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
        // FSTP with DF opcode is DF D0 or DF D8
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte is D0 (reg = 2, rm = 0, mod = 3) or D8 (reg = 3, rm = 0, mod = 3)
        byte modRm = Decoder.PeakByte();
        return modRm == 0xD0 || modRm == 0xD8;
    }
    
    /// <summary>
    /// Decodes a FSTP instruction with DF opcode
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
        instruction.Type = InstructionType.Fstp;

        // Create the FPU register operand
        // For both D0 and D8, the operand is ST1
        var fpuRegisterOperand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST1);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            fpuRegisterOperand
        ];

        return true;
    }
}
