namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FNCLEX instruction (0xDB 0xE2) - Clears floating-point exception flags without checking for pending unmasked exceptions
/// </summary>
public class FnclexHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FnclexHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FnclexHandler(InstructionDecoder decoder)
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
        // FNCLEX is DB E2
        if (opcode != 0xDB) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the next byte is E2
        byte nextByte = Decoder.PeakByte();
        return nextByte == 0xE2;
    }
    
    /// <summary>
    /// Decodes a FNCLEX instruction
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
        instruction.Type = InstructionType.Fnclex;

        // FCLEX has no operands
        instruction.StructuredOperands = [];

        return true;
    }
}
