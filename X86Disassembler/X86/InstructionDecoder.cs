namespace X86Disassembler.X86;

/// <summary>
/// Decoder for x86 instructions
/// </summary>
public class InstructionDecoder
{
    // Instruction prefixes
    private const byte PREFIX_LOCK = 0xF0;
    private const byte PREFIX_REPNE = 0xF2;
    private const byte PREFIX_REP = 0xF3;
    private const byte PREFIX_CS = 0x2E;
    private const byte PREFIX_SS = 0x36;
    private const byte PREFIX_DS = 0x3E;
    private const byte PREFIX_ES = 0x26;
    private const byte PREFIX_FS = 0x64;
    private const byte PREFIX_GS = 0x65;
    private const byte PREFIX_OPERAND_SIZE = 0x66;
    private const byte PREFIX_ADDRESS_SIZE = 0x67;
    
    // Common opcodes
    private const byte OPCODE_INT3 = 0xCC;
    private const byte OPCODE_NOP = 0x90;
    private const byte OPCODE_RET = 0xC3;
    private const byte OPCODE_CALL_NEAR_RELATIVE = 0xE8;
    private const byte OPCODE_JMP_NEAR_RELATIVE = 0xE9;
    private const byte OPCODE_JMP_SHORT_RELATIVE = 0xEB;
    
    // Opcode groups
    private const byte OPCODE_GROUP_1_BYTE = 0x80;
    private const byte OPCODE_GROUP_1_WORD_DWORD = 0x81;
    private const byte OPCODE_GROUP_1_BYTE_IMM8 = 0x83;
    
    // ModR/M byte masks
    private const byte MODRM_MOD_MASK = 0xC0; // 11000000b
    private const byte MODRM_REG_MASK = 0x38; // 00111000b
    private const byte MODRM_RM_MASK = 0x07;  // 00000111b
    
    // SIB byte masks
    private const byte SIB_SCALE_MASK = 0xC0; // 11000000b
    private const byte SIB_INDEX_MASK = 0x38; // 00111000b
    private const byte SIB_BASE_MASK = 0x07;  // 00000111b
    
