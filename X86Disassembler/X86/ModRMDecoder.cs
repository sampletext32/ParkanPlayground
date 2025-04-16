namespace X86Disassembler.X86;

using Operands;

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
    /// Maps the register index from the ModR/M byte to the RegisterIndex enum value
    /// </summary>
    /// <param name="modRMRegIndex">The register index from the ModR/M byte (0-7)</param>
    /// <returns>The corresponding RegisterIndex enum value</returns>
    private RegisterIndex MapModRMToRegisterIndex(int modRMRegIndex)
    {
        // The mapping from ModR/M register index to RegisterIndex enum is:
        // 0 -> A (EAX)
        // 1 -> C (ECX)
        // 2 -> D (EDX)
        // 3 -> B (EBX)
        // 4 -> Sp (ESP)
        // 5 -> Bp (EBP)
        // 6 -> Si (ESI)
        // 7 -> Di (EDI)
        return modRMRegIndex switch
        {
            0 => RegisterIndex.A,  // EAX
            1 => RegisterIndex.C,  // ECX
            2 => RegisterIndex.D,  // EDX
            3 => RegisterIndex.B,  // EBX
            4 => RegisterIndex.Sp, // ESP
            5 => RegisterIndex.Bp, // EBP
            6 => RegisterIndex.Si, // ESI
            7 => RegisterIndex.Di, // EDI
            _ => RegisterIndex.A   // Default to EAX
        };
    }

    /// <summary>
    /// Maps the register index from the ModR/M byte to the RegisterIndex8 enum value
    /// </summary>
    /// <param name="modRMRegIndex">The register index from the ModR/M byte (0-7)</param>
    /// <returns>The corresponding RegisterIndex8 enum value</returns>
    private RegisterIndex8 MapModRMToRegisterIndex8(int modRMRegIndex)
    {
        // The mapping from ModR/M register index to RegisterIndex8 enum is direct:
        // 0 -> AL, 1 -> CL, 2 -> DL, 3 -> BL, 4 -> AH, 5 -> CH, 6 -> DH, 7 -> BH
        return (RegisterIndex8)modRMRegIndex;
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
        return DecodeModRMInternal(mod, rmIndex, is64Bit ? 64 : 32);
    }
    
    /// <summary>
    /// Decodes a ModR/M byte to get an 8-bit operand
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <returns>The 8-bit operand object</returns>
    public Operand DecodeModRM8(byte mod, RegisterIndex rmIndex)
    {
        return DecodeModRMInternal(mod, rmIndex, 8);
    }
    
    /// <summary>
    /// Decodes a ModR/M byte to get a 16-bit operand
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <returns>The 16-bit operand object</returns>
    public Operand DecodeModRM16(byte mod, RegisterIndex rmIndex)
    {
        return DecodeModRMInternal(mod, rmIndex, 16);
    }
    
    /// <summary>
    /// Internal implementation for decoding a ModR/M byte to get an operand with specific size
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <param name="operandSize">The size of the operand in bits (8, 16, 32, or 64)</param>
    /// <returns>The operand object</returns>
    private Operand DecodeModRMInternal(byte mod, RegisterIndex rmIndex, int operandSize)
    {

        switch (mod)
        {
            case 0: // [reg] or disp32
                // Special case: [EBP] is encoded as disp32 with no base register
                if (rmIndex == RegisterIndex.Bp) // disp32 (was EBP/BP)
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
                if (rmIndex == RegisterIndex.Sp) // SIB (was ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadByte())
                    {
                        byte sib = _decoder.ReadByte();
                        return DecodeSIB(sib, 0, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Sp, operandSize);
                }

                // Regular case: [reg]
                return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);

            case 1: // [reg + disp8]
                if (rmIndex == RegisterIndex.Sp) // SIB + disp8 (ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadByte())
                    {
                        byte sib = _decoder.ReadByte();
                        sbyte disp8 = (sbyte)(_decoder.CanReadByte() ? _decoder.ReadByte() : 0);
                        return DecodeSIB(sib, (uint)disp8, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Sp, operandSize);
                }
                else
                {
                    if (_decoder.CanReadByte())
                    {
                        sbyte disp8 = (sbyte)_decoder.ReadByte();

                        // For EBP (BP), always create a displacement memory operand, even if displacement is 0
                        // This is because [EBP] with no displacement is encoded as [EBP+0]
                        if (disp8 == 0 && rmIndex != RegisterIndex.Bp)
                        {
                            return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                        }

                        return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, disp8, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                }

            case 2: // [reg + disp32]
                if (rmIndex == RegisterIndex.Sp) // SIB + disp32 (ESP/SP)
                {
                    // Handle SIB byte
                    if (_decoder.CanReadUInt())
                    {
                        byte sib = _decoder.ReadByte();
                        uint disp32 = _decoder.ReadUInt32();
                        return DecodeSIB(sib, disp32, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Sp, operandSize);
                }
                else
                {
                    if (_decoder.CanReadUInt())
                    {
                        uint disp32 = _decoder.ReadUInt32();

                        // For EBP (BP), always create a displacement memory operand, even if displacement is 0
                        // This is because [EBP] with no displacement is encoded as [EBP+disp]
                        if (rmIndex == RegisterIndex.Bp)
                        {
                            return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, (int)disp32, operandSize);
                        }

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
    /// Peaks a ModR/M byte and returns the raw field values, without advancing position
    /// </summary>
    /// <returns>A tuple containing the raw mod, reg, and rm fields from the ModR/M byte</returns>
    public byte PeakModRMReg()
    {
        if (!_decoder.CanReadByte())
        {
            return 0;
        }

        byte modRM = _decoder.PeakByte();

        // Extract fields from ModR/M byte
        byte regIndex = (byte)((modRM & REG_MASK) >> 3);  // Middle 3 bits (bits 3-5)

        return regIndex;
    }

    /// <summary>
    /// Reads a ModR/M byte and returns the raw field values
    /// </summary>
    /// <returns>A tuple containing the raw mod, reg, and rm fields from the ModR/M byte</returns>
    public (byte mod, byte reg, byte rm) ReadModRMRaw()
    {
        if (!_decoder.CanReadByte())
        {
            return (0, 0, 0);
        }

        byte modRM = _decoder.ReadByte();

        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & MOD_MASK) >> 6);  // Top 2 bits (bits 6-7)
        byte regIndex = (byte)((modRM & REG_MASK) >> 3);  // Middle 3 bits (bits 3-5)
        byte rmIndex = (byte)(modRM & RM_MASK);  // Bottom 3 bits (bits 0-2)

        return (mod, regIndex, rmIndex);
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte for standard 32-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM()
    {
        return ReadModRMInternal(false);
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte for 64-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM64()
    {
        return ReadModRMInternal(true);
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte for 8-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex8 reg, RegisterIndex8 rm, Operand operand) ReadModRM8()
    {
        return ReadModRM8Internal();
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte for 16-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM16()
    {
        var (mod, reg, rm, operand) = ReadModRMInternal(false);
        
        // Create a new operand with 16-bit size using the appropriate factory method
        if (operand is RegisterOperand registerOperand)
        {
            // For register operands, create a new 16-bit register operand
            operand = OperandFactory.CreateRegisterOperand(registerOperand.Register, 16);
        }
        else if (operand is MemoryOperand)
        {
            // For memory operands, create a new 16-bit memory operand with the same properties
            // This depends on the specific type of memory operand
            if (operand is DirectMemoryOperand directMemory)
            {
                operand = OperandFactory.CreateDirectMemoryOperand16(directMemory.Address);
            }
            else if (operand is BaseRegisterMemoryOperand baseRegMemory)
            {
                operand = OperandFactory.CreateBaseRegisterMemoryOperand16(baseRegMemory.BaseRegister);
            }
            else if (operand is DisplacementMemoryOperand dispMemory)
            {
                operand = OperandFactory.CreateDisplacementMemoryOperand16(dispMemory.BaseRegister, dispMemory.Displacement);
            }
            else if (operand is ScaledIndexMemoryOperand scaledMemory)
            {
                operand = OperandFactory.CreateScaledIndexMemoryOperand16(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement);
            }
        }
        
        return (mod, reg, rm, operand);
    }

    /// <summary>
    /// Internal implementation for reading and decoding a ModR/M byte for standard 32-bit or 64-bit operands
    /// </summary>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    private (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRMInternal(bool is64Bit)
    {
        if (!_decoder.CanReadByte())
        {
            return (0, RegisterIndex.A, RegisterIndex.A, OperandFactory.CreateRegisterOperand(RegisterIndex.A, is64Bit ? 64 : 32));
        }

        byte modRM = _decoder.ReadByte();

        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & MOD_MASK) >> 6);
        byte regIndex = (byte)((modRM & REG_MASK) >> 3);
        byte rmIndex = (byte)(modRM & RM_MASK);
        
        // Map the ModR/M register indices to RegisterIndex enum values
        RegisterIndex reg = MapModRMToRegisterIndex(regIndex);
        RegisterIndex rm = MapModRMToRegisterIndex(rmIndex);

        // Create the operand based on the mod and rm fields
        Operand operand = DecodeModRM(mod, rm, is64Bit);

        return (mod, reg, rm, operand);
    }
    
    /// <summary>
    /// Internal implementation for reading and decoding a ModR/M byte for 8-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    private (byte mod, RegisterIndex8 reg, RegisterIndex8 rm, Operand operand) ReadModRM8Internal()
    {
        if (!_decoder.CanReadByte())
        {
            return (0, RegisterIndex8.AL, RegisterIndex8.AL, OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL));
        }

        byte modRM = _decoder.ReadByte();

        // Extract fields from ModR/M byte
        byte mod = (byte)((modRM & MOD_MASK) >> 6);
        byte regIndex = (byte)((modRM & REG_MASK) >> 3);
        byte rmIndex = (byte)(modRM & RM_MASK);
        
        // Map the ModR/M register indices to RegisterIndex8 enum values
        RegisterIndex8 reg = MapModRMToRegisterIndex8(regIndex);
        RegisterIndex8 rm = MapModRMToRegisterIndex8(rmIndex);

        // Create the operand based on the mod and rm fields
        Operand operand;
        
        if (mod == 3) // Register operand
        {
            // For register operands, create an 8-bit register operand
            operand = OperandFactory.CreateRegisterOperand8(rm);
        }
        else // Memory operand
        {
            // For memory operands, we need to map the RegisterIndex8 to RegisterIndex for base registers
            // The rmIndex is the raw value from the ModR/M byte, not the mapped RegisterIndex8
            // This is important because we need to check if it's 4 (ESP) for SIB byte
            RegisterIndex rmRegIndex = MapModRMToRegisterIndex(rmIndex);
            
            // Use the DecodeModRM8 method to get an 8-bit memory operand
            operand = DecodeModRM8(mod, rmRegIndex);
        }

        return (mod, reg, rm, operand);
    }

    /// <summary>
    /// Decodes a SIB byte
    /// </summary>
    /// <param name="sib">The SIB byte</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="operandSize">The size of the operand in bits (8, 16, 32, or 64)</param>
    /// <returns>The decoded SIB operand</returns>
    private Operand DecodeSIB(byte sib, uint displacement, int operandSize)
    {

        // Extract fields from SIB byte
        byte scale = (byte)((sib & SIB_SCALE_MASK) >> 6);
        int indexIndex = (sib & SIB_INDEX_MASK) >> 3;
        int baseIndex = sib & SIB_BASE_MASK;
        
        // Map the SIB register indices to RegisterIndex enum values
        RegisterIndex index = MapModRMToRegisterIndex(indexIndex);
        RegisterIndex @base = MapModRMToRegisterIndex(baseIndex);

        // Special case: ESP/SP (4) in index field means no index register
        if (index == RegisterIndex.Sp)
        {
            // Special case: EBP/BP (5) in base field with no displacement means disp32 only
            if (@base == RegisterIndex.Bp && displacement == 0)
            {
                if (_decoder.CanReadUInt())
                {
                    uint disp32 = _decoder.ReadUInt32();
                    
                    // When both index is ESP (no index) and base is EBP with disp32,
                    // this is a direct memory reference [disp32]
                    return OperandFactory.CreateDirectMemoryOperand(disp32, operandSize);
                }

                // Fallback for incomplete data
                return OperandFactory.CreateDirectMemoryOperand(0, operandSize);
            }

            // When index is ESP (no index), we just have a base register with optional displacement
            if (displacement == 0)
            {
                return OperandFactory.CreateBaseRegisterMemoryOperand(@base, operandSize);
            }

            return OperandFactory.CreateDisplacementMemoryOperand(@base, (int)displacement, operandSize);
        }

        // Special case: EBP/BP (5) in base field with no displacement means disp32 only
        if (@base == RegisterIndex.Bp && displacement == 0)
        {
            if (_decoder.CanReadUInt())
            {
                uint disp32 = _decoder.ReadUInt32();
                int scaleValue = 1 << scale; // 1, 2, 4, or 8
                
                // If we have a direct memory reference with a specific displacement,
                // use a direct memory operand instead of a scaled index memory operand
                if (disp32 > 0 && index == RegisterIndex.Sp)
                {
                    return OperandFactory.CreateDirectMemoryOperand(disp32, operandSize);
                }
                
                // Create a scaled index memory operand with displacement but no base register
                return OperandFactory.CreateScaledIndexMemoryOperand(
                    index,
                    scaleValue,
                    null,
                    (int)disp32,
                    operandSize);
            }

            // Fallback for incomplete data
            return OperandFactory.CreateScaledIndexMemoryOperand(
                index,
                1 << scale,
                null,
                0,
                operandSize);
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
    /// <param name="size">The register size (16 or 32 bits)</param>
    /// <returns>The register name</returns>
    public static string GetRegisterName(RegisterIndex regIndex, int size)
    {
        return size switch
        {
            16 => RegisterNames16[(int)regIndex],
            32 => RegisterNames32[(int)regIndex],
            64 => RegisterNames32[(int)regIndex], // For now, reuse 32-bit names for 64-bit
            _ => "unknown"
        };
    }
    
    /// <summary>
    /// Gets the 8-bit register name based on the RegisterIndex8 enum value
    /// </summary>
    /// <param name="regIndex8">The register index as RegisterIndex8 enum</param>
    /// <returns>The 8-bit register name</returns>
    public static string GetRegisterName(RegisterIndex8 regIndex8)
    {
        return regIndex8.ToString().ToLower();
    }
    
    /// <summary>
    /// Maps a RegisterIndex8 enum value to the corresponding RegisterIndex enum value for base registers
    /// </summary>
    /// <param name="regIndex8">The RegisterIndex8 enum value</param>
    /// <returns>The corresponding RegisterIndex enum value</returns>
    private RegisterIndex MapRegister8ToBaseRegister(RegisterIndex8 regIndex8)
    {
        // Map 8-bit register indices to their corresponding 32-bit register indices
        return regIndex8 switch
        {
            RegisterIndex8.AL => RegisterIndex.A,
            RegisterIndex8.CL => RegisterIndex.C,
            RegisterIndex8.DL => RegisterIndex.D,
            RegisterIndex8.BL => RegisterIndex.B,
            RegisterIndex8.AH => RegisterIndex.A,
            RegisterIndex8.CH => RegisterIndex.C,
            RegisterIndex8.DH => RegisterIndex.D,
            RegisterIndex8.BH => RegisterIndex.B,
            _ => RegisterIndex.A // Default to EAX
        };
    }
}