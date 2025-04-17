namespace X86Disassembler.X86.Handlers.FloatingPoint.Transcendental;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FPATAN instruction (D9 F3)
/// </summary>
public class FpatanHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FpatanHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FpatanHandler(InstructionDecoder decoder)
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
        // FPATAN is D9 F3
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the next byte is F3
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xF3;
    }
    
    /// <summary>
    /// Decodes a FPATAN instruction
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

        // Read the second byte of the opcode
        byte secondByte = Decoder.ReadByte();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fpatan;

        // FPATAN has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
