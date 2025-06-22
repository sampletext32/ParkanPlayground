namespace X86Disassembler.X86.Handlers.FloatingPoint.Transcendental;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FYL2X instruction (D9 F1)
/// </summary>
public class Fyl2xHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Fyl2xHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public Fyl2xHandler(InstructionDecoder decoder)
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
        // FYL2X is D9 F1
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the next byte is F1
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xF1;
    }
    
    /// <summary>
    /// Decodes a FYL2X instruction
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
        instruction.Type = InstructionType.Fyl2x;

        // FYL2X has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
