namespace X86Disassembler.X86.Handlers.Group3;

/// <summary>
/// Handler for TEST r/m32, imm32 instruction (0xF7 /0)
/// </summary>
public class TestImmWithRm32Handler : Group3BaseHandler
{
    /// <summary>
    /// Initializes a new instance of the TestImmWithRm32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestImmWithRm32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // This handler only handles opcode 0xF7
        // The reg field check (for TEST operation) will be done in the Decode method
        return opcode == 0xF7;
    }
    
    /// <summary>
    /// Decodes a TEST r/m32, imm32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3); // Should be 0 for TEST
        byte rm = (byte)(modRM & 0x07);
        
        // Check if the reg field is 0 (TEST operation)
        if (reg != 0)
        {
            return false; // Not a TEST instruction
        }
        
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        Decoder.SetPosition(position);
        
        // Get the operand based on the addressing mode
        string destOperand;
        
        // For direct register addressing (mod == 3), the r/m field specifies a register
        if (mod == 3)
        {
            destOperand = GetRegister32(rm);
        }
        else
        {
            // Use the ModR/M decoder for memory addressing
            destOperand = _modRMDecoder.DecodeModRM(mod, rm, false);
        }
        
        // Read the immediate value
        if (position + 3 >= Length)
        {
            return false;
        }
        
        // Read the immediate value using BitConverter
        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, 0x{imm32:X8}";
        
        return true;
    }
}
