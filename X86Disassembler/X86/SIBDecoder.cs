namespace X86Disassembler.X86;

using Operands;

/// <summary>
/// Handles decoding of SIB (Scale-Index-Base) bytes in x86 instructions
/// </summary>
public class SIBDecoder
{
    private readonly InstructionDecoder _decoder;
    
    /// <summary>
    /// Initializes a new instance of the SIBDecoder class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this SIB decoder</param>
    public SIBDecoder(InstructionDecoder decoder)
    {
        _decoder = decoder;
    }
    
    /// <summary>
    /// Decodes a SIB byte
    /// </summary>
    /// <param name="sib">The SIB byte</param>
    /// <param name="displacement">The displacement value</param>
    /// <param name="operandSize">The size of the operand in bits (8, 16, 32, or 64)</param>
    /// <param name="mod">The mod field from the ModR/M byte</param>
    /// <returns>The decoded SIB operand</returns>
    public Operand DecodeSIB(byte sib, uint displacement, int operandSize, byte mod = 0)
    {
        // Extract fields from SIB byte
        byte scale = (byte)((sib & Constants.SIB_SCALE_MASK) >> 6);
        int indexIndex = (sib & Constants.SIB_INDEX_MASK) >> 3;
        int baseIndex = sib & Constants.SIB_BASE_MASK;
        
        // Map the SIB register indices to RegisterIndex enum values
        RegisterIndex index = RegisterMapper.MapModRMToRegisterIndex(indexIndex);
        RegisterIndex @base = RegisterMapper.MapModRMToRegisterIndex(baseIndex);

        // Special case: ESP/SP (4) in index field means no index register
        if (index == RegisterIndex.Sp)
        {
            // Special case: EBP/BP (5) in base field with mod=00 means disp32 only
            if (@base == RegisterIndex.Bp && mod == 0)
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
            // Always include the displacement, even if it's zero, to match the encoding
            return OperandFactory.CreateDisplacementMemoryOperand(@base, (int)displacement, operandSize);
        }

        // Special case: EBP/BP (5) in base field with mod=00 means disp32 only
        if (@base == RegisterIndex.Bp && mod == 0 && displacement == 0)
        {
            if (_decoder.CanReadUInt())
            {
                // For other instructions, read the 32-bit displacement
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
                    null,   // No base register
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

        // Special case: When base is EBP/BP and mod is 01 or 10
        // This is a special case in x86 addressing.
        if (@base == RegisterIndex.Bp && (mod == 1 || mod == 2))
        {
            int scaleFactorBp = 1 << scale; // 1, 2, 4, or 8

            // Always include the displacement for EBP, even if it's zero
            // This ensures we show exactly what's encoded in the ModR/M and SIB bytes
            return OperandFactory.CreateScaledIndexMemoryOperand(
                index,
                scaleFactorBp,
                @base,
                (int)displacement,
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
}
