namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for RDTSC instruction (0x0F 0x31)
/// </summary>
public class RdtscHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the RdtscHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public RdtscHandler(InstructionDecoder decoder)
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
        // RDTSC is encoded as 0x0F 0x31
        if (opcode != 0x0F)
            return false;

        // Check if we can read the second byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the second byte is 0x31
        byte secondByte = Decoder.PeakByte();
        return secondByte == 0x31;
    }

    /// <summary>
    /// Decodes a RDTSC instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Rdtsc;
        
        // Read and discard the second byte (0x31)
        if (!Decoder.CanReadByte())
            return false;
            
        Decoder.ReadByte();
        
        // RDTSC has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
