namespace X86Disassembler.X86;

using Operands;

/// <summary>
/// Handles decoding of ModR/M bytes in x86 instructions
/// </summary>
public class ModRMDecoder
{
    // The instruction decoder that owns this ModRM decoder
    private readonly InstructionDecoder _decoder;
    
    // The SIB decoder for handling SIB bytes
    private readonly SIBDecoder _sibDecoder;

    /// <summary>
    /// Initializes a new instance of the ModRMDecoder class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this ModRM decoder</param>
    public ModRMDecoder(InstructionDecoder decoder)
    {
        _decoder = decoder;
        _sibDecoder = new SIBDecoder(decoder);
    }

    /// <summary>
    /// Decodes a ModR/M byte to get the operand
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <param name="is64Bit">True if the operand is 64-bit</param>
    /// <returns>The operand object</returns>
    public Operand DecodeModRM(byte mod, RegisterIndex rmIndex, bool is64Bit) => DecodeModRMInternal(mod, rmIndex, is64Bit ? 64 : 32);

    /// <summary>
    /// Decodes a ModR/M byte to get an 8-bit operand
    /// </summary>
    /// <param name="mod">The mod field (2 bits)</param>
    /// <param name="rmIndex">The r/m field as RegisterIndex</param>
    /// <returns>The 8-bit operand object</returns>
    public Operand DecodeModRM8(byte mod, RegisterIndex rmIndex) => DecodeModRMInternal(mod, rmIndex, 8);

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
                        return _sibDecoder.DecodeSIB(sib, 0, operandSize);
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
                        return _sibDecoder.DecodeSIB(sib, (uint)disp8, operandSize);
                    }

                    // Fallback for incomplete data
                    return OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Sp, operandSize);
                }
                else
                {
                    if (_decoder.CanReadByte())
                    {
                        sbyte disp8 = (sbyte)_decoder.ReadByte();

                        // Always create a displacement memory operand for mod=1, even if displacement is 0
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
                        return _sibDecoder.DecodeSIB(sib, disp32, operandSize);
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
                            // Cast to long to preserve the unsigned value for large displacements
                            return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, (long)disp32, operandSize);
                        }

                        // Only show displacement if it's not zero
                        if (disp32 == 0)
                        {
                            return OperandFactory.CreateBaseRegisterMemoryOperand(rmIndex, operandSize);
                        }

                        // Cast to long to preserve the unsigned value for large displacements
                        return OperandFactory.CreateDisplacementMemoryOperand(rmIndex, (long)disp32, operandSize);
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
        byte regIndex = (byte)((modRM & Constants.REG_MASK) >> 3);  // Middle 3 bits (bits 3-5)

        return regIndex;
    }

    /// <summary>
    /// Reads and decodes a ModR/M byte for standard 32-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM() => ReadModRMInternal(false);

    /// <summary>
    /// Reads and decodes a ModR/M byte for 64-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex reg, RegisterIndex rm, Operand operand) ReadModRM64() => ReadModRMInternal(true);

    /// <summary>
    /// Reads and decodes a ModR/M byte for 8-bit operands
    /// </summary>
    /// <returns>A tuple containing the mod, reg, rm fields and the decoded operand</returns>
    public (byte mod, RegisterIndex8 reg, RegisterIndex8 rm, Operand operand) ReadModRM8() => ReadModRM8Internal();

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
        byte mod = (byte)((modRM & Constants.MOD_MASK) >> 6);
        byte regIndex = (byte)((modRM & Constants.REG_MASK) >> 3);
        byte rmIndex = (byte)(modRM & Constants.RM_MASK);
        
        // Map the ModR/M register indices to RegisterIndex enum values
        RegisterIndex reg = RegisterMapper.MapModRMToRegisterIndex(regIndex);
        RegisterIndex rm = RegisterMapper.MapModRMToRegisterIndex(rmIndex);

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
        byte mod = (byte)((modRM & Constants.MOD_MASK) >> 6);
        byte regIndex = (byte)((modRM & Constants.REG_MASK) >> 3);
        byte rmIndex = (byte)(modRM & Constants.RM_MASK);
        
        // Map the ModR/M register indices to RegisterIndex8 enum values
        RegisterIndex8 reg = RegisterMapper.MapModRMToRegisterIndex8(regIndex);
        RegisterIndex8 rm = RegisterMapper.MapModRMToRegisterIndex8(rmIndex);

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
            RegisterIndex rmRegIndex = RegisterMapper.MapModRMToRegisterIndex(rmIndex);
            
            // Use the DecodeModRM8 method to get an 8-bit memory operand
            operand = DecodeModRM8(mod, rmRegIndex);
        }

        return (mod, reg, rm, operand);
    }
}