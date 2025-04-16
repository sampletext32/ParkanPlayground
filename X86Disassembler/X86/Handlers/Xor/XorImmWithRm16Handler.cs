using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Xor;

/// <summary>
/// Handler for XOR r/m16, imm16 instruction (0x81 /6 with 0x66 prefix)
/// </summary>
public class XorImmWithRm16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorImmWithRm16Handler(InstructionDecoder decoder) 
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
        if (opcode != 0x81 || !Decoder.HasOperandSizePrefix())
            return false;
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        if (!Decoder.CanReadByte())
            return false;
            
        var reg = ModRMDecoder.PeakModRMReg();
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m16, imm16 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Xor;
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the ModR/M byte, specifying that we're dealing with 16-bit operands
        var (_, _, _, destinationOperand) = ModRMDecoder.ReadModRM16();
        
        // Note: The operand size is already set to 16-bit by the ReadModRM16 method
        
        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadUShort())
        {
            return false;
        }
        
        // Read the immediate value
        ushort imm16 = Decoder.ReadUInt16();
        
        // Create the source immediate operand
        var sourceOperand = OperandFactory.CreateImmediateOperand(imm16, 16);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destinationOperand,
            sourceOperand
        ];
        
        return true;
    }
}
