namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for Group 1 instructions (ADD, OR, ADC, SBB, AND, SUB, XOR, CMP)
/// </summary>
public class Group1Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the Group1Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public Group1Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x80 || opcode == 0x81 || opcode == 0x83;
    }
    
    /// <summary>
    /// Decodes a Group 1 instruction
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
        instruction.Mnemonic = OpcodeMap.Group1Operations[reg];
        
        // Read the immediate value based on opcode
        string immOperand;
        position = Decoder.GetPosition();
        
        switch (opcode)
        {
            case 0x80: // 8-bit immediate
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
                
            case 0x81: // 32-bit immediate
                if (position + 4 <= Length)
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
                
            case 0x83: // 8-bit sign-extended immediate
                if (position < Length)
                {
                    sbyte imm8 = (sbyte)CodeBuffer[position];
                    Decoder.SetPosition(position + 1);
                    immOperand = $"0x{imm8:X2}";
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
        
        return true;
    }
}
