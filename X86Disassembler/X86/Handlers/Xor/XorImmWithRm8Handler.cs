using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m8, imm8 instruction (0x80 /6)
/// </summary>
public class XorImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm8Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorImmWithRm8Handler(InstructionDecoder decoder) 
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
        if (opcode != 0x80)
            return false;
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        if (!Decoder.CanReadByte())
            return false;
            
        byte modRM = Decoder.PeakByte();
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m8, imm8 instruction
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
        // For XOR r/m8, imm8 (0x80 /6):
        // - The r/m field with mod specifies the destination operand (register or memory)
        // - The immediate value is the source operand
        var (mod, reg, rm, destinationOperand) = ModRMDecoder.ReadModRM();
        
        // Adjust the operand size to 8-bit
        destinationOperand.Size = 8;
        
        // Read the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the immediate value
        byte imm8 = Decoder.ReadByte();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm8, 8);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
