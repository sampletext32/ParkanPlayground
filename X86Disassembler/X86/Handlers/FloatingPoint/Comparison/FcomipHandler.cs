namespace X86Disassembler.X86.Handlers.FloatingPoint.Comparison;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FCOMIP ST(0), ST(i) instruction (DF F0-F7)
/// </summary>
public class FcomipHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FcomipHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FcomipHandler(InstructionDecoder decoder)
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
        // FCOMIP ST(0), ST(i) is DF F0-F7
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check second opcode byte
        byte secondOpcode = Decoder.PeakByte();
        
        // Only handle F0-F7
        return secondOpcode is >= 0xF0 and <= 0xF7;
    }
    
    /// <summary>
    /// Decodes a FCOMIP ST(0), ST(i) instruction
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

        // Read the ModR/M byte
        var (mod, reg, rm, _) = ModRMDecoder.ReadModRMFpu();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fcomip;
        
        // Create the FPU register operands
        var srcOperand = OperandFactory.CreateFPURegisterOperand(rm);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            srcOperand
        ];

        return true;
    }
}
