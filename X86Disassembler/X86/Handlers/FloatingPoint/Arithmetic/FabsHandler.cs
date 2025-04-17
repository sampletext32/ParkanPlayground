namespace X86Disassembler.X86.Handlers.FloatingPoint.Arithmetic;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FABS instruction (D9 E1)
/// </summary>
public class FabsHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FabsHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FabsHandler(InstructionDecoder decoder)
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
        // FABS is D9 E1
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the next byte is E1
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xE1;
    }
    
    /// <summary>
    /// Decodes a FABS instruction
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
        instruction.Type = InstructionType.Fabs;

        // FABS has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
