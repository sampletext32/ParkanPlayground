namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FADD ST, ST(i) instruction (D8 C0-C7)
/// </summary>
public class FaddStStiHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FaddStStHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FaddStStiHandler(InstructionDecoder decoder)
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
        // FADD ST, ST(i) is D8 C0-C7
        if (opcode != 0xD8) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check second opcode byte
        byte secondOpcode = Decoder.PeakByte();
        
        // Only handle C0-C7
        return secondOpcode is >= 0xC0 and <= 0xC7;
    }
    
    /// <summary>
    /// Decodes a FADD ST, ST(i) instruction
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
        var stIndex = (FpuRegisterIndex)(Decoder.ReadByte() - 0xC0);
        
        // Set the instruction type
        instruction.Type = InstructionType.Fadd;
        
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
