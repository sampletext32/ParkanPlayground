namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FISUBR int32 instruction (DA /5)
/// </summary>
public class FisubrInt32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FisubrInt32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FisubrInt32Handler(InstructionDecoder decoder)
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
        // FISUBR is DA /5
        if (opcode != 0xDA) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 5
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3)
        return reg == 5 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FISUBR int32 instruction
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
        var (mod, reg, rm, rawOperand) = ModRMDecoder.ReadModRM();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fisubr;

        // Set the structured operands - the operand already has the correct size from ReadModRM
        instruction.StructuredOperands = 
        [
            rawOperand
        ];

        return true;
    }
}
