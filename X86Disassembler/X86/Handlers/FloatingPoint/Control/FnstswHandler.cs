using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FNSTSW AX instruction (0xDF 0xE0)
/// </summary>
public class FnstswHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FnstswHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FnstswHandler(InstructionDecoder decoder)
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
        // FNSTSW is a two-byte opcode (0xDF 0xE0)
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        if (Decoder.PeakByte() != 0xE0) 
            return false;
        
        return true;

    }
    
    /// <summary>
    /// Decodes an FNSTSW instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Check if we can read the second byte of the opcode
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Verify the second byte is 0xE0
        byte secondByte = Decoder.ReadByte();
        if (secondByte != 0xE0)
        {
            return false;
        }
        
        // Set the instruction type
        instruction.Type = InstructionType.Fnstsw;
        
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
