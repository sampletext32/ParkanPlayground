namespace X86Disassembler.X86.Handlers.FloatingPoint.Comparison;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FCOM ST(i) instruction (D8 D0-D7) - compares ST(0) with ST(i)
/// </summary>
public class FcomStHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FcomStHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FcomStHandler(InstructionDecoder decoder)
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
        // FCOM ST(i) is D8 D0-D7 (compares ST(0) with ST(i))
        if (opcode != 0xD8) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        var opcodeSecond = Decoder.PeakByte();

        // this is a special case of a handler, only handling FCOM with ST(i)
        if (opcodeSecond < 0xD0 || opcodeSecond > 0xD7)
            return false;

        return true;
    }
    
    /// <summary>
    /// Decodes a FCOM ST(i) instruction - compares ST(0) with ST(i)
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
        
        var stIndex = (FpuRegisterIndex)(Decoder.ReadByte() - 0xD0);

        // Set the instruction type
        instruction.Type = InstructionType.Fcom;
        
        // Create the FPU register operands
        var st0Operand = OperandFactory.CreateFPURegisterOperand(FpuRegisterIndex.ST0);
        var stiOperand = OperandFactory.CreateFPURegisterOperand(stIndex);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            stiOperand  // The instruction is FCOM ST(i), which compares ST(0) with ST(i)
        ];

        return true;
    }
}
