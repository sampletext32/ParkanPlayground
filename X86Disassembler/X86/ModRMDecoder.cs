namespace X86Disassembler.X86;

/// <summary>
/// Handles decoding of ModR/M bytes in x86 instructions
/// </summary>
public class ModRMDecoder
{
    // ModR/M byte masks
    private const byte MOD_MASK = 0xC0; // 11000000b
    private const byte REG_MASK = 0x38; // 00111000b
    private const byte RM_MASK = 0x07;  // 00000111b
    
    // SIB byte masks
    private const byte SIB_SCALE_MASK = 0xC0; // 11000000b
    private const byte SIB_INDEX_MASK = 0x38; // 00111000b
    private const byte SIB_BASE_MASK = 0x07;  // 00000111b
    
    // Register names
    private static readonly string[] RegisterNames8 = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
    private static readonly string[] RegisterNames16 = { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di" };
    private static readonly string[] RegisterNames32 = { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi" };
    
    // Buffer containing the code to decode
    private readonly byte[] _codeBuffer;
    
    // The instruction decoder that owns this ModRM decoder
    private readonly InstructionDecoder _decoder;
    
    // Length of the buffer
    private readonly int _length;
    
    /// <summary>
    /// Initializes a new instance of the ModRMDecoder class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this ModRM decoder</param>
    /// <param name="length">The length of the buffer</param>
    public ModRMDecoder(byte[] codeBuffer, InstructionDecoder decoder, int length)
    {
        _codeBuffer = codeBuffer;
        _decoder = decoder;
        _length = length;
    }
    
    /// <summary>
    /// Decodes a ModR/M byte to get the operand string
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rm">The r/m field (3 bits)</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The operand string</returns>
    public string DecodeModRM(byte mod, byte rm, bool is64Bit)
    {
        string sizePrefix = is64Bit ? "qword" : "dword";
        int position = _decoder.GetPosition();
        
        switch (mod)
        {
            case 0: // [reg] or disp32
                if (rm == 5) // disp32
                {
                    if (position + 4 <= _length)
                    {
                        uint disp32 = BitConverter.ToUInt32(_codeBuffer, position);
                        _decoder.SetPosition(position + 4);
                        return $"{sizePrefix} ptr [0x{disp32:X8}]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else if (rm == 4) // SIB
                {
                    // Handle SIB byte
                    if (position < _length)
                    {
                        byte sib = _codeBuffer[position];
                        _decoder.SetPosition(position + 1);
                        return DecodeSIB(sib, 0, is64Bit);
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
                    if (position + 1 < _length)
                    {
                        byte sib = _codeBuffer[position];
                        sbyte disp8 = (sbyte)_codeBuffer[position + 1];
                        _decoder.SetPosition(position + 2);
                        return DecodeSIB(sib, disp8, is64Bit);
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else
                {
                    if (position < _length)
                    {
                        sbyte disp8 = (sbyte)_codeBuffer[position];
                        _decoder.SetPosition(position + 1);
                        string dispStr8 = disp8 < 0 ? $"-0x{-disp8:X2}" : $"+0x{disp8:X2}";
                        return $"{sizePrefix} ptr [{RegisterNames32[rm]}{dispStr8}]";
                    }
                    return $"{sizePrefix} ptr [{RegisterNames32[rm]}+???]";
                }
                
            case 2: // [reg + disp32]
                if (rm == 4) // SIB + disp32
                {
                    // Handle SIB byte
                    if (position + 4 < _length)
                    {
                        byte sib = _codeBuffer[position];
                        int disp32 = BitConverter.ToInt32(_codeBuffer, position + 1);
                        _decoder.SetPosition(position + 5);
                        return DecodeSIB(sib, disp32, is64Bit);
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                else
                {
                    if (position + 4 <= _length)
                    {
                        int disp32 = BitConverter.ToInt32(_codeBuffer, position);
                        _decoder.SetPosition(position + 4);
                        string dispStr32 = disp32 < 0 ? $"-0x{-disp32:X8}" : $"+0x{disp32:X8}";
                        return $"{sizePrefix} ptr [{RegisterNames32[rm]}{dispStr32}]";
                    }
                    return $"{sizePrefix} ptr [{RegisterNames32[rm]}+???]";
                }
                
            case 3: // reg
                return is64Bit ? "mm" + rm : RegisterNames32[rm];
                
            default:
                return "???";
        }
    }
    
    /// <summary>
    /// Reads and decodes a ModR/M byte
    /// </summary>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand string</returns>
    public (byte mod, byte reg, byte rm, string operand) ReadModRM(bool is64Bit = false)
    {
        int position = _decoder.GetPosition();
        
        if (position >= _length)
        {
            return (0, 0, 0, "???");
        }
        
        byte modRM = _codeBuffer[position];
        _decoder.SetPosition(position + 1);
        
        byte mod = (byte)((modRM & MOD_MASK) >> 6);
        byte reg = (byte)((modRM & REG_MASK) >> 3);
        byte rm = (byte)(modRM & RM_MASK);
        
        string operand = DecodeModRM(mod, rm, is64Bit);
        
        return (mod, reg, rm, operand);
    }
    
    /// <summary>
    /// Decodes a SIB byte
    /// </summary>
    /// <param name="sib">The SIB byte</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The decoded SIB string</returns>
    private string DecodeSIB(byte sib, int displacement, bool is64Bit)
    {
        string sizePrefix = is64Bit ? "qword" : "dword";
        int position = _decoder.GetPosition();
        
        byte scale = (byte)((sib & SIB_SCALE_MASK) >> 6);
        byte index = (byte)((sib & SIB_INDEX_MASK) >> 3);
        byte @base = (byte)(sib & SIB_BASE_MASK);
        
        // Special case: no index register
        if (index == 4)
        {
            if (@base == 5 && displacement == 0) // Special case: disp32 only
            {
                if (position + 4 <= _length)
                {
                    uint disp32 = BitConverter.ToUInt32(_codeBuffer, position);
                    _decoder.SetPosition(position + 4);
                    return $"{sizePrefix} ptr [0x{disp32:X8}]";
                }
                return $"{sizePrefix} ptr [???]";
            }
            else
            {
                string baseDispStr = "";
                if (displacement != 0)
                {
                    baseDispStr = displacement < 0 ? 
                        $"-0x{-displacement:X}" : 
                        $"+0x{displacement:X}";
                }
                return $"{sizePrefix} ptr [{RegisterNames32[@base]}{baseDispStr}]";
            }
        }
        
        // Normal case with index register
        int scaleFactor = 1 << scale; // 1, 2, 4, or 8
        string scaleStr = scaleFactor > 1 ? $"*{scaleFactor}" : "";
        
        string indexDispStr = "";
        if (displacement != 0)
        {
            indexDispStr = displacement < 0 ? 
                $"-0x{-displacement:X}" : 
                $"+0x{displacement:X}";
        }
        
        return $"{sizePrefix} ptr [{RegisterNames32[@base]}+{RegisterNames32[index]}{scaleStr}{indexDispStr}]";
    }
    
    /// <summary>
    /// Gets the register name based on the register index and size
    /// </summary>
    /// <param name="index">The register index</param>
    /// <param name="size">The register size (8, 16, or 32 bits)</param>
    /// <returns>The register name</returns>
    public static string GetRegisterName(int index, int size)
    {
        return size switch
        {
            8 => RegisterNames8[index],
            16 => RegisterNames16[index],
            _ => RegisterNames32[index]
        };
    }
}
