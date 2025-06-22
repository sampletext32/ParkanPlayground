namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FINIT instruction with WAIT prefix (0x9B 0xDB 0xE3) - Initialize FPU after checking for pending unmasked floating-point exceptions
/// </summary>
public class FinitHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FinitHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FinitHandler(InstructionDecoder decoder)
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
        // FINIT with WAIT prefix starts with 0x9B
        if (opcode != 0x9B) return false;

        // Check if we can read the next two bytes
        if (!Decoder.CanReadByte())
            return false;

        // Check if the next bytes are 0xDB 0xE3 (for FINIT with WAIT)
        var (nextByte, thirdByte) = Decoder.PeakTwoBytes();

        // The sequence must be 9B DB E3 for FINIT with WAIT
        return nextByte == 0xDB && thirdByte == 0xE3;
    }
    
    /// <summary>
    /// Decodes a FINIT instruction with WAIT prefix
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
            
        // Read the third byte (0xE3)
        if (!Decoder.CanReadByte())
            return false;

        byte thirdByte = Decoder.ReadByte();
        if (thirdByte != 0xE3)
            return false;
        
        // Set the instruction type
        instruction.Type = InstructionType.Finit;
        
        // FINIT has no operands
        instruction.StructuredOperands = [];
        
        return true;
    }
}
