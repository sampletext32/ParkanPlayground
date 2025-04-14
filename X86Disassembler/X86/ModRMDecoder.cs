namespace X86Disassembler.X86;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handles decoding of ModR/M bytes in x86 instructions
/// </summary>
public class ModRMDecoder
{
    // ModR/M byte masks
    private const byte MOD_MASK = 0xC0; // 11000000b
    private const byte REG_MASK = 0x38; // 00111000b
    private const byte RM_MASK = 0x07; // 00000111b

    // SIB byte masks
    private const byte SIB_SCALE_MASK = 0xC0; // 11000000b
    private const byte SIB_INDEX_MASK = 0x38; // 00111000b
    private const byte SIB_BASE_MASK = 0x07; // 00000111b

    // Register names for different sizes
    private static readonly string[] RegisterNames8 = {"al", "cl", "dl", "bl", "ah", "ch", "dh", "bh"};
    private static readonly string[] RegisterNames16 = {"ax", "cx", "dx", "bx", "sp", "bp", "si", "di"};
    private static readonly string[] RegisterNames32 = {"eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi"};

    // The instruction decoder that owns this ModRM decoder
    private readonly InstructionDecoder _decoder;

    /// <summary>
    /// Initializes a new instance of the ModRMDecoder class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this ModRM decoder</param>
    public ModRMDecoder(InstructionDecoder decoder)
    {
        _decoder = decoder;
    }

    /// <summary>
    /// Decodes a ModR/M byte to get the operand
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The operand object</returns>
    public Operand DecodeModRM(byte mod, RegisterIndex rmIndex, bool is64Bit)
    {
        int operandSize = is64Bit ? 64 : 32;

        switch (mod)
        {
            case 0: // [reg] or disp32
                // Special case: [EBP] is encoded as disp32 with no base register
                if (rmIndex == RegisterIndex.Di) // disp32 (was EBP/BP)
                {
                    if (_decoder.CanReadUInt())
                    {
                        uint disp32 = _decoder.ReadUInt32();
                        return OperandFactory.CreateDirectMemoryOperand(disp32, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateDirectMemoryOperand(0, operandSize);
                }

                // Special case: [ESP] is encoded with SIB byte
                if (rmIndex == RegisterIndex.Si) // SIB (was ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadByte())
                    {
                        byte sib = _decoder.ReadByte();
                        return DecodeSIB(sib, 0, is64Bit);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, operandSize);
                }

                // Regular case: [reg]
                return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);

            case 1: // [reg + disp8]
                if (rmIndex == RegisterIndex.Si) // SIB + disp8 (was ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadByte())
                    {
                        byte sib = _decoder.ReadByte();
                        sbyte disp8 = (sbyte)(_decoder.CanReadByte() ? _decoder.ReadByte() : 0);
                        return DecodeSIB(sib, (uint)disp8, is64Bit);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, operandSize);
                }
                else
                {
                    if (_decoder.CanReadByte())
                    {
                        sbyte disp8 = (sbyte)_decoder.ReadByte();

                        // Only show displacement if it's not zero
                        if (disp8 == 0)
                        {
                            return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                        }

                        return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, disp8, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                }

            case 2: // [reg + disp32]
                if (rmIndex == RegisterIndex.Si) // SIB + disp32 (was ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadUInt())
                    {
                        byte sib = _decoder.ReadByte();
                        uint disp32 = _decoder.ReadUInt32();
                        return DecodeSIB(sib, disp32, is64Bit);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, operandSize);
                }
                else
                {
                    if (_decoder.CanReadUInt())
                    {
                        uint disp32 = _decoder.ReadUInt32();

                        // Only show displacement if it's not zero
                        if (disp32 == 0)
                        {
                            return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                        }

                        return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, (int)disp32, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                }

            case 3: // reg (direct register access)
                return OperandFactory.CreateRegisterOperand(rmIndex, operandSize);

            default:
                // Fallback for invalid mod value
                return OperandFactory.CreateRegisterOperand(RegisterIndex.A, operandSize);
        }
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte
    /// </summary>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM(bool is64Bit = false)
    {
        if (!_decoder.CanReadByte())
        {
            return (0, RegisterIndex.A, RegisterIndex.A, OperandFactory.CreateRegisterOperand(RegisterIndex.A, is64Bit ? 64 : 32));
        }

        byte modRM = _decoder.ReadByte();

        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & MOD_MASK) >> 6);
        RegisterIndex reg = (RegisterIndex)((modRM & REG_MASK) >> 3);
        RegisterIndex rm = (RegisterIndex)(modRM & RM_MASK);

        Operand operand = DecodeModRM(mod, rm, is64Bit);

        return (mod, reg, rm, operand);
    }

    /// <summary>
    /// Decodes a SIB byte
    /// </summary>
    /// <param name="sib">The SIB byte</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The decoded SIB operand</returns>
    private Operand DecodeSIB(byte sib, uint displacement, bool is64Bit)
    {
        int operandSize = is64Bit ? 64 : 32;

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
                if (_decoder.CanReadUInt())
                {
                    uint disp32 = _decoder.ReadUInt32();
                    return OperandFactory.CreateDirectMemoryOperand(disp32, operandSize);
                }

                // Fallback for incomplete data
                return OperandFactory.CreateDirectMemoryOperand(0, operandSize);
            }

            // Base register only with displacement
            if (displacement == 0)
            {
                return OperandFactory.CreateBaseRegisterMemoryOperand(@base, operandSize);
            }

            return OperandFactory.CreateDisplacementMemoryOperand(@base, (int)displacement, operandSize);
        }

        // Normal case with base and index registers
        int scaleFactor = 1 << scale; // 1, 2, 4, or 8

        // Create a scaled index memory operand
        return OperandFactory.CreateScaledIndexMemoryOperand(
            index,
            scaleFactor,
            @base,
            (int)displacement,
            operandSize);
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