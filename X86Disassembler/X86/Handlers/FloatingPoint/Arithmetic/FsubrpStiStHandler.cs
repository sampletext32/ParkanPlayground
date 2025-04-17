namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FSUBRP ST(i), ST instruction (DE E8-EF)
/// </summary>
public class FsubrpStiStHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FsubrpStiStHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FsubrpStiStHandler(InstructionDecoder decoder)
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
        // FSUBRP ST(i), ST is DE E8-EF
        if (opcode != 0xDE) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check second opcode byte
        byte secondOpcode = Decoder.PeakByte();
        
        // Only handle E8-EF
        return secondOpcode is >= 0xE8 and <= 0xEF;
    }
    
    /// <summary>
    /// Decodes a FSUBRP ST(i), ST instruction
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

        // Read the ModR/M byte and calculate ST(i) index
        var stIndex = (FpuRegisterIndex)(Decoder.ReadByte() - 0xE8);
        
        // Set the instruction type
        instruction.Type = InstructionType.Fsubrp;
        
        // Create the FPU register operands
        var stiOperand = OperandFactory.CreateFPURegisterOperand(stIndex);
        var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            stiOperand,
            st0Operand
        ];

        return true;
    }
}
