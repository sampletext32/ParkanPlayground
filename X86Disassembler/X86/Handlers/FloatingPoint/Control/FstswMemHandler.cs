using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FSTSW m2byte instruction (0x9B 0xDD /7) - Store FPU status word with wait prefix to memory
/// </summary>
public class FstswMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FstswMemHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FstswMemHandler(InstructionDecoder decoder)
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
        // FSTSW m2byte starts with the WAIT prefix (0x9B)
        if (opcode != 0x9B) return false;

        // Check if we can read the next two bytes
        if (!Decoder.CanReadByte())
            return false;

        // Check if the next bytes are 0xDD followed by ModR/M with reg field = 7
        var (nextByte, modRM) = Decoder.PeakTwoBytes();

        // The first byte must be 0xDD for FSTSW m2byte
        if (nextByte != 0xDD)
            return false;
            
        // Check if ModR/M byte has reg field = 7
        byte regField = ModRMDecoder.GetRegFromModRM(modRM);
        
        // The reg field must be 7 for FSTSW m2byte
        return regField == 7;
    }
    
    /// <summary>
    /// Decodes an FSTSW m2byte instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Skip the WAIT prefix (0x9B) - we already read it in CanHandle
        if (!Decoder.CanReadByte())
            return false;

        // Read the second byte (0xDD)
        byte secondByte = Decoder.ReadByte();
        if (secondByte != 0xDD)
            return false;
            
        // Set the instruction type
        instruction.Type = InstructionType.Fstsw;
        
        // Use ModRMDecoder to read and decode the ModR/M byte for 16-bit memory operand
        var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM16();
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            memoryOperand
        ];
        
        return true;
    }
}
