namespace X86Disassembler.X86;

public class Constants
{
    // ModR/M byte masks
    public const byte MOD_MASK = 0xC0; // 11000000b
    public const byte REG_MASK = 0x38; // 00111000b
    public const byte RM_MASK = 0x07; // 00000111b

    // SIB byte masks
    public const byte SIB_SCALE_MASK = 0xC0; // 11000000b
    public const byte SIB_INDEX_MASK = 0x38; // 00111000b
    public const byte SIB_BASE_MASK = 0x07; // 00000111b

    // Register names for different sizes
    public static readonly string[] RegisterNames16 = {"ax", "cx", "dx", "bx", "sp", "bp", "si", "di"};
    public static readonly string[] RegisterNames32 = {"eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi"};

}