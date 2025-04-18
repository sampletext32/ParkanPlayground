namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FCLEX instruction with WAIT prefix (0x9B 0xDB 0xE2) - Clears floating-point exception flags after checking for pending unmasked floating-point exceptions
/// </summary>
public class FclexWaitHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FclexWaitHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FclexWaitHandler(InstructionDecoder decoder)
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
        // FCLEX with WAIT prefix starts with 0x9B
        if (opcode != 0x9B) return false;

        // Check if we can read the next two bytes
        if (!Decoder.CanReadByte())
            return false;

        // Check if the next bytes are 0xDB 0xE2 (for FCLEX with WAIT)
        var (nextByte, thirdByte) = Decoder.PeakTwoBytes();

        // The sequence must be 9B DB E2 for FCLEX with WAIT
        return nextByte == 0xDB && thirdByte == 0xE2;
    }
    
    /// <summary>
    /// Decodes a FCLEX instruction with WAIT prefix
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Skip the WAIT prefix (0x9B) - we already read it in CanHandle
        if (!Decoder.CanReadByte())
            return false;

        // Read the second byte (0xDB)
        byte secondByte = Decoder.ReadByte();
        if (secondByte != 0xDB)
            return false;
            
        // Read the third byte (0xE2)
        if (!Decoder.CanReadByte())
            return false;

        byte thirdByte = Decoder.ReadByte();
        if (thirdByte != 0xE2)
            return false;
        
        // Set the instruction type
        instruction.Type = InstructionType.Fclex;
        
        // FCLEX has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
