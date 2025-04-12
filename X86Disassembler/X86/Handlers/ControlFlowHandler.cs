namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for control flow instructions (JMP, CALL, RET, etc.)
/// </summary>
public class ControlFlowHandler : InstructionHandler
{
    // Condition codes for conditional jumps
    private static readonly string[] ConditionCodes = {
        "o", "no", "b", "ae", "e", "ne", "be", "a",
        "s", "ns", "p", "np", "l", "ge", "le", "g"
    };
    
    /// <summary>
    /// Initializes a new instance of the ControlFlowHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public ControlFlowHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // RET instruction
        if (opcode == 0xC3 || opcode == 0xC2)
        {
            return true;
        }
        
        // CALL instruction
        if (opcode == 0xE8)
        {
            return true;
        }
        
        // JMP instructions
        if (opcode == 0xE9 || opcode == 0xEB)
        {
            return true;
        }
        
        // Conditional jumps
        if (opcode >= 0x70 && opcode <= 0x7F)
        {
            return true;
        }
        
        // INT instructions
        if (opcode == 0xCC || opcode == 0xCD)
        {
            return true;
        }
        
        // JECXZ instruction
        if (opcode == 0xE3)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes a control flow instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic based on the opcode
        instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
        
        // Handle different types of control flow instructions
        if (opcode == 0xC3) // RET
        {
            // No operands for RET
            instruction.Operands = string.Empty;
            return true;
        }
        else if (opcode == 0xC2) // RET imm16
        {
            return DecodeRETImm16(instruction);
        }
        else if (opcode == 0xE8) // CALL rel32
        {
            return DecodeCALLRel32(instruction);
        }
        else if (opcode == 0xE9) // JMP rel32
        {
            return DecodeJMPRel32(instruction);
        }
        else if (opcode == 0xEB) // JMP rel8
        {
            return DecodeJMPRel8(instruction);
        }
        else if (opcode >= 0x70 && opcode <= 0x7F) // Conditional jumps
        {
            return DecodeConditionalJump(opcode, instruction);
        }
        else if (opcode == 0xCC) // INT3
        {
            // No operands for INT3
            instruction.Operands = string.Empty;
            return true;
        }
        else if (opcode == 0xCD) // INT imm8
        {
            return DecodeINTImm8(instruction);
        }
        else if (opcode == 0xE3) // JECXZ rel8
        {
            return DecodeJECXZRel8(instruction);
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes a RET instruction with 16-bit immediate operand
    /// </summary>
    private bool DecodeRETImm16(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 2 > Length)
        {
            return false;
        }
        
        // Read the immediate value
        ushort imm16 = BitConverter.ToUInt16(CodeBuffer, position);
        Decoder.SetPosition(position + 2);
        
        instruction.Operands = $"0x{imm16:X4}";
        return true;
    }
    
    /// <summary>
    /// Decodes a CALL instruction with 32-bit relative offset
    /// </summary>
    private bool DecodeCALLRel32(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Read the relative offset
        int offset = BitConverter.ToInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        // Calculate the target address (relative to the next instruction)
        uint targetAddress = (uint)(position + offset);
        
        instruction.Operands = $"0x{targetAddress:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes a JMP instruction with 32-bit relative offset
    /// </summary>
    private bool DecodeJMPRel32(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Read the relative offset
        int offset = BitConverter.ToInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        // Calculate the target address (relative to the next instruction)
        uint targetAddress = (uint)(position + offset);
        
        instruction.Operands = $"0x{targetAddress:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes a JMP instruction with 8-bit relative offset
    /// </summary>
    private bool DecodeJMPRel8(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the relative offset
        sbyte offset = (sbyte)CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        // Calculate the target address (relative to the next instruction)
        uint targetAddress = (uint)(position + offset + 1); // +1 because the offset is relative to the next instruction
        
        instruction.Operands = $"0x{targetAddress:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes a conditional jump instruction
    /// </summary>
    private bool DecodeConditionalJump(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the relative offset
        sbyte offset = (sbyte)CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        // Calculate the target address (relative to the next instruction)
        uint targetAddress = (uint)(position + offset + 1); // +1 because the offset is relative to the next instruction
        
        instruction.Operands = $"0x{targetAddress:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes an INT instruction with 8-bit immediate operand
    /// </summary>
    private bool DecodeINTImm8(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value
        byte imm8 = CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        instruction.Operands = $"0x{imm8:X2}";
        return true;
    }
    
    /// <summary>
    /// Decodes a JECXZ instruction with 8-bit relative offset
    /// </summary>
    private bool DecodeJECXZRel8(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the relative offset
        sbyte offset = (sbyte)CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        // Calculate the target address (relative to the next instruction)
        uint targetAddress = (uint)(position + offset + 1); // +1 because the offset is relative to the next instruction
        
        instruction.Operands = $"0x{targetAddress:X8}";
        return true;
    }
}
