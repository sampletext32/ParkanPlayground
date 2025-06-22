namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

/// <summary>
/// Handler for FPREM1 instruction (D9 F5) - Computes the IEEE-compliant partial remainder of ST(0) ÷ ST(1)
/// </summary>
public class Fprem1Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Fprem1Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Fprem1Handler(InstructionDecoder decoder)
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
        // FPREM1 is D9 F5
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
            return false;

        // Check if the next byte is F5
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xF5;
    }
    
    /// <summary>
    /// Decodes an FPREM1 instruction
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
        if (secondByte != 0xF5)
            return false;
        
        // Set the instruction type
        instruction.Type = InstructionType.Fprem1;
        
        // FPREM1 has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
