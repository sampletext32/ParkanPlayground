namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m8, r8 instruction (0x84)
/// </summary>
public class TestRegMem8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestRegMem8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestRegMem8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        
        // For direct register addressing (mod == 3), the r/m field specifies a register
        if (mod == 3)
        {
            // Get the register names
            string rmReg = GetRegister8(rm);
            string regReg = GetRegister8(reg);
            
            // Set the operands (TEST r/m8, r8)
            // In x86 assembly, the TEST instruction has the operand order r/m8, r8
            // According to Ghidra and standard x86 assembly convention, it should be TEST CL,AL
            // where CL is the r/m operand and AL is the reg operand
            instruction.Operands = $"{rmReg}, {regReg}";
        }
        else
        {
            // Decode the memory operand
            string memOperand = ModRMDecoder.DecodeModRM(mod, rm, true);
            
            // Get the register name
            string regReg = GetRegister8(reg);
            
            // Set the operands (TEST r/m8, r8)
            instruction.Operands = $"{memOperand}, {regReg}";
        }
        
        return true;
    }
}
