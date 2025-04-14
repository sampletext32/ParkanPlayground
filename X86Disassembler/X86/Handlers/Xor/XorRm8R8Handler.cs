using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m8, r8 instruction (0x30)
/// </summary>
public class XorRm8R8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorRm8R8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorRm8R8Handler(InstructionDecoder decoder)
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
        return opcode == 0x30;
    }

    /// <summary>
    /// Decodes a XOR r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For XOR r/m8, r8 (0x30):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The reg field specifies the source register
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();

        // Adjust the operand size to 8-bit
        destinationOperand.Size = 8;

        // Create the source register operand
        var sourceOperand = OperandFactory.CreateRegisterOperand(reg, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];

        return true;
    }
}
