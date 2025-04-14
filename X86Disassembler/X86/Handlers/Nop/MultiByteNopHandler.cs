namespace X86Disassembler.X86.Handlers.Nop;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for multi-byte NOP instructions (0x0F 0x1F ...)
/// These are used for alignment and are encoded as NOP operations with specific memory operands
/// </summary>
public class MultiByteNopHandler : InstructionHandler
{
    // NOP variant information (ModR/M byte, expected bytes pattern, and operand creation info)
    private static readonly (byte ModRm, byte[] ExpectedBytes, RegisterIndex BaseReg, RegisterIndex? IndexReg, int Scale)[] NopVariants =
    {
        // 8-byte NOP: 0F 1F 84 00 00 00 00 00 (check longest patterns first)
        (0x84, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, RegisterIndex.A, RegisterIndex.A, 1),
        
        // 7-byte NOP: 0F 1F 80 00 00 00 00
        (0x80, new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegisterIndex.A, null, 0),
        
        // 6-byte NOP: 0F 1F 44 00 00 00
        (0x44, new byte[] { 0x00, 0x00, 0x00 }, RegisterIndex.A, RegisterIndex.A, 1),
        
        // 5-byte NOP: 0F 1F 44 00 00
        (0x44, new byte[] { 0x00, 0x00 }, RegisterIndex.A, RegisterIndex.A, 1),
        
        // 4-byte NOP: 0F 1F 40 00
        (0x40, new byte[] { 0x00 }, RegisterIndex.A, null, 0),
        
        // 3-byte NOP: 0F 1F 00
        (0x00, Array.Empty<byte>(), RegisterIndex.A, null, 0)
    };

    /// <summary>
    /// Initializes a new instance of the MultiByteNopHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public MultiByteNopHandler(InstructionDecoder decoder)
        : base(decoder)
    {
    }

    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // Multi-byte NOPs start with 0x0F
        if (opcode != 0x0F)
        {
            return false;
        }

        // Check if we have enough bytes to read the second opcode
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the second byte is 0x1F (part of the multi-byte NOP encoding)
        byte secondByte = Decoder.PeakByte();
        return secondByte == 0x1F;
    }

    /// <summary>
    /// Decodes a multi-byte NOP instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Nop;

        // Read the second byte (0x1F)
        Decoder.ReadByte();

        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if we have an operand size prefix (0x66)
        bool hasOperandSizePrefix = Decoder.HasOperandSizeOverridePrefix();
        
        // Determine the size of the operand
        int operandSize = hasOperandSizePrefix ? 16 : 32;
        
        // Read the ModR/M byte
        byte modRm = Decoder.ReadByte();
        
        // Default memory operand parameters
        RegisterIndex baseReg = RegisterIndex.A;
        RegisterIndex? indexReg = null;
        int scale = 0;
        
        // Try to find a matching NOP variant (we check longest patterns first)
        foreach (var (variantModRm, expectedBytes, variantBaseReg, variantIndexReg, variantScale) in NopVariants)
        {
            // Skip if ModR/M doesn't match
            if (variantModRm != modRm)
            {
                continue;
            }

            // Check if we have enough bytes for this pattern
            if (!Decoder.CanRead(expectedBytes.Length))
            {
                continue;
            }
            
            // Create a buffer to read the expected bytes
            byte[] buffer = new byte[expectedBytes.Length];
            
            // Read the bytes into the buffer without advancing the decoder position
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                if (!Decoder.CanReadByte())
                {
                    break;
                }
                buffer[i] = Decoder.PeakByte(i);
            }
            
            // Check if the expected bytes match
            bool isMatch = true;
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                if (buffer[i] != expectedBytes[i])
                {
                    isMatch = false;
                    break;
                }
            }
            
            // If we found a match, use it and stop checking
            if (isMatch)
            {
                baseReg = variantBaseReg;
                indexReg = variantIndexReg;
                scale = variantScale;
                
                // Consume the expected bytes
                for (int i = 0; i < expectedBytes.Length; i++)
                {
                    Decoder.ReadByte();
                }
                
                break;
            }
        }
        
        // Create the appropriate structured operand based on the NOP variant
        if (indexReg.HasValue && scale > 0)
        {
            // Create a scaled index memory operand (e.g., [eax+eax*1])
            instruction.StructuredOperands = 
            [
                OperandFactory.CreateScaledIndexMemoryOperand(
                    indexReg.Value, 
                    scale, 
                    baseReg, 
                    0, 
                    operandSize)
            ];
        }
        else
        {
            // Create a simple base register memory operand (e.g., [eax])
            instruction.StructuredOperands = 
            [
                OperandFactory.CreateBaseRegisterMemoryOperand(
                    baseReg, 
                    operandSize)
            ];
        }
        
        return true;
    }
}