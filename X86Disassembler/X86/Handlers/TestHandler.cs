namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for TEST instructions
/// </summary>
public class TestHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x84 || opcode == 0x85 || opcode == 0xA8 || opcode == 0xA9;
    }
    
    /// <summary>
    /// Decodes a TEST instruction
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
        
        // Set the mnemonic
        instruction.Mnemonic = "test";
        
        switch (opcode)
        {
            case 0x84: // TEST r/m8, r8
            case 0x85: // TEST r/m32, r32
                // Read the ModR/M byte
                var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
                
                // Determine the source register
                string sourceReg;
                if (opcode == 0x84) // 8-bit registers
                {
                    sourceReg = ModRMDecoder.GetRegister8(reg);
                }
                else // 32-bit registers
                {
                    sourceReg = ModRMDecoder.GetRegister32(reg);
                }
                
                // Set the operands
                instruction.Operands = $"{destOperand}, {sourceReg}";
                break;
                
            case 0xA8: // TEST AL, imm8
                if (position < Length)
                {
                    byte imm8 = CodeBuffer[position];
                    Decoder.SetPosition(position + 1);
                    instruction.Operands = $"al, 0x{imm8:X2}";
                }
                else
                {
                    instruction.Operands = "al, ???";
                }
                break;
                
            case 0xA9: // TEST EAX, imm32
                if (position + 3 < Length)
                {
                    uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
                    Decoder.SetPosition(position + 4);
                    instruction.Operands = $"eax, 0x{imm32:X8}";
                }
                else
                {
                    instruction.Operands = "eax, ???";
                }
                break;
                
            default:
                return false;
        }
        
        return true;
    }
}
