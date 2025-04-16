using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Imul;

/// <summary>
/// Handler for IMUL r32, r/m32 instruction (0x0F 0xAF /r)
/// </summary>
public class ImulR32Rm32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ImulR32Rm32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public ImulR32Rm32Handler(InstructionDecoder decoder)
        : base(decoder)
    {
    }

    /// <summary>
    /// Checks if this handler can decode the given opcode sequence
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // IMUL r32, r/m32: opcode 0F AF /r
        if (opcode != 0x0F)
            return false;

        // Check if we can read the second byte
        if (!Decoder.CanReadByte())
            return false;
            
        // Check if the second byte is 0xAF
        byte secondByte = Decoder.PeakByte();
        
        // Only handle when the operand size prefix is NOT present
        // This ensures 16-bit handlers get priority when the prefix is present
        return secondByte == 0xAF && !Decoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Decodes an IMUL r32, r/m32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        instruction.Type = InstructionType.IMul;

        // Read the second byte of the opcode (0xAF)
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        byte secondByte = Decoder.ReadByte();
        if (secondByte != 0xAF)
        {
            return false;
        }

        // Read ModR/M: reg = destination, r/m = source
        var (_, reg, _, operand) = ModRMDecoder.ReadModRM();

        // Create the destination register operand (32-bit)
        var destOperand = OperandFactory.CreateRegisterOperand(reg);

        // Source operand is already an Operand
        instruction.StructuredOperands =
        [
            destOperand,
            operand
        ];
        return true;
    }
}
