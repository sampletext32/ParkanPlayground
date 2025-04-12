namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for Group 3 instructions (TEST, NOT, NEG, MUL, IMUL, DIV, IDIV)
/// </summary>
public class Group3Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Group3Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Group3Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return OpcodeMap.IsGroup3Opcode(opcode);
    }
    
    /// <summary>
    /// Decodes a Group 3 instruction
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
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Determine the operation based on reg field
        instruction.Mnemonic = OpcodeMap.Group3Operations[reg];
        
        // For TEST instruction (reg = 0), we need to read an immediate value
        if (reg == 0) // TEST
        {
            position = Decoder.GetPosition();
            string immOperand;
            
            switch (opcode)
            {
                case 0xF6: // 8-bit TEST
                    if (position < Length)
                    {
                        byte imm8 = CodeBuffer[position];
                        Decoder.SetPosition(position + 1);
                        immOperand = $"0x{imm8:X2}";
                    }
                    else
                    {
                        immOperand = "???";
                    }
                    break;
                    
                case 0xF7: // 32-bit TEST
                    if (position + 3 < Length)
                    {
                        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
                        Decoder.SetPosition(position + 4);
                        immOperand = $"0x{imm32:X8}";
                    }
                    else
                    {
                        immOperand = "???";
                    }
                    break;
                    
                default:
                    return false;
            }
            
            // Set the operands
            instruction.Operands = $"{destOperand}, {immOperand}";
        }
        else
        {
            // For other Group 3 instructions (NOT, NEG, MUL, etc.), there's only one operand
            instruction.Operands = destOperand;
        }
        
        return true;
    }
}
