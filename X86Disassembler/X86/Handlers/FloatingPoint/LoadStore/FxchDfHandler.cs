namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FXCH instruction with DF opcode (DF C8)
/// </summary>
public class FxchDfHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FxchDfHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FxchDfHandler(InstructionDecoder decoder)
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
        // FXCH with DF opcode is DF C8
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte is exactly C8 (reg = 1, rm = 0, mod = 3)
        byte modRm = Decoder.PeakByte();
        return modRm == 0xC8;
    }
    
    /// <summary>
    /// Decodes a FXCH instruction with DF opcode
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

        // Create the FPU register operand
        var fpuRegisterOperand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            fpuRegisterOperand
        ];

        return true;
    }
}
