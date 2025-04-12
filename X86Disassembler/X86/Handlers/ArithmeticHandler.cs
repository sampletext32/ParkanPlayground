namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for arithmetic and logical instructions (ADD, SUB, AND, OR, XOR, etc.)
/// </summary>
public class ArithmeticHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the ArithmeticHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public ArithmeticHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // XOR instructions
        if (opcode >= 0x30 && opcode <= 0x35)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes an arithmetic or logical instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic based on the opcode
        instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        switch (opcode)
        {
            case 0x30: // XOR r/m8, r8
            case 0x31: // XOR r/m32, r32
                {
                    // Read the ModR/M byte
                    var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
                    
                    // Determine the source register
                    string sourceReg;
                    if (opcode == 0x30) // 8-bit registers
                    {
                        sourceReg = ModRMDecoder.GetRegister8(reg);
                    }
                    else // 32-bit registers
                    {
                        sourceReg = ModRMDecoder.GetRegister32(reg);
                    }
                    
                    // Set the operands
                    instruction.Operands = $"{destOperand}, {sourceReg}";
                    return true;
                }
                
            case 0x32: // XOR r8, r/m8
            case 0x33: // XOR r32, r/m32
                {
                    // Read the ModR/M byte
                    var (mod, reg, rm, srcOperand) = ModRMDecoder.ReadModRM();
                    
                    // Determine the destination register
                    string destReg;
                    if (opcode == 0x32) // 8-bit registers
                    {
                        destReg = ModRMDecoder.GetRegister8(reg);
                    }
                    else // 32-bit registers
                    {
                        destReg = ModRMDecoder.GetRegister32(reg);
                    }
                    
                    // Set the operands
                    instruction.Operands = $"{destReg}, {srcOperand}";
                    return true;
                }
                
            case 0x34: // XOR AL, imm8
                {
                    if (position < Length)
                    {
                        byte imm8 = CodeBuffer[position];
                        Decoder.SetPosition(position + 1);
                        instruction.Operands = $"al, 0x{imm8:X2}";
                        return true;
                    }
                    break;
                }
                
            case 0x35: // XOR EAX, imm32
                {
                    if (position + 3 < Length)
                    {
                        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
                        Decoder.SetPosition(position + 4);
                        instruction.Operands = $"eax, 0x{imm32:X8}";
                        return true;
                    }
                    break;
                }
        }
        
        return false;
    }
}