    // Register names
    private static readonly string[] RegisterNames8 = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
    private static readonly string[] RegisterNames16 = { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di" };
    private static readonly string[] RegisterNames32 = { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi" };
    private static readonly string[] SegmentRegisterNames = { "es", "cs", "ss", "ds", "fs", "gs" };
    
    // Condition codes for conditional jumps
    private static readonly string[] ConditionCodes = {
        "o", "no", "b", "ae", "e", "ne", "be", "a",
        "s", "ns", "p", "np", "l", "ge", "le", "g"
    };
    
    // One-byte opcode map
    private static readonly string[] OneByteOpcodes = new string[256];
    
    // Buffer containing the code to decode
    private readonly byte[] _codeBuffer;
    
    // Current position in the code buffer
    private int _position;
    
    // Length of the buffer
    private readonly int _length;
    
    /// <summary>
    /// Static constructor to initialize the opcode maps
    /// </summary>
    static InstructionDecoder()
    {
        InitializeOpcodeMaps();
    }
    
    /// <summary>
    /// Initializes the opcode maps
    /// </summary>
    private static void InitializeOpcodeMaps()
    {
        // Initialize all entries to "??" (unknown)
        for (int i = 0; i < 256; i++)
        {
            OneByteOpcodes[i] = "??";
        }
        
        // Floating-point instructions
        OneByteOpcodes[0xD8] = "fadd";  // Various FP instructions based on ModR/M
        OneByteOpcodes[0xD9] = "fld";   // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDA] = "fiadd"; // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDB] = "fild";  // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDC] = "fadd";  // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDD] = "fld";   // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDE] = "fiadd"; // Various FP instructions based on ModR/M
        OneByteOpcodes[0xDF] = "fistp"; // Various FP instructions based on ModR/M
        
        // Data transfer instructions
        for (int i = 0x88; i <= 0x8B; i++)
        {
            OneByteOpcodes[i] = "mov";
        }
        OneByteOpcodes[0xA0] = "mov"; // MOV AL, moffs8
        OneByteOpcodes[0xA1] = "mov"; // MOV EAX, moffs32
        OneByteOpcodes[0xA2] = "mov"; // MOV moffs8, AL
        OneByteOpcodes[0xA3] = "mov"; // MOV moffs32, EAX
        for (int i = 0xB0; i <= 0xB7; i++)
        {
            OneByteOpcodes[i] = "mov"; // MOV r8, imm8
        }
        for (int i = 0xB8; i <= 0xBF; i++)
        {
            OneByteOpcodes[i] = "mov"; // MOV r32, imm32
        }
        OneByteOpcodes[0xC6] = "mov"; // MOV r/m8, imm8
        OneByteOpcodes[0xC7] = "mov"; // MOV r/m32, imm32
        
        // Push/Pop instructions
        for (int i = 0x50; i <= 0x57; i++)
        {
            OneByteOpcodes[i] = "push"; // PUSH r32
        }
        for (int i = 0x58; i <= 0x5F; i++)
        {
            OneByteOpcodes[i] = "pop"; // POP r32
        }
        OneByteOpcodes[0x68] = "push"; // PUSH imm32
        OneByteOpcodes[0x6A] = "push"; // PUSH imm8
        OneByteOpcodes[0x8F] = "pop";  // POP r/m32
        OneByteOpcodes[0x9C] = "pushf"; // PUSHF
        OneByteOpcodes[0x9D] = "popf";  // POPF
        
        // Arithmetic instructions
        for (int i = 0x00; i <= 0x05; i++)
        {
            OneByteOpcodes[i] = "add";
        }
        for (int i = 0x28; i <= 0x2D; i++)
        {
            OneByteOpcodes[i] = "sub";
        }
        for (int i = 0x30; i <= 0x35; i++)
        {
            OneByteOpcodes[i] = "xor";
        }
        for (int i = 0x38; i <= 0x3D; i++)
        {
            OneByteOpcodes[i] = "cmp";
        }
        OneByteOpcodes[0x40] = "inc"; // INC eax
        OneByteOpcodes[0x41] = "inc"; // INC ecx
        OneByteOpcodes[0x42] = "inc"; // INC edx
        OneByteOpcodes[0x43] = "inc"; // INC ebx
        OneByteOpcodes[0x44] = "inc"; // INC esp
        OneByteOpcodes[0x45] = "inc"; // INC ebp
        OneByteOpcodes[0x46] = "inc"; // INC esi
        OneByteOpcodes[0x47] = "inc"; // INC edi
        OneByteOpcodes[0x48] = "dec"; // DEC eax
        OneByteOpcodes[0x49] = "dec"; // DEC ecx
        OneByteOpcodes[0x4A] = "dec"; // DEC edx
        OneByteOpcodes[0x4B] = "dec"; // DEC ebx
        OneByteOpcodes[0x4C] = "dec"; // DEC esp
        OneByteOpcodes[0x4D] = "dec"; // DEC ebp
        OneByteOpcodes[0x4E] = "dec"; // DEC esi
        OneByteOpcodes[0x4F] = "dec"; // DEC edi
        
        // Logical instructions
        for (int i = 0x20; i <= 0x25; i++)
        {
            OneByteOpcodes[i] = "and";
        }
        for (int i = 0x08; i <= 0x0D; i++)
        {
            OneByteOpcodes[i] = "or";
        }
        OneByteOpcodes[0xF7] = "not"; // Group 3 - NOT, NEG, MUL, IMUL, DIV, IDIV
        
        // Shift and rotate instructions
        OneByteOpcodes[0xD0] = "rol"; // Group 2 - ROL, ROR, RCL, RCR, SHL/SAL, SHR, SAR
        OneByteOpcodes[0xD1] = "rol"; // Group 2 - ROL, ROR, RCL, RCR, SHL/SAL, SHR, SAR
        OneByteOpcodes[0xD2] = "rol"; // Group 2 - ROL, ROR, RCL, RCR, SHL/SAL, SHR, SAR
        OneByteOpcodes[0xD3] = "rol"; // Group 2 - ROL, ROR, RCL, RCR, SHL/SAL, SHR, SAR
        
        // Control flow instructions
        OneByteOpcodes[0xC3] = "ret";
        OneByteOpcodes[0xC2] = "ret";
        OneByteOpcodes[0xCA] = "retf";
        OneByteOpcodes[0xCB] = "retf";
        OneByteOpcodes[0xCC] = "int3";
        OneByteOpcodes[0xCD] = "int";
        OneByteOpcodes[0xCE] = "into";
        OneByteOpcodes[0xCF] = "iret";
        OneByteOpcodes[0xE8] = "call";
        OneByteOpcodes[0xE9] = "jmp";
        OneByteOpcodes[0xEB] = "jmp";
        OneByteOpcodes[0xFF] = "call"; // Group 5 - CALL, JMP, PUSH
        
        // Conditional jumps
        for (int i = 0x70; i <= 0x7F; i++)
        {
            OneByteOpcodes[i] = "j" + ConditionCodes[i - 0x70];
        }
        
        // String instructions
        OneByteOpcodes[0xA4] = "movsb";
        OneByteOpcodes[0xA5] = "movsd";
        OneByteOpcodes[0xA6] = "cmpsb";
        OneByteOpcodes[0xA7] = "cmpsd";
        OneByteOpcodes[0xAA] = "stosb";
        OneByteOpcodes[0xAB] = "stosd";
        OneByteOpcodes[0xAC] = "lodsb";
        OneByteOpcodes[0xAD] = "lodsd";
        OneByteOpcodes[0xAE] = "scasb";
        OneByteOpcodes[0xAF] = "scasd";
        
        // Misc instructions
        OneByteOpcodes[0x90] = "nop";
        OneByteOpcodes[0x91] = "xchg"; // XCHG eax, ecx
        OneByteOpcodes[0x92] = "xchg"; // XCHG eax, edx
        OneByteOpcodes[0x93] = "xchg"; // XCHG eax, ebx
        OneByteOpcodes[0x94] = "xchg"; // XCHG eax, esp
        OneByteOpcodes[0x95] = "xchg"; // XCHG eax, ebp
        OneByteOpcodes[0x96] = "xchg"; // XCHG eax, esi
        OneByteOpcodes[0x97] = "xchg"; // XCHG eax, edi
        OneByteOpcodes[0x98] = "cwde";
        OneByteOpcodes[0x99] = "cdq";
        OneByteOpcodes[0xF4] = "hlt";
        OneByteOpcodes[0xF5] = "cmc";
        OneByteOpcodes[0xF8] = "clc";
        OneByteOpcodes[0xF9] = "stc";
        OneByteOpcodes[0xFA] = "cli";
        OneByteOpcodes[0xFB] = "sti";
        OneByteOpcodes[0xFC] = "cld";
        OneByteOpcodes[0xFD] = "std";
    }
    
    /// <summary>
    /// Initializes a new instance of the InstructionDecoder class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    public InstructionDecoder(byte[] codeBuffer)
    {
        _codeBuffer = codeBuffer;
        _position = 0;
        _length = codeBuffer.Length;
    }
    
    /// <summary>
    /// Decodes an instruction at the specified position in the code buffer
    /// </summary>
    /// <param name="position">The position in the code buffer</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>The number of bytes read</returns>
    public int DecodeAt(int position, Instruction instruction)
    {
        _position = position;
        return Decode(instruction);
    }
    
    /// <summary>
    /// Decodes an instruction at the current position in the code buffer
    /// </summary>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>The number of bytes read</returns>
    public int Decode(Instruction instruction)
    {
        // Store the starting position
        int startPosition = _position;
        
        // Check if we've reached the end of the buffer
        if (_position >= _length)
        {
            return 0;
        }
        
        // Handle instruction prefixes
        bool hasPrefix = true;
        bool operandSizePrefix = false;
        bool addressSizePrefix = false;
        string segmentOverride = string.Empty;
        
        while (hasPrefix && _position < _length)
        {
            byte prefix = _codeBuffer[_position];
            
            switch (prefix)
            {
                case PREFIX_LOCK:
                case PREFIX_REPNE:
                case PREFIX_REP:
                    _position++;
                    break;
                    
                case PREFIX_CS:
                    segmentOverride = "cs";
                    _position++;
                    break;
                    
                case PREFIX_SS:
                    segmentOverride = "ss";
                    _position++;
                    break;
                    
                case PREFIX_DS:
                    segmentOverride = "ds";
                    _position++;
                    break;
                    
                case PREFIX_ES:
                    segmentOverride = "es";
                    _position++;
                    break;
                    
                case PREFIX_FS:
                    segmentOverride = "fs";
                    _position++;
                    break;
                    
                case PREFIX_GS:
                    segmentOverride = "gs";
                    _position++;
                    break;
                    
                case PREFIX_OPERAND_SIZE:
                    operandSizePrefix = true;
                    _position++;
                    break;
                    
                case PREFIX_ADDRESS_SIZE:
                    addressSizePrefix = true;
                    _position++;
                    break;
                    
                default:
                    hasPrefix = false;
                    break;
            }
        }
        
        // We've reached the end of the buffer after processing prefixes
        if (_position >= _length)
        {
            return _position - startPosition;
        }
        
        // Read the opcode
        byte opcode = _codeBuffer[_position++];
        
        // Get the mnemonic from the opcode map
        string mnemonic = OneByteOpcodes[opcode];
        
        // Handle specific opcodes
        string operands = string.Empty;
        
        switch (opcode)
        {
            case 0xDF: // FISTP and other FPU instructions
                if (_position < _length)
                {
                    byte modRM = _codeBuffer[_position++];
                    byte mod = (byte)((modRM & MODRM_MOD_MASK) >> 6);
                    byte reg = (byte)((modRM & MODRM_REG_MASK) >> 3);
                    byte rm = (byte)(modRM & MODRM_RM_MASK);
                    
                    // FISTP with memory operand
                    if (reg == 7) // FISTP
                    {
                        if (mod == 0 && rm == 5) // Displacement only addressing
                        {
                            if (_position + 4 <= _length)
                            {
                                uint disp32 = BitConverter.ToUInt32(_codeBuffer, _position);
                                _position += 4;
                                operands = $"qword ptr [0x{disp32:X8}]";
                            }
                        }
                        else
                        {
                            // Handle other addressing modes if needed
                            operands = DecodeModRM(mod, rm, true);
                        }
                    }
                }
                break;
                
            case 0xA1: // MOV EAX, memory
                if (_position + 4 <= _length)
                {
                    uint addr = BitConverter.ToUInt32(_codeBuffer, _position);
                    _position += 4;
                    operands = $"eax, [0x{addr:X8}]";
                }
                break;
                
            case OPCODE_INT3:
                // No operands for INT3
                break;
                
            case OPCODE_NOP:
                // No operands for NOP
                break;
                
            case OPCODE_RET:
                // No operands for RET
                break;
                
            case OPCODE_CALL_NEAR_RELATIVE:
                if (_position + 4 <= _length)
                {
                    // Read 32-bit relative offset
                    int offset = BitConverter.ToInt32(_codeBuffer, _position);
                    _position += 4;
                    
                    // Calculate target address (relative to next instruction)
                    uint targetAddress = (uint)(_position + offset);
                    operands = $"0x{targetAddress:X8}";
                }
                break;
                
            case OPCODE_JMP_NEAR_RELATIVE:
                if (_position + 4 <= _length)
                {
                    // Read 32-bit relative offset
                    int offset = BitConverter.ToInt32(_codeBuffer, _position);
                    _position += 4;
                    
                    // Calculate target address (relative to next instruction)
                    uint targetAddress = (uint)(_position + offset);
                    operands = $"0x{targetAddress:X8}";
                }
                break;
                
            case OPCODE_JMP_SHORT_RELATIVE:
                if (_position < _length)
                {
                    // Read 8-bit relative offset
                    sbyte offset = (sbyte)_codeBuffer[_position++];
                    
                    // Calculate target address (relative to next instruction)
                    uint targetAddress = (uint)(_position + offset);
                    operands = $"0x{targetAddress:X8}";
                }
                break;
                
            default:
                // Handle register-based instructions
                if (opcode >= 0x40 && opcode <= 0x47) // INC r32
                {
                    int reg = opcode - 0x40;
                    operands = RegisterNames32[reg];
                }
                else if (opcode >= 0x48 && opcode <= 0x4F) // DEC r32
                {
                    int reg = opcode - 0x48;
                    operands = RegisterNames32[reg];
                }
                else if (opcode >= 0x50 && opcode <= 0x57) // PUSH r32
                {
                    int reg = opcode - 0x50;
                    operands = RegisterNames32[reg];
                }
                else if (opcode >= 0x58 && opcode <= 0x5F) // POP r32
                {
                    int reg = opcode - 0x58;
                    operands = RegisterNames32[reg];
                }
                else if (opcode >= 0x91 && opcode <= 0x97) // XCHG eax, r32
                {
                    int reg = opcode - 0x90;
                    operands = $"eax, {RegisterNames32[reg]}";
                }
                else if (opcode >= 0xB0 && opcode <= 0xB7) // MOV r8, imm8
                {
                    if (_position < _length)
                    {
                        int reg = opcode - 0xB0;
                        byte imm8 = _codeBuffer[_position++];
                        operands = $"{RegisterNames8[reg]}, 0x{imm8:X2}";
                    }
                }
                else if (opcode >= 0xB8 && opcode <= 0xBF) // MOV r32, imm32
                {
                    if (_position + 4 <= _length)
                    {
                        int reg = opcode - 0xB8;
                        uint imm32 = BitConverter.ToUInt32(_codeBuffer, _position);
                        _position += 4;
                        operands = $"{RegisterNames32[reg]}, 0x{imm32:X8}";
                    }
                }
                else if (opcode >= 0x70 && opcode <= 0x7F) // Conditional jumps (short)
                {
                    if (_position < _length)
                    {
                        sbyte offset = (sbyte)_codeBuffer[_position++];
                        uint targetAddress = (uint)(_position + offset);
                        operands = $"0x{targetAddress:X8}";
                    }
                }
                else if (opcode == 0x68) // PUSH imm32
                {
                    if (_position + 4 <= _length)
                    {
                        uint imm32 = BitConverter.ToUInt32(_codeBuffer, _position);
                        _position += 4;
                        operands = $"0x{imm32:X8}";
                    }
                }
                else if (opcode == 0x6A) // PUSH imm8
                {
                    if (_position < _length)
                    {
                        byte imm8 = _codeBuffer[_position++];
                        operands = $"0x{imm8:X2}";
                    }
                }
                else if (opcode == 0xCD) // INT imm8
                {
                    if (_position < _length)
                    {
                        byte imm8 = _codeBuffer[_position++];
                        operands = $"0x{imm8:X2}";
                    }
                }
                else if (opcode == 0xE3) // JECXZ rel8
                {
                    if (_position < _length)
                    {
                        sbyte offset = (sbyte)_codeBuffer[_position++];
                        uint targetAddress = (uint)(_position + offset);
                        operands = $"0x{targetAddress:X8}";
                    }
                }
                else
                {
                    // For other opcodes, we'll just show the raw bytes for now
                    // In a full implementation, we would decode the ModR/M byte, SIB byte, etc.
                }
                break;
        }
        
        // Set the instruction properties
        instruction.Mnemonic = mnemonic;
        instruction.Operands = operands;
        
        // Copy the instruction bytes
        int bytesRead = _position - startPosition;
        instruction.Bytes = new byte[bytesRead];
        Array.Copy(_codeBuffer, startPosition, instruction.Bytes, 0, bytesRead);
        
        return bytesRead;
    }
    
    /// <summary>
    /// Decodes a ModR/M byte to get the operand string
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rm">The r/m field (3 bits)</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The operand string</returns>
    private string DecodeModRM(byte mod, byte rm, bool is64Bit)
    {
        string sizePrefix = is64Bit ? "qword" : "dword";
        
        switch (mod)
        {
            case 0: // [reg] or disp32
                if (rm == 5) // disp32
                {
                    if (_position + 4 <= _length)
                    {
                        uint disp32 = BitConverter.ToUInt32(_codeBuffer, _position);
                        _position += 4;
                        return $"{sizePrefix} ptr [0x{disp32:X8}]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else if (rm == 4) // SIB
                {
                    // Handle SIB byte
                    if (_position < _length)
                    {
                        byte sib = _codeBuffer[_position++];
                        // Decode SIB byte (not implemented yet)
                        return $"{sizePrefix} ptr [SIB]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else
                {
                    return $"{sizePrefix} ptr [{RegisterNames32[rm]}]";
                }
                
            case 1: // [reg + disp8]
                if (rm == 4) // SIB + disp8
                {
                    // Handle SIB byte
                    if (_position + 1 < _length)
                    {
                        byte sib = _codeBuffer[_position++];
                        sbyte disp8 = (sbyte)_codeBuffer[_position++];
                        // Decode SIB byte (not implemented yet)
                        return $"{sizePrefix} ptr [SIB+0x{disp8:X2}]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else
                {
                    if (_position < _length)
                    {
                        sbyte disp8 = (sbyte)_codeBuffer[_position++];
                        string dispStr = disp8 < 0 ? $"-0x{-disp8:X2}" : $"+0x{disp8:X2}";
                        return $"{sizePrefix} ptr [{RegisterNames32[rm]}{dispStr}]";
                    }
                    return $"{sizePrefix} ptr [{RegisterNames32[rm]}+???]";
                }
                
            case 2: // [reg + disp32]
                if (rm == 4) // SIB + disp32
                {
                    // Handle SIB byte
                    if (_position + 4 < _length)
                    {
                        byte sib = _codeBuffer[_position++];
                        int disp32 = BitConverter.ToInt32(_codeBuffer, _position);
                        _position += 4;
                        // Decode SIB byte (not implemented yet)
                        return $"{sizePrefix} ptr [SIB+0x{disp32:X8}]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else
                {
                    if (_position + 4 <= _length)
                    {
                        int disp32 = BitConverter.ToInt32(_codeBuffer, _position);
                        _position += 4;
                        string dispStr = disp32 < 0 ? $"-0x{-disp32:X8}" : $"+0x{disp32:X8}";
                        return $"{sizePrefix} ptr [{RegisterNames32[rm]}{dispStr}]";
                    }
                    return $"{sizePrefix} ptr [{RegisterNames32[rm]}+???]";
                }
                
            case 3: // reg
                return is64Bit ? "mm" + rm : RegisterNames32[rm];
                
            default:
                return "???";
        }
    }
}
