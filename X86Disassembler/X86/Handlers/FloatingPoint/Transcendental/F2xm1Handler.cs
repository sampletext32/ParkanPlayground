namespace X86Disassembler.X86.Handlers.FloatingPoint.Transcendental;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for F2XM1 instruction (D9 F0)
/// </summary>
public class F2xm1Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the F2xm1Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public F2xm1Handler(InstructionDecoder decoder)
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
        // F2XM1 is D9 F0
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the next byte is F0
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xF0;
    }
    
    /// <summary>
    /// Decodes a F2XM1 instruction
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
        instruction.Type = InstructionType.F2xm1;

        // F2XM1 has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
