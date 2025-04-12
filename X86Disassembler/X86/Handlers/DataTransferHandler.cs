namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for data transfer instructions (MOV, PUSH, POP, etc.)
/// </summary>
public class DataTransferHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the DataTransferHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public DataTransferHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        // MOV instructions
        if ((opcode >= 0x88 && opcode <= 0x8B) || // MOV r/m, r and MOV r, r/m
            (opcode >= 0xB0 && opcode <= 0xB7) || // MOV r8, imm8
            (opcode >= 0xB8 && opcode <= 0xBF) || // MOV r32, imm32
            opcode == 0xA0 || opcode == 0xA1 ||   // MOV AL/EAX, moffs
            opcode == 0xA2 || opcode == 0xA3)     // MOV moffs, AL/EAX
        {
            return true;
        }
        
        // PUSH instructions
        if ((opcode >= 0x50 && opcode <= 0x57) || // PUSH r32
            opcode == 0x68 || opcode == 0x6A)     // PUSH imm32/imm8
        {
            return true;
        }
        
        // POP instructions
        if (opcode >= 0x58 && opcode <= 0x5F)     // POP r32
        {
            return true;
        }
        
        // XCHG instructions
        if (opcode >= 0x90 && opcode <= 0x97)     // XCHG EAX, r32
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes a data transfer instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic based on the opcode
        instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
        
        // Handle different types of data transfer instructions
        if (opcode >= 0x88 && opcode <= 0x8B) // MOV r/m, r and MOV r, r/m
        {
            return DecodeMOVRegMem(opcode, instruction);
        }
        else if (opcode >= 0xB0 && opcode <= 0xB7) // MOV r8, imm8
        {
            return DecodeMOVRegImm8(opcode, instruction);
        }
        else if (opcode >= 0xB8 && opcode <= 0xBF) // MOV r32, imm32
        {
            return DecodeMOVRegImm32(opcode, instruction);
        }
        else if (opcode == 0xA0 || opcode == 0xA1) // MOV AL/EAX, moffs
        {
            return DecodeMOVAccMem(opcode, instruction);
        }
        else if (opcode == 0xA2 || opcode == 0xA3) // MOV moffs, AL/EAX
        {
            return DecodeMOVMemAcc(opcode, instruction);
        }
        else if (opcode >= 0x50 && opcode <= 0x57) // PUSH r32
        {
            return DecodePUSHReg(opcode, instruction);
        }
        else if (opcode == 0x68) // PUSH imm32
        {
            return DecodePUSHImm32(instruction);
        }
        else if (opcode == 0x6A) // PUSH imm8
        {
            return DecodePUSHImm8(instruction);
        }
        else if (opcode >= 0x58 && opcode <= 0x5F) // POP r32
        {
            return DecodePOPReg(opcode, instruction);
        }
        else if (opcode >= 0x90 && opcode <= 0x97) // XCHG EAX, r32
        {
            return DecodeXCHGEAXReg(opcode, instruction);
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes a MOV instruction with register and memory operands
    /// </summary>
    private bool DecodeMOVRegMem(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, memOperand) = ModRMDecoder.ReadModRM();
        
        // Determine direction (0 = r/m to reg, 1 = reg to r/m)
        bool direction = (opcode & 0x02) != 0;
        
        // Determine operand size (0 = 8-bit, 1 = 32-bit)
        bool operandSize32 = (opcode & 0x01) != 0;
        
        // Get register name based on size
        string regName = ModRMDecoder.GetRegisterName(reg, operandSize32 ? 32 : 8);
        
        // For mod == 3, both operands are registers
        if (mod == 3)
        {
            string rmRegName = ModRMDecoder.GetRegisterName(rm, operandSize32 ? 32 : 8);
            instruction.Operands = direction ? $"{rmRegName}, {regName}" : $"{regName}, {rmRegName}";
        }
        else // Memory operand
        {
            instruction.Operands = direction ? $"{memOperand}, {regName}" : $"{regName}, {memOperand}";
        }
        
        return true;
    }
    
    /// <summary>
    /// Decodes a MOV instruction with 8-bit register and immediate operand
    /// </summary>
    private bool DecodeMOVRegImm8(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 8);
        
        // Read the immediate value
        byte imm8 = CodeBuffer[position];
        Decoder.SetPosition(position + 1);
        
        instruction.Operands = $"{regName}, 0x{imm8:X2}";
        return true;
    }
    
    /// <summary>
    /// Decodes a MOV instruction with 32-bit register and immediate operand
    /// </summary>
    private bool DecodeMOVRegImm32(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        // Read the immediate value
        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        instruction.Operands = $"{regName}, 0x{imm32:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes a MOV instruction with accumulator (AL/EAX) and memory operand
    /// </summary>
    private bool DecodeMOVAccMem(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Determine operand size (0xA0 = 8-bit, 0xA1 = 32-bit)
        bool operandSize32 = opcode == 0xA1;
        string regName = operandSize32 ? "eax" : "al";
        
        // Read the memory offset
        uint offset = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        instruction.Operands = $"{regName}, [0x{offset:X8}]";
        return true;
    }
    
    /// <summary>
    /// Decodes a MOV instruction with memory operand and accumulator (AL/EAX)
    /// </summary>
    private bool DecodeMOVMemAcc(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Determine operand size (0xA2 = 8-bit, 0xA3 = 32-bit)
        bool operandSize32 = opcode == 0xA3;
        string regName = operandSize32 ? "eax" : "al";
        
        // Read the memory offset
        uint offset = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        instruction.Operands = $"[0x{offset:X8}], {regName}";
        return true;
    }
    
    /// <summary>
    /// Decodes a PUSH instruction with register operand
    /// </summary>
    private bool DecodePUSHReg(byte opcode, Instruction instruction)
    {
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        instruction.Operands = regName;
        return true;
    }
    
    /// <summary>
    /// Decodes a PUSH instruction with 32-bit immediate operand
    /// </summary>
    private bool DecodePUSHImm32(Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position + 4 > Length)
        {
            return false;
        }
        
        // Read the immediate value
        uint imm32 = BitConverter.ToUInt32(CodeBuffer, position);
        Decoder.SetPosition(position + 4);
        
        instruction.Operands = $"0x{imm32:X8}";
        return true;
    }
    
    /// <summary>
    /// Decodes a PUSH instruction with 8-bit immediate operand
    /// </summary>
    private bool DecodePUSHImm8(Instruction instruction)
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
    /// Decodes a POP instruction with register operand
    /// </summary>
    private bool DecodePOPReg(byte opcode, Instruction instruction)
    {
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        instruction.Operands = regName;
        return true;
    }
    
    /// <summary>
    /// Decodes an XCHG instruction with EAX and register operands
    /// </summary>
    private bool DecodeXCHGEAXReg(byte opcode, Instruction instruction)
    {
        // Register is encoded in the low 3 bits of the opcode
        int reg = opcode & 0x07;
        string regName = ModRMDecoder.GetRegisterName(reg, 32);
        
        instruction.Operands = $"eax, {regName}";
        return true;
    }
}
