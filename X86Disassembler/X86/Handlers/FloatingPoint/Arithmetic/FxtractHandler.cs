namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

/// <summary>
/// Handler for FXTRACT instruction (D9 F4) - Extracts the exponent and significand from the ST(0) value
/// </summary>
public class FxtractHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FxtractHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FxtractHandler(InstructionDecoder decoder)
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
        // FXTRACT is D9 F4
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
            return false;

        // Check if the next byte is F4
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xF4;
    }
    
    /// <summary>
    /// Decodes an FXTRACT instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        if (!Decoder.CanReadByte())
            return false;

        // Read the second byte of the opcode
        byte secondByte = Decoder.ReadByte();
        
        // Verify the opcode is correct
        if (secondByte != 0xF4)
            return false;
        
        // Set the instruction type
        instruction.Type = InstructionType.Fxtract;
        
        // FXTRACT has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
