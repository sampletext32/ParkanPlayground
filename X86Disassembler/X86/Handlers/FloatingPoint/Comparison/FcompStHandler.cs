namespace X86Disassembler.X86.Handlers.FloatingPoint.Comparison;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FCOMP ST(i) instruction (D8 D8-DF) - compares ST(0) with ST(i) and pops the register stack
/// </summary>
public class FcompStHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FcompStHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FcompStHandler(InstructionDecoder decoder)
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
        // FCOMP ST(i) is D8 D8-DF (compares ST(0) with ST(i) and pops the register stack)
        if (opcode != 0xD8) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        var opcodeSecond = Decoder.PeakByte();

        // this is a special case of a handler, only handling FCOMP with ST(i)
        if (opcodeSecond < 0xD8 || opcodeSecond > 0xDF)
            return false;

        return true;
    }
    
    /// <summary>
    /// Decodes a FCOMP ST(i) instruction - compares ST(0) with ST(i) and pops the register stack
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
        
        var stIndex = (FpuRegisterIndex)(Decoder.ReadByte() - 0xD8);

        // Set the instruction type
        instruction.Type = InstructionType.Fcomp;
        
        // Create the FPU register operands
        var stiOperand = OperandFactory.CreateFPURegisterOperand(stIndex);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            stiOperand  // The instruction is FCOMP ST(i), which compares ST(0) with ST(i) and pops the register stack
        ];

        return true;
    }
}
