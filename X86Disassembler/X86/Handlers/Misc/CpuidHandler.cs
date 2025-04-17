namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for CPUID instruction (0x0F 0xA2)
/// </summary>
public class CpuidHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CpuidHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CpuidHandler(InstructionDecoder decoder)
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
        // CPUID is encoded as 0x0F 0xA2
        if (opcode != 0x0F)
            return false;

        // Check if we can read the second byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the second byte is 0xA2
        byte secondByte = Decoder.PeakByte();
        return secondByte == 0xA2;
    }

    /// <summary>
    /// Decodes a CPUID instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Cpuid;
        
        // Read and discard the second byte (0xA2)
        if (!Decoder.CanReadByte())
            return false;
            
        Decoder.ReadByte();
        
        // CPUID has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
