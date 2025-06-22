using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FSTSW AX instruction (0x9B 0xDF 0xE0) - Store FPU status word with wait prefix to AX register
/// </summary>
public class FstswHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FstswHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FstswHandler(InstructionDecoder decoder)
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
        // FSTSW AX starts with the WAIT prefix (0x9B)
        if (opcode != 0x9B) return false;

        // Check if we can read the next two bytes
        if (!Decoder.CanReadByte())
            return false;

        // Check if the next bytes are 0xDF 0xE0 (for FSTSW AX)
        var (nextByte, thirdByte) = Decoder.PeakTwoBytes();

        // The sequence must be 9B DF E0 for FSTSW AX
        return nextByte == 0xDF && thirdByte == 0xE0;
    }
    
    /// <summary>
    /// Decodes an FSTSW AX instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Skip the WAIT prefix (0x9B) - we already read it in CanHandle
        if (!Decoder.CanReadByte())
            return false;

        // Read the second byte (0xDF)
        byte secondByte = Decoder.ReadByte();
        if (secondByte != 0xDF)
            return false;
            
        // Read the third byte (0xE0)
        if (!Decoder.CanReadByte())
            return false;

        byte thirdByte = Decoder.ReadByte();
        if (thirdByte != 0xE0)
            return false;
        
        // Set the instruction type
        instruction.Type = InstructionType.Fstsw;
        
        // Create the AX register operand
        var axOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            axOperand
        ];
        
        return true;
    }
}
