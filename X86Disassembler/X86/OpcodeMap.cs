namespace X86Disassembler.X86;

/// <summary>
/// Provides mapping between opcodes and their mnemonics
/// </summary>
public class OpcodeMap
{
    // One-byte opcode map
    private static readonly string[] OneByteOpcodes = new string[256];
    
    // Condition codes for conditional jumps
    private static readonly string[] ConditionCodes = {
        "o", "no", "b", "ae", "e", "ne", "be", "a",
        "s", "ns", "p", "np", "l", "ge", "le", "g"
    };
    
    // Group 1 operations (used with opcodes 0x80, 0x81, 0x83)
    public static readonly string[] Group1Operations = { 
        "add", "or", "adc", "sbb", "and", "sub", "xor", "cmp" 
    };
    
    // Group 3 operations (used with opcodes 0xF6, 0xF7)
    public static readonly string[] Group3Operations = { 
        "test", "???", "not", "neg", "mul", "imul", "div", "idiv" 
    };
    
    // Static constructor to initialize the opcode maps
    static OpcodeMap()
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
        OneByteOpcodes[0xD8] = "fadd";
        OneByteOpcodes[0xD9] = "fld";
        OneByteOpcodes[0xDA] = "fiadd";
        OneByteOpcodes[0xDB] = "fild";
        OneByteOpcodes[0xDC] = "fadd";
        OneByteOpcodes[0xDD] = "fld";
        OneByteOpcodes[0xDE] = "fiadd";
        OneByteOpcodes[0xDF] = "fistp";
        
        // Group 1 instructions (ADD, OR, ADC, SBB, AND, SUB, XOR, CMP)
        OneByteOpcodes[0x80] = "group1b";
        OneByteOpcodes[0x81] = "group1d";
        OneByteOpcodes[0x83] = "group1s"; // Sign-extended immediate
        
        // Group 3 instructions (TEST, NOT, NEG, MUL, IMUL, DIV, IDIV)
        OneByteOpcodes[0xF6] = "group3b"; // 8-bit operations
        OneByteOpcodes[0xF7] = "group3d"; // 32-bit operations
        
        // TEST instructions
        OneByteOpcodes[0x84] = "test"; // TEST r/m8, r8
        OneByteOpcodes[0x85] = "test"; // TEST r/m32, r32
        OneByteOpcodes[0xA8] = "test"; // TEST AL, imm8
        OneByteOpcodes[0xA9] = "test"; // TEST EAX, imm32
        
        // Data transfer instructions
        for (int i = 0x88; i <= 0x8B; i++)
        {
            OneByteOpcodes[i] = "mov";
        }
        OneByteOpcodes[0xA0] = "mov"; // MOV AL, moffs8
        OneByteOpcodes[0xA1] = "mov"; // MOV EAX, moffs32
        OneByteOpcodes[0xA2] = "mov"; // MOV moffs8, AL
        OneByteOpcodes[0xA3] = "mov"; // MOV moffs32, EAX
        
        // Control flow instructions
        OneByteOpcodes[0xCC] = "int3";
        OneByteOpcodes[0x90] = "nop";
        OneByteOpcodes[0xC3] = "ret";
        OneByteOpcodes[0xC2] = "ret"; // RET imm16
        OneByteOpcodes[0xE8] = "call";
        OneByteOpcodes[0xE9] = "jmp";
        OneByteOpcodes[0xEB] = "jmp";
        
        // Register operations
        for (int i = 0; i <= 7; i++)
        {
            OneByteOpcodes[0x40 + i] = "inc";
            OneByteOpcodes[0x48 + i] = "dec";
            OneByteOpcodes[0x50 + i] = "push";
            OneByteOpcodes[0x58 + i] = "pop";
        }
        
        // XCHG instructions
        OneByteOpcodes[0x90] = "nop"; // Special case: XCHG eax, eax = NOP
        for (int i = 1; i <= 7; i++)
        {
            OneByteOpcodes[0x90 + i] = "xchg";
        }
        
        // MOV instructions
        for (int i = 0; i <= 7; i++)
        {
            OneByteOpcodes[0xB0 + i] = "mov"; // MOV r8, imm8
            OneByteOpcodes[0xB8 + i] = "mov"; // MOV r32, imm32
        }
        
        // Conditional jumps
        for (int i = 0; i <= 0xF; i++)
        {
            OneByteOpcodes[0x70 + i] = "j" + ConditionCodes[i];
        }
        
        // Other common instructions
        OneByteOpcodes[0x68] = "push"; // PUSH imm32
        OneByteOpcodes[0x6A] = "push"; // PUSH imm8
        OneByteOpcodes[0xCD] = "int";  // INT imm8
        OneByteOpcodes[0xE3] = "jecxz"; // JECXZ rel8
        
        // XOR instructions
        OneByteOpcodes[0x30] = "xor"; // XOR r/m8, r8
        OneByteOpcodes[0x31] = "xor"; // XOR r/m32, r32
        OneByteOpcodes[0x32] = "xor"; // XOR r8, r/m8
        OneByteOpcodes[0x33] = "xor"; // XOR r32, r/m32
        OneByteOpcodes[0x34] = "xor"; // XOR AL, imm8
        OneByteOpcodes[0x35] = "xor"; // XOR EAX, imm32
    }
    
    /// <summary>
    /// Gets the mnemonic for a one-byte opcode
    /// </summary>
    /// <param name="opcode">The opcode</param>
    /// <returns>The mnemonic</returns>
    public static string GetMnemonic(byte opcode)
    {
        return OneByteOpcodes[opcode];
    }
    
    /// <summary>
    /// Checks if the opcode is a Group 1 opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if the opcode is a Group 1 opcode</returns>
    public static bool IsGroup1Opcode(byte opcode)
    {
        return opcode == 0x80 || opcode == 0x81 || opcode == 0x83;
    }
    
    /// <summary>
    /// Checks if the opcode is a Group 3 opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if the opcode is a Group 3 opcode</returns>
    public static bool IsGroup3Opcode(byte opcode)
    {
        return opcode == 0xF6 || opcode == 0xF7;
    }
    
    /// <summary>
    /// Checks if the opcode is a floating-point instruction
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if the opcode is a floating-point instruction</returns>
    public static bool IsFloatingPointOpcode(byte opcode)
    {
        return opcode >= 0xD8 && opcode <= 0xDF;
    }
}
