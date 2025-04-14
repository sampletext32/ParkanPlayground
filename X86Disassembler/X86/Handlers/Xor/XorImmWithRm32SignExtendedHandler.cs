namespace X86Disassembler.X86.Handlers.Xor;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for XOR r/m32, imm8 (sign-extended) instruction (0x83 /6)
/// </summary>
public class XorImmWithRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the XorImmWithRm32SignExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public XorImmWithRm32SignExtendedHandler(InstructionDecoder decoder) 
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
        if (opcode != 0x83)
            return false;
            
        // Check if the reg field of the ModR/M byte is 6 (XOR)
        if (!Decoder.CanReadByte())
            return false;
            
        byte modRM = Decoder.PeakByte();
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 6; // 6 = XOR
    }
    
    /// <summary>
    /// Decodes a XOR r/m32, imm8 (sign-extended) instruction
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Read the immediate value (sign-extended from 8 to 32 bits)
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Read the immediate value and sign-extend it to 32 bits
        int imm32 = (sbyte)Decoder.ReadByte();
        
        // Create the immediate operand with sign extension
        var immOperand = OperandFactory.CreateImmediateOperand(imm32);
        
        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            immOperand
        ];
        
        return true;
    }
}
