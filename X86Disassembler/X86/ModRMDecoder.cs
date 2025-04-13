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
    
    // Register names for different sizes
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
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The operand string</returns>
    public string DecodeModRM(byte mod, RegisterIndex rmIndex, bool is64Bit)
    {
        string sizePrefix = is64Bit ? "qword" : "dword";
        int position = _decoder.GetPosition();
        
        switch (mod)
        {
            case 0: // [reg] or disp32
                // Special case: [EBP] is encoded as disp32 with no base register
                if (rmIndex == RegisterIndex.Di) // disp32 (was EBP/BP)
                {
                    if (position + 4 <= _length)
                    {
                        uint disp32 = BitConverter.ToUInt32(_codeBuffer, position);
                        _decoder.SetPosition(position + 4);
                        return $"{sizePrefix} ptr [0x{disp32:X8}]";
                    }
                    return $"{sizePrefix} ptr [???]";
                }
                // Special case: [ESP] is encoded with SIB byte
                else if (rmIndex == RegisterIndex.Si) // SIB (was ESP/SP)
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
                    // Regular case: [reg]
                    return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}]";
                }
                
            case 1: // [reg + disp8]
                if (rmIndex == RegisterIndex.Si) // SIB + disp8 (was ESP/SP)
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
                        
                        // Only show displacement if it's not zero
                        if (disp8 == 0)
                        {
                            return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}]";
                        }
                        
                        string dispStr8 = disp8 < 0 ? $"-0x{-disp8:X2}" : $"+0x{disp8:X2}";
                        return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}{dispStr8}]";
                    }
                    return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}+???]";
                }
                
            case 2: // [reg + disp32]
                if (rmIndex == RegisterIndex.Si) // SIB + disp32 (was ESP/SP)
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
                        
                        // Only show displacement if it's not zero
                        if (disp32 == 0)
                        {
                            return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}]";
                        }
                        
                        string dispStr32 = disp32 < 0 ? $"-0x{-disp32:X8}" : $"+0x{disp32:X8}";
                        return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}{dispStr32}]";
                    }
                    return $"{sizePrefix} ptr [{GetRegisterName(rmIndex, 32)}+???]";
                }
                
            case 3: // reg (direct register access)
                return is64Bit ? $"mm{(int)rmIndex}" : GetRegisterName(rmIndex, 32);
                
            default:
                return "???";
        }
    }
    
    /// <summary>
    /// Reads and decodes a ModR/M byte
    /// </summary>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand string</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, string operand) ReadModRM(bool is64Bit = false)
    {
        int position = _decoder.GetPosition();
        
        if (position >= _length)
        {
            return (0, RegisterIndex.A, RegisterIndex.A, "???");
        }
        
        byte modRM = _codeBuffer[position];
        _decoder.SetPosition(position + 1);
        
        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & MOD_MASK) >> 6);
        RegisterIndex reg = (RegisterIndex)((modRM & REG_MASK) >> 3);
        RegisterIndex rm = (RegisterIndex)(modRM & RM_MASK);
        
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
        
        // Extract fields from SIB byte
        byte scale = (byte)((sib & SIB_SCALE_MASK) >> 6);
        RegisterIndex index = (RegisterIndex)((sib & SIB_INDEX_MASK) >> 3);
        RegisterIndex @base = (RegisterIndex)(sib & SIB_BASE_MASK);
        
        // Special case: ESP/SP (4) in index field means no index register
        if (index == RegisterIndex.Si)
        {
            // Special case: EBP/BP (5) in base field with no displacement means disp32 only
            if (@base == RegisterIndex.Di && displacement == 0)
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
                // Base register only
                // Only show displacement if it's not zero
                if (displacement == 0)
                {
                    return $"{sizePrefix} ptr [{GetRegisterName(@base, 32)}]";
                }
                
                string baseDispStr = displacement < 0 ? 
                    $"-0x{-displacement:X}" : 
                    $"+0x{displacement:X}";
                return $"{sizePrefix} ptr [{GetRegisterName(@base, 32)}{baseDispStr}]";
            }
        }
        
        // Normal case with base and index registers
        int scaleFactor = 1 << scale; // 1, 2, 4, or 8
        
        // Only include the scale factor if it's not 1
        string scaleStr = scaleFactor > 1 ? $"*{scaleFactor}" : "";
        
        // Only show displacement if it's not zero
        if (displacement == 0)
        {
            return $"{sizePrefix} ptr [{GetRegisterName(@base, 32)}+{GetRegisterName(index, 32)}{scaleStr}]";
        }
        
        string indexDispStr = displacement < 0 ? 
            $"-0x{-displacement:X}" : 
            $"+0x{displacement:X}";
        
        return $"{sizePrefix} ptr [{GetRegisterName(@base, 32)}+{GetRegisterName(index, 32)}{scaleStr}{indexDispStr}]";
    }

    /// <summary>
    /// Gets the register name based on the register index and size
    /// </summary>
    /// <param name="regIndex">The register index as RegisterIndex enum</param>
    /// <param name="size">The register size (8, 16, or 32 bits)</param>
    /// <returns>The register name</returns>
    public static string GetRegisterName(RegisterIndex regIndex, int size)
    {
        // Convert RegisterIndex to raw index for array access
        int index = (int)regIndex;
        
        return size switch
        {
            8 => RegisterNames8[index],
            16 => RegisterNames16[index],
            32 => RegisterNames32[index],
            _ => RegisterNames32[index] // Default to 32-bit registers
        };
    }
}
