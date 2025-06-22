namespace X86Disassembler.X86.Handlers.FloatingPoint.Comparison;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FCOMI instruction (DB F0-F7)
/// </summary>
public class FcomiHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FcomiHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FcomiHandler(InstructionDecoder decoder)
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
        // FCOMI is DB F0-F7
        if (opcode != 0xDB) return false;

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
    /// Decodes a FCOMI instruction
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
        instruction.Type = InstructionType.Fcomi;
        
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
