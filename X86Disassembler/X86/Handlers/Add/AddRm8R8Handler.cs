using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m8, r8 instruction (opcode 00)
/// </summary>
public class AddRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddRm8R8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public AddRm8R8Handler(InstructionDecoder decoder)
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
        // ADD r/m8, r8 is encoded as 00 /r
        return opcode == 0x00;
    }

    /// <summary>
    /// Decodes an ADD r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Add;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For ADD r/m8, r8 (00 /r):
        // - The reg field specifies the source register
        // - The r/m field with mod specifies the destination operand (register or memory)
        var (_, reg, _, destinationOperand) = ModRMDecoder.ReadModRM8();
        
        // Create the source register operand using the 8-bit register type
        var sourceOperand = OperandFactory.CreateRegisterOperand8(reg);

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
