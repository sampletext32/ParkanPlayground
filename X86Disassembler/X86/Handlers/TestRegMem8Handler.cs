namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for TEST r/m8, r8 instruction (0x84)
/// </summary>
public class TestRegMem8Handler : InstructionHandler
{
    // ModR/M decoder
    private readonly ModRMDecoder _modRMDecoder;
    
    /// <summary>
    /// Initializes a new instance of the TestRegMem8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestRegMem8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
        _modRMDecoder = new ModRMDecoder(codeBuffer, decoder, length);
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        return opcode == 0x84;
    }
    
    /// <summary>
    /// Decodes a TEST r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);
        
        // Decode the destination operand
        string destOperand = _modRMDecoder.DecodeModRM(mod, rm, true);
        
        // Get the source register
        string srcReg = GetRegister8(reg);
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {srcReg}";
        
        return true;
    }
    
    /// <summary>
    /// Gets the 8-bit register name for the given register index
    /// </summary>
    /// <param name="reg">The register index</param>
    /// <returns>The register name</returns>
    private static string GetRegister8(byte reg)
    {
        string[] registerNames = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
        return registerNames[reg & 0x07];
    }
}
