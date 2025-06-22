namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FILD int64 instruction (DF /5)
/// </summary>
public class FildInt64Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FildInt64Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FildInt64Handler(InstructionDecoder decoder)
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
        // FILD int64 is DF /5
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 5
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3) with reg = 5
        return reg == 5 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FILD int64 instruction
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
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM64();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fild;

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            operand
        ];

        return true;
    }
}
